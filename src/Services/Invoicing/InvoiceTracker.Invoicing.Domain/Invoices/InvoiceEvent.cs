namespace InvoiceTracker.Invoicing.Domain.Invoices;

/// <summary>Append-only timeline entry.</summary>
public sealed class InvoiceEvent
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public InvoiceEventType EventType { get; private set; }
    public DateTime OccurredAt { get; private set; }

    /// <summary>Null when the system made the change (for example the overdue scanner).</summary>
    public Guid? UserId { get; private set; }

    public string? Detail { get; private set; }
}
