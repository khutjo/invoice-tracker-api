namespace InvoiceTracker.Invoicing.Domain.Invoices;

/// <summary>Append-only timeline entry.</summary>
public sealed class InvoiceEvent
{
    private InvoiceEvent()
    {
    }

    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public InvoiceEventType EventType { get; private set; }
    public DateTime OccurredAt { get; private set; }

    /// <summary>Null when the system made the change (for example the overdue scanner).</summary>
    public Guid? UserId { get; private set; }

    public string? Detail { get; private set; }

    internal static InvoiceEvent Create(InvoiceEventType eventType, Guid? userId, DateTime occurredAt, string? detail)
    {
        return new InvoiceEvent
        {
            EventType = eventType,
            UserId = userId,
            OccurredAt = occurredAt,
            Detail = detail
        };
    }
}
