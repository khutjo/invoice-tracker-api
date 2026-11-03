namespace InvoiceTracker.Invoicing.Domain.Invoices;

public sealed class Payment
{
    private Payment() { }

    public Guid Id
    {
        get; private set;
    }
    public Guid InvoiceId
    {
        get; private set;
    }
    public decimal Amount
    {
        get; private set;
    }
    public DateOnly PaidOn
    {
        get; private set;
    }
    public PaymentMethod Method
    {
        get; private set;
    }
    public string? Reference
    {
        get; private set;
    }

    /// <summary>
    /// The payment processor's event id when the payment arrived by webhook. Unique, so a redelivered
    /// webhook can never record the same payment twice. Null for payments captured by hand.
    /// </summary>
    public string? ExternalId
    {
        get; private set;
    }

    public DateTime RecordedAt
    {
        get; private set;
    }

    internal static Payment Create(decimal amount, DateOnly paidOn, PaymentMethod method, string? reference, string? externalId, DateTime recordedAt)
    {
        return new Payment
        {
            Amount = amount,
            PaidOn = paidOn,
            Method = method,
            Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim(),
            ExternalId = string.IsNullOrWhiteSpace(externalId) ? null : externalId.Trim(),
            RecordedAt = recordedAt
        };
    }
}
