namespace InvoiceTracker.Identity.Infrastructure.Tokens;

public sealed record AccessToken(
    string Token,
    DateTimeOffset ExpiresAt
);
