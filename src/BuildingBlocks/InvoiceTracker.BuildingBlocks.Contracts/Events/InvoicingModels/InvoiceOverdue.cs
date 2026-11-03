namespace InvoiceTracker.BuildingBlocks.Contracts.Events.InvoicingModels;

public sealed record InvoiceOverdue : IntegrationEvent
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

    public required DateOnly DueDate
    {
        get; init;
    }

    public required int DaysOverdue
    {
        get; init;
    }

    public required decimal Balance
    {
        get; init;
    }

    public required OverdueReminderStage ReminderStage
    {
        get; init;
    }
}
