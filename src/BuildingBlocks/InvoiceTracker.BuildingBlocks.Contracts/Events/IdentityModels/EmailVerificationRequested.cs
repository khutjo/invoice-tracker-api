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

    public required string VerificationUrl
    {
        get; init;
    }
}
