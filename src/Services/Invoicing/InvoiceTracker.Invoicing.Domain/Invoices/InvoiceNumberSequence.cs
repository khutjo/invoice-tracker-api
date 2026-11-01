using System.Globalization;

namespace InvoiceTracker.Invoicing.Domain.Invoices;

/// <summary>
/// Last invoice number used by a business in a year. Numbers are taken only at send, inside the send
/// transaction, so a rolled-back send or a deleted draft never leaves a gap.
/// </summary>
public sealed class InvoiceNumberSequence
{
    private InvoiceNumberSequence()
    {
    }

    public Guid BusinessId { get; private set; }
    public int Year { get; private set; }
    public int LastNumber { get; private set; }

    public static InvoiceNumberSequence Start(Guid businessId, int year)
    {
        if (businessId == Guid.Empty)
        {
            throw new ArgumentException("A number sequence belongs to a business.", nameof(businessId));
        }

        return new InvoiceNumberSequence { BusinessId = businessId, Year = year };
    }

    /// <summary>Moves the sequence on and returns the formatted number, for example INV-2026-0042.</summary>
    public string Next()
    {
        LastNumber++;
        return Format(Year, LastNumber);
    }

    public static string Format(int year, int number)
    {
        return string.Create(CultureInfo.InvariantCulture, $"INV-{year}-{number:D4}");
    }
}
