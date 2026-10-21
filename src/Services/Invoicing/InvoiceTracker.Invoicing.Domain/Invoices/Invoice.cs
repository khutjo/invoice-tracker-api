namespace InvoiceTracker.Invoicing.Domain.Invoices;

/// <summary>
/// Aggregate root. Lines, payments and the timeline change only through this class.
/// Behaviour (send, record payment, cancel, mark overdue) is added with the domain rules.
/// </summary>
public sealed class Invoice
{
    private readonly List<InvoiceLine> _lines = [];
    private readonly List<Payment> _payments = [];
    private readonly List<InvoiceEvent> _events = [];

    public Guid Id { get; private set; }
    public Guid BusinessId { get; private set; }
    public Guid CustomerId { get; private set; }

    /// <summary>Assigned at send, so drafts have none.</summary>
    public string? InvoiceNumber { get; private set; }

    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    public DateOnly? IssueDate { get; private set; }
    public DateOnly? DueDate { get; private set; }

    public decimal Subtotal { get; private set; }
    public decimal VatTotal { get; private set; }
    public decimal Total { get; private set; }
    public decimal AmountPaid { get; private set; }
    public decimal Balance { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? ModifiedAt { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public IReadOnlyCollection<InvoiceLine> Lines => _lines;
    public IReadOnlyCollection<Payment> Payments => _payments;
    public IReadOnlyCollection<InvoiceEvent> Events => _events;
}
