namespace InvoiceTracker.Identity.Api.Models.Response;

public sealed record RegisterUserResponse
{
    public required Guid UserId
    {
        get; init;
    }

    public required Guid BusinessId
    {
        get; init;
    }

    public required string Email
    {
        get; init;
    }

    public required string Role
    {
        get; init;
    }

    public required bool EmailVerified
    {
        get; init;
    }
}
