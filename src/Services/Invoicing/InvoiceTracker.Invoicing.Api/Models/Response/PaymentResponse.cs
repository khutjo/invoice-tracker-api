using InvoiceTracker.Invoicing.Domain.Invoices;

namespace InvoiceTracker.Invoicing.Api.Models.Response;

public sealed record PaymentResponse
{
    public required Guid PaymentId
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

    public required PaymentMethod Method
    {
        get; init;
    }

    public string? Reference
    {
        get; init;
    }

    public required DateTime RecordedAt
    {
        get; init;
    }
}
