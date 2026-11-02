using System.Globalization;
using InvoiceTracker.BuildingBlocks.Contracts.Events.InvoicingModels;

namespace InvoiceTracker.Notification.Api.Templates;

/// <summary>
/// Plain-text message bodies. The portal shows them as text (never as HTML), so customer-supplied names
/// can't inject markup; a real email sender would HTML-encode them in its own template.
/// </summary>
public static class NotificationTemplates
{
    private static readonly CultureInfo _southAfrica = CultureInfo.GetCultureInfo("en-ZA");

    public static string InvoiceSent(InvoiceSent invoice)
    {
        return string.Join(Environment.NewLine,
            $"Hi {invoice.CustomerName},",
            string.Empty,
            $"Invoice {invoice.InvoiceNumber} for {Money(invoice.Total)} was issued on {Date(invoice.IssueDate)}.",
            $"Please pay {Money(invoice.Balance)} by {Date(invoice.DueDate)}, using {invoice.InvoiceNumber} as the payment reference.",
            string.Empty,
            "Thank you for your business.");
    }

    private static string Money(decimal amount)
    {
        return amount.ToString("C", _southAfrica);
    }

    private static string Date(DateOnly date)
    {
        return date.ToString("d MMMM yyyy", _southAfrica);
    }
}
