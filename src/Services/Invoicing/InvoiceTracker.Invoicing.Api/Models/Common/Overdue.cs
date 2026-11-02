namespace InvoiceTracker.Invoicing.Api.Models.Common;

public sealed record Overdue(
    decimal Balance,
    int OverdueInvoiceCount,
    int? OldestDaysPastDue
);
