namespace InvoiceTracker.BuildingBlocks.Contracts;

public abstract record IntegrationEvent
{
    public Guid EventId
    {
        get; init;
    } = Guid.CreateVersion7();

    public required DateTimeOffset OccurredAt
    {
        get; init;
    }

    public required Guid BusinessId
    {
        get; init;
    }

    public int Version
    {
        get; init;
    } = 1;
}
