namespace InvoiceTracker.Identity.Api.Models.Response;

public sealed record RefreshTokenResponse
{
    public required string JwtToken
    {
        get; init;
    }

    public required string RefreshToken
    {
        get; init;
    }

    public required int ExpiresIn
    {
        get; init;
    }
}
