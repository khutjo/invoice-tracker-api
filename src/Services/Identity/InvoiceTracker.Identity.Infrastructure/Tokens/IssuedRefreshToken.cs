namespace InvoiceTracker.Identity.Infrastructure.Tokens;

/// <summary>The raw token goes to the client once; only its hash is stored.</summary>
public sealed record IssuedRefreshToken(
    Guid UserId,
    string Token,
    DateTimeOffset ExpiresAt
);
