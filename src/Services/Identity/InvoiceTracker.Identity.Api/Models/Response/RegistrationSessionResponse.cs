namespace InvoiceTracker.Identity.Api.Models.Response;

public sealed record RegistrationSessionResponse
{
    public required string BusinessName
    {
        get; init;
    }

    public required string Email
    {
        get; init;
    }

    public required string RegisterToken
    {
        get; init;
    }

    public required DateTimeOffset ExpiresAt
    {
        get; init;
    }
}
