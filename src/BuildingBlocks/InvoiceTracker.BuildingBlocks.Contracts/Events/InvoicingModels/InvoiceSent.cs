namespace InvoiceTracker.BuildingBlocks.Contracts.Events.InvoicingModels;

public sealed record InvoiceSent : IntegrationEvent
{
    public required Guid InvoiceId
    {
        get; init;
    }

    public required string InvoiceNumber
    {
        get; init;
    }

    public required Guid CustomerId
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

    public required DateOnly IssueDate
    {
        get; init;
    }

    public required DateOnly DueDate
    {
        get; init;
    }

    /// <summary>Rand (ZAR), VAT inclusive.</summary>
    public required decimal Total
    {
        get; init;
    }

    public required decimal Balance
    {
        get; init;
    }
}
