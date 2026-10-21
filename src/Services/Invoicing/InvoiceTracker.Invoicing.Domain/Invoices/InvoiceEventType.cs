namespace InvoiceTracker.Invoicing.Domain.Invoices;

public enum InvoiceEventType
{
    Created,
    Edited,
    Sent,
    PaymentRecorded,
    MarkedOverdue,
    ReminderRaised,
    Paid,
    Cancelled
}
