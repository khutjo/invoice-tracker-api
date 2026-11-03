namespace InvoiceTracker.Invoicing.Domain.Invoices;

/// <summary>
/// How far the overdue reminders have escalated. Stored as a number so the scanner can compare stages in SQL.
/// </summary>
public enum OverdueReminderStage
{
    None = 0,

    /// <summary>Raised on the first day the invoice is overdue.</summary>
    First = 1,

    /// <summary>Raised at 7 days overdue.</summary>
    Second = 2,

    /// <summary>Raised at 14 days overdue; no reminders after this.</summary>
    Final = 3
}
