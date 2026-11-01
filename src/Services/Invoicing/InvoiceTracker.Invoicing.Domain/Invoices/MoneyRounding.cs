namespace InvoiceTracker.Invoicing.Domain.Invoices;

internal static class MoneyRounding
{
    /// <summary>Rand amounts to the cent, half a cent rounds up (0.005 becomes 0.01).</summary>
    public static decimal ToCents(decimal amount)
    {
        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }
}
