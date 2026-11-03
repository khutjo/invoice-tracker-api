using InvoiceTracker.Invoicing.Domain.Invoices;

namespace InvoiceTracker.Invoicing.Api.Models.Response;

public sealed record PaymentWebhookResponse
{
    public const string Processed = "processed";
    public const string Duplicate = "duplicate";
    public const string Ignored = "ignored";

    public required string Result
    {
        get; init;
    }

    public Guid? PaymentId
    {
        get; init;
    }

    public InvoiceStatus? InvoiceStatus
    {
        get; init;
    }

    public decimal? Balance
    {
        get; init;
    }
}
