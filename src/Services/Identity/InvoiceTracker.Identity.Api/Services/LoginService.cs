using InvoiceTracker.Identity.Api.Exceptions;
using InvoiceTracker.Identity.Api.Interfaces;
using InvoiceTracker.Identity.Api.Models.Request;
using InvoiceTracker.Identity.Api.Models.Response;
using InvoiceTracker.Identity.Infrastructure.Interfaces;
using InvoiceTracker.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace InvoiceTracker.Identity.Api.Services;

public sealed class LoginService(
    UserManager<ApplicationUser> userManager,
    IAccessTokenIssuer accessTokenIssuer,
    IRefreshTokenService refreshTokenService,
    TimeProvider timeProvider) : ILoginService
{
    private static readonly ApplicationUser _dummyUser = new();
    private static string? _dummyHash;

    public async Task<LoginResponse> LoginAsync(LogInRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            // Hash the password anyway, so an unknown email takes as long to reject as a wrong password
            userManager.PasswordHasher.VerifyHashedPassword(_dummyUser, GetDummyHash(), request.Password);
            throw InvalidCredentials();
        }

        if (!user.IsActive || await userManager.IsLockedOutAsync(user))
        {
            // Same reason: a locked or deactivated account must not answer faster than a wrong password
            await userManager.CheckPasswordAsync(user, request.Password);
            throw InvalidCredentials();
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            await userManager.AccessFailedAsync(user);
            throw InvalidCredentials();
        }

        await userManager.ResetAccessFailedCountAsync(user);

        if (!user.EmailConfirmed)
        {
            throw new LoginException("Verify your email address before logging in.", StatusCodes.Status403Forbidden);
        }

        user.LastLoginAt = timeProvider.GetUtcNow().UtcDateTime;
        await userManager.UpdateAsync(user);

        var accessToken = accessTokenIssuer.Issue(user);
        var refreshToken = await refreshTokenService.IssueAsync(user.Id, cancellationToken);

        return new LoginResponse
        {
            JwtToken = accessToken.Token,
            RefreshToken = refreshToken.Token,
            ExpiresIn = SecondsUntil(accessToken.ExpiresAt),
            UserId = user.Id,
            BusinessId = user.BusinessId,
            Role = user.Role.ToString()
        };
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var refreshToken = await refreshTokenService.RotateAsync(request.RefreshToken, cancellationToken)
            ?? throw InvalidRefreshToken();

        var user = await userManager.FindByIdAsync(refreshToken.UserId.ToString());
        if (user is null || !user.IsActive)
        {
            await refreshTokenService.RevokeAsync(refreshToken.Token, cancellationToken);
            throw InvalidRefreshToken();
        }

        var accessToken = accessTokenIssuer.Issue(user);

        return new RefreshTokenResponse
        {
            JwtToken = accessToken.Token,
            RefreshToken = refreshToken.Token,
            ExpiresIn = SecondsUntil(accessToken.ExpiresAt)
        };
    }

    public Task LogoutAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        return refreshTokenService.RevokeAsync(request.RefreshToken, cancellationToken);
    }

    private string GetDummyHash()
    {
        // Built once with the configured hasher, so it costs the same iterations as a real hash
        return _dummyHash ??= userManager.PasswordHasher.HashPassword(_dummyUser, Guid.NewGuid().ToString());
    }

    private int SecondsUntil(DateTimeOffset expiresAt)
    {
        return (int)(expiresAt - timeProvider.GetUtcNow()).TotalSeconds;
    }

    private static LoginException InvalidCredentials()
    {
        return new LoginException("Invalid email or password.", StatusCodes.Status401Unauthorized);
    }

    private static LoginException InvalidRefreshToken()
    {
        return new LoginException("Refresh token is invalid or expired.", StatusCodes.Status401Unauthorized);
    }
}
