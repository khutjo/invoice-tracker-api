using InvoiceTracker.Invoicing.Domain.Invoices;

namespace InvoiceTracker.Invoicing.Api.Models.Response;

public sealed record InvoiceDetailsResponse
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

    /// <summary>The customer's details as they were at send. Null for drafts.</summary>
    public BilledCustomerResponse? BilledCustomer
    {
        get; init;
    }

    public required InvoiceStatus Status
    {
        get; init;
    }

    public required int PaymentTermsDays
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

    public required decimal Subtotal
    {
        get; init;
    }

    public required decimal VatTotal
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

    public required IReadOnlyList<InvoiceLineResponse> Lines
    {
        get; init;
    }

    public required IReadOnlyList<PaymentResponse> Payments
    {
        get; init;
    }

    public required IReadOnlyList<InvoiceEventResponse> Timeline
    {
        get; init;
    }

    public required DateTime CreatedAt
    {
        get; init;
    }

    public DateTime? ModifiedAt
    {
        get; init;
    }

    public required string Version
    {
        get; init;
    }
}
