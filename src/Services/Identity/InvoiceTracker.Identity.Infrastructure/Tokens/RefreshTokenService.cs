using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using InvoiceTracker.Identity.Infrastructure.Interfaces;
using InvoiceTracker.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InvoiceTracker.Identity.Infrastructure.Tokens;

public sealed class RefreshTokenService(
    IdentityServiceDbContext context,
    IOptions<JwtOptions> options,
    TimeProvider timeProvider) : IRefreshTokenService
{
    private const int TokenSizeBytes = 32;

    public async Task<IssuedRefreshToken> IssueAsync(Guid userId, CancellationToken cancellationToken)
    {
        var (entity, token) = Create(userId, Guid.CreateVersion7());

        context.RefreshTokens.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return new IssuedRefreshToken(userId, token, entity.ExpiresAt);
    }

    public async Task<IssuedRefreshToken?> RotateAsync(string token, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var hash = Hash(token);

        var current = await context.RefreshTokens
            .AsNoTracking()
            .SingleOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (current is null || current.ExpiresAt <= now)
        {
            return null;
        }

        if (current.RevokedAt is not null)
        {
            // A used token coming back means it was copied: end the session for everyone holding it
            await RevokeFamilyAsync(current.FamilyId, now, cancellationToken);
            return null;
        }

        // two concurrent refreshes with the same token cannot both succeed
        var claimed = await context.RefreshTokens
            .Where(t => t.Id == current.Id && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now),
                cancellationToken);

        if (claimed == 0)
        {
            await RevokeFamilyAsync(current.FamilyId, now, cancellationToken);
            return null;
        }

        // If this insert fails the old token is already revoked and the user must log in again.
        var (next, nextToken) = Create(current.UserId, current.FamilyId);
        context.RefreshTokens.Add(next);
        await context.SaveChangesAsync(cancellationToken);

        return new IssuedRefreshToken(next.UserId, nextToken, next.ExpiresAt);
    }

    public async Task RevokeAsync(string token, CancellationToken cancellationToken)
    {
        var hash = Hash(token);

        var familyId = await context.RefreshTokens
            .Where(t => t.TokenHash == hash)
            .Select(t => (Guid?)t.FamilyId)
            .SingleOrDefaultAsync(cancellationToken);

        if (familyId is null)
        {
            return;
        }

        await RevokeFamilyAsync(familyId.Value, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
    }

    private Task<int> RevokeFamilyAsync(Guid familyId, DateTime now, CancellationToken cancellationToken)
    {
        return context.RefreshTokens
            .Where(t => t.FamilyId == familyId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), cancellationToken);
    }

    private (RefreshToken Entity, string Token) Create(Guid userId, Guid familyId)
    {
        var token = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(TokenSizeBytes));
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var entity = new RefreshToken
        {
            UserId = userId,
            FamilyId = familyId,
            TokenHash = Hash(token),
            CreatedAt = now,
            ExpiresAt = now.AddDays(options.Value.RefreshTokenDays)
        };

        return (entity, token);
    }

    private static string Hash(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
