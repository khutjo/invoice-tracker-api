namespace InvoiceTracker.Invoicing.Domain.Invoices;

public sealed class InvoiceLine
{
    private InvoiceLine()
    {
    }

    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }

    /// <summary>1-based position on the invoice, so the PDF and the editor show lines in the order they were entered.</summary>
    public int LineNumber { get; private set; }

    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    /// <summary>0.15 for standard VAT, 0 for zero-rated or exempt lines.</summary>
    public decimal VatRate { get; private set; }

    /// <summary>Quantity x unit price, excluding VAT, rounded per line (AwayFromZero).</summary>
    public decimal LineTotal { get; private set; }

    /// <summary>VAT on this line, rounded per line so the invoice VAT total is a sum of rounded lines.</summary>
    public decimal VatAmount => MoneyRounding.ToCents(LineTotal * VatRate);

    internal static InvoiceLine Create(int lineNumber, InvoiceLineDetails details)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(details.Description);

        if (details.Quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(details), "Quantity must be more than zero.");
        }

        if (details.UnitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(details), "Unit price cannot be negative.");
        }

        if (details.VatRate is < 0 or >= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(details), "VAT rate is a fraction from 0 up to, but not including, 1.");
        }

        return new InvoiceLine
        {
            LineNumber = lineNumber,
            Description = details.Description.Trim(),
            Quantity = details.Quantity,
            UnitPrice = details.UnitPrice,
            VatRate = details.VatRate,
            LineTotal = MoneyRounding.ToCents(details.Quantity * details.UnitPrice)
        };
    }
}
