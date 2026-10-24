using InvoiceTracker.Identity.Infrastructure.Tokens;

namespace InvoiceTracker.Identity.Infrastructure.Interfaces;

public interface IRefreshTokenService
{
    /// <summary>Starts a new session (token family) for a user who just logged in.</summary>
    Task<IssuedRefreshToken> IssueAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Revokes <paramref name="token"/> and returns its replacement, or null if the token is unknown,
    /// expired or already used. Reuse of a revoked token revokes the whole family.
    /// </summary>
    Task<IssuedRefreshToken?> RotateAsync(string token, CancellationToken cancellationToken);

    /// <summary>Ends the session the token belongs to. Unknown tokens are ignored.</summary>
    Task RevokeAsync(string token, CancellationToken cancellationToken);
}
