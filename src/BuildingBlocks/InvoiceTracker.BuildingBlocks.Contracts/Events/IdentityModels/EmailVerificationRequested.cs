namespace InvoiceTracker.BuildingBlocks.Contracts.Events.IdentityModels;

public sealed record EmailVerificationRequested : IntegrationEvent
{
    public required Guid UserId
    {
        get; init;
    }

    public required string Email
    {
        get; init;
    }

    public required string FirstName
    {
        get; init;
    }

    /// <summary>
    /// Base64Url email confirmation token. Notification builds the portal link from UserId and Token;
    /// the portal posts both to POST /api/v1/auth/verify-email.
    /// </summary>
    public required string Token
    {
        get; init;
    }
}
