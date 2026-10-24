namespace InvoiceTracker.BuildingBlocks.Contracts.Events.IdentityModels;

public sealed record BusinessRegistered : IntegrationEvent
{
    public required string BusinessName
    {
        get; init;
    }

    public required Guid OwnerUserId
    {
        get; init;
    }

    public required string OwnerEmail
    {
        get; init;
    }

    public required string OwnerFirstName
    {
        get; init;
    }

    public required string OwnerLastName
    {
        get; init;
    }
}
