namespace InvoiceTracker.Invoicing.Api.Models.Request;

public sealed record PaymentWebhookRequest
{
    public string? EventId { get; init; }

    public string? EventType { get; init; }

    public Guid? InvoiceId { get; init; }

    public decimal? Amount { get; init; }

    public string? Currency { get; init; }

    public DateTimeOffset? PaidAt { get; init; }

    public string? Method { get; init; }

    public string? Reference { get; init; }
}
