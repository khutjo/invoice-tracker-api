namespace InvoiceTracker.Invoicing.Domain.Invoices;

public sealed class Payment
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly PaidOn { get; private set; }
    public PaymentMethod Method { get; private set; }
    public string? Reference { get; private set; }
    public DateTime RecordedAt { get; private set; }
}
