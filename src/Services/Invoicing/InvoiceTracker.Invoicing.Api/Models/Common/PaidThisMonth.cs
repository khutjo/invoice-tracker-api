namespace InvoiceTracker.Invoicing.Api.Models.Common;

public sealed record PaidThisMonth(
    decimal AmountReceived,
    int PaidInFullCount
);
