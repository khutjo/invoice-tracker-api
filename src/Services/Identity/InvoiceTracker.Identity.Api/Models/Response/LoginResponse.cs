namespace InvoiceTracker.Identity.Api.Models.Response;

public sealed record LoginResponse
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

    public required Guid UserId
    {
        get; init;
    }

    public required Guid BusinessId
    {
        get; init;
    }

    public required string Role
    {
        get; init;
    }
}
