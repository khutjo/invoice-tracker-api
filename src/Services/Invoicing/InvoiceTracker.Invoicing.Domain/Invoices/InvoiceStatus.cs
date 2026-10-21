namespace InvoiceTracker.Invoicing.Domain.Invoices;

public enum InvoiceStatus
{
    Draft,
    Sent,
    Overdue,
    Paid,
    Cancelled
}
