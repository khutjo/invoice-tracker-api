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

    public static string InvoiceOverdue(InvoiceOverdue invoice)
    {
        var opening = invoice.ReminderStage switch
        {
            OverdueReminderStage.First => $"This is a friendly reminder that invoice {invoice.InvoiceNumber} was due on {Date(invoice.DueDate)} and is now overdue.",
            OverdueReminderStage.Second => $"Invoice {invoice.InvoiceNumber} is now {invoice.DaysOverdue} days overdue (it was due on {Date(invoice.DueDate)}).",
            _ => $"Final reminder: invoice {invoice.InvoiceNumber} is {invoice.DaysOverdue} days overdue (it was due on {Date(invoice.DueDate)})."
        };

        var ask = invoice.ReminderStage == OverdueReminderStage.Final
            ? "Please settle it today, or contact us if there is a problem with the invoice."
            : "If you have already paid, please ignore this message.";

        return string.Join(Environment.NewLine,
            $"Hi {invoice.CustomerName},",
            string.Empty,
            opening,
            $"The outstanding balance is {Money(invoice.Balance)}. Please use {invoice.InvoiceNumber} as the payment reference.",
            ask,
            string.Empty,
            "Thank you.");
    }

    public static string PaymentReceived(PaymentRecorded payment)
    {
        var balanceLine = payment.Balance switch
        {
            0 => $"Invoice {payment.InvoiceNumber} is now paid in full.",
            < 0 => $"Invoice {payment.InvoiceNumber} is paid in full and you have overpaid by {Money(-payment.Balance)}. We will contact you about the credit.",
            _ => $"The remaining balance on invoice {payment.InvoiceNumber} is {Money(payment.Balance)}."
        };

        return string.Join(Environment.NewLine,
            $"Hi {payment.CustomerName},",
            string.Empty,
            $"We received your payment of {Money(payment.Amount)} on {Date(payment.PaidOn)} for invoice {payment.InvoiceNumber}.",
            balanceLine,
            string.Empty,
            "Thank you for your payment.");
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
