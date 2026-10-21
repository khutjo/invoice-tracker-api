namespace InvoiceTracker.Invoicing.Domain.Invoices;

public sealed class InvoiceLine
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    /// <summary>0.15 for standard VAT, 0 for zero-rated or exempt lines.</summary>
    public decimal VatRate { get; private set; }

    /// <summary>Quantity x unit price, rounded per line (AwayFromZero).</summary>
    public decimal LineTotal { get; private set; }
}
