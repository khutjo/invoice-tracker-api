namespace InvoiceTracker.BuildingBlocks.Contracts.Events.InvoicingModels;

public sealed record PaymentRecorded : IntegrationEvent
{
    public required Guid InvoiceId
    {
        get; init;
    }

    public required Guid PaymentId
    {
        get; init;
    }

    public required string InvoiceNumber
    {
        get; init;
    }

    public required string CustomerName
    {
        get; init;
    }

    public required string CustomerEmail
    {
        get; init;
    }

    public required decimal Amount
    {
        get; init;
    }

    public required DateOnly PaidOn
    {
        get; init;
    }

    public required decimal Balance
    {
        get; init;
    }
}
