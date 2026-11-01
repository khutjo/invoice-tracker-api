using InvoiceTracker.Invoicing.Domain.Invoices;

namespace InvoiceTracker.Invoicing.Api.Models.Response;

public sealed record InvoiceEventResponse
{
    public required InvoiceEventType EventType
    {
        get; init;
    }

    public required DateTime OccurredAt
    {
        get; init;
    }

    public Guid? UserId
    {
        get; init;
    }

    public string? Detail
    {
        get; init;
    }
}
