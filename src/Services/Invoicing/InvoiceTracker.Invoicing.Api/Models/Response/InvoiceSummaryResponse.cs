using InvoiceTracker.Invoicing.Domain.Invoices;

namespace InvoiceTracker.Invoicing.Api.Models.Response;

public sealed record InvoiceSummaryResponse
{
    public required Guid InvoiceId
    {
        get; init;
    }

    public string? InvoiceNumber
    {
        get; init;
    }

    public required Guid CustomerId
    {
        get; init;
    }

    /// <summary>Frozen at send. Null for drafts: look the name up through the customers API.</summary>
    public string? CustomerName
    {
        get; init;
    }

    public required InvoiceStatus Status
    {
        get; init;
    }

    public DateOnly? IssueDate
    {
        get; init;
    }

    public DateOnly? DueDate
    {
        get; init;
    }

    public required decimal Total
    {
        get; init;
    }

    public required decimal AmountPaid
    {
        get; init;
    }

    public required decimal Balance
    {
        get; init;
    }

    public required DateTime CreatedAt
    {
        get; init;
    }
}
