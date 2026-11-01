namespace InvoiceTracker.Invoicing.Domain.Invoices;

/// <summary>
/// Aggregate root. Lines, payments and the timeline change only through this class,
/// and totals are always recalculated here, never taken from the caller.
/// </summary>
public sealed class Invoice
{
    public const int MaxLines = 100;
    public const int MaxPaymentTermsDays = 365;

    private readonly List<InvoiceLine> _lines = [];
    private readonly List<Payment> _payments = [];
    private readonly List<InvoiceEvent> _events = [];

    private Invoice()
    {
    }

    public Guid Id { get; private set; }
    public Guid BusinessId { get; private set; }
    public Guid CustomerId { get; private set; }

    /// <summary>Assigned at send, so drafts have none.</summary>
    public string? InvoiceNumber { get; private set; }

    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;

    /// <summary>Days from the issue date to the due date. The editor prefills it from the business's default terms.</summary>
    public int PaymentTermsDays { get; private set; }

    /// <summary>Frozen at send; null while Draft.</summary>
    public BilledCustomer? BilledCustomer { get; private set; }

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

    /// <summary>The id is left empty for EF Core's sequential GUID generator to fill on Add.</summary>
    public static Invoice CreateDraft(
        Guid businessId,
        Guid customerId,
        int paymentTermsDays,
        IEnumerable<InvoiceLineDetails> lines,
        Guid createdBy,
        DateTime now)
    {
        if (businessId == Guid.Empty)
        {
            throw new ArgumentException("An invoice belongs to a business.", nameof(businessId));
        }

        var invoice = new Invoice
        {
            BusinessId = businessId,
            CreatedAt = now
        };

        invoice.SetDraftDetails(customerId, paymentTermsDays, lines);
        invoice.Record(InvoiceEventType.Created, createdBy, now);

        return invoice;
    }

    /// <summary>Replaces the customer, terms and every line. Only a draft can be edited.</summary>
    public void UpdateDraft(
        Guid customerId,
        int paymentTermsDays,
        IEnumerable<InvoiceLineDetails> lines,
        Guid modifiedBy,
        DateTime now)
    {
        EnsureDraft("edited");

        SetDraftDetails(customerId, paymentTermsDays, lines);
        ModifiedAt = now;
        Record(InvoiceEventType.Edited, modifiedBy, now);
    }

    /// <summary>Drafts can be deleted; anything sent stays for the audit trail and can only be cancelled.</summary>
    public void EnsureCanDelete()
    {
        EnsureDraft("deleted");
    }

    /// <summary>
    /// Draft to Sent: takes the number, freezes the customer's details, sets the issue date
    /// and works out the due date from the terms.
    /// </summary>
    public void Send(string invoiceNumber, DateOnly issueDate, BilledCustomer billedCustomer, Guid sentBy, DateTime now)
    {
        EnsureDraft("sent");
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceNumber);
        ArgumentNullException.ThrowIfNull(billedCustomer);

        if (_lines.Count == 0)
        {
            throw new InvoiceRuleException("An invoice needs at least one line before it can be sent.");
        }

        if (Total <= 0)
        {
            throw new InvoiceRuleException("An invoice with a zero total cannot be sent.");
        }

        var dueDate = issueDate.AddDays(PaymentTermsDays);

        InvoiceNumber = invoiceNumber;
        BilledCustomer = billedCustomer;
        Status = InvoiceStatus.Sent;
        IssueDate = issueDate;
        DueDate = dueDate;
        ModifiedAt = now;
        Record(InvoiceEventType.Sent, sentBy, now, $"Sent as {invoiceNumber}, due {dueDate:yyyy-MM-dd}.");
    }

    private void SetDraftDetails(Guid customerId, int paymentTermsDays, IEnumerable<InvoiceLineDetails> lines)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("An invoice needs a customer.", nameof(customerId));
        }

        if (paymentTermsDays is < 0 or > MaxPaymentTermsDays)
        {
            throw new ArgumentOutOfRangeException(nameof(paymentTermsDays), $"Payment terms are 0 to {MaxPaymentTermsDays} days.");
        }

        var newLines = lines.Select((line, index) => InvoiceLine.Create(index + 1, line)).ToList();
        if (newLines.Count > MaxLines)
        {
            throw new ArgumentOutOfRangeException(nameof(lines), $"An invoice has at most {MaxLines} lines.");
        }

        CustomerId = customerId;
        PaymentTermsDays = paymentTermsDays;

        // Lines are replaced, not patched: EF deletes the removed rows because a line cannot exist without its invoice
        _lines.Clear();
        _lines.AddRange(newLines);

        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        Subtotal = _lines.Sum(l => l.LineTotal);
        VatTotal = _lines.Sum(l => l.VatAmount);
        Total = Subtotal + VatTotal;
        Balance = Total - AmountPaid;
    }

    private void EnsureDraft(string action)
    {
        if (Status != InvoiceStatus.Draft)
        {
            throw new InvoiceRuleException($"Only a draft invoice can be {action}. This invoice is {Status}.");
        }
    }

    private void Record(InvoiceEventType eventType, Guid? userId, DateTime now, string? detail = null)
    {
        _events.Add(InvoiceEvent.Create(eventType, userId, now, detail));
    }
}
