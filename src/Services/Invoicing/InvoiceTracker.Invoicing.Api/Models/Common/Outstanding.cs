namespace InvoiceTracker.Invoicing.Api.Models.Common;

public sealed record Outstanding(
    decimal Balance,
    int OpenInvoiceCount
);
