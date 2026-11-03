namespace InvoiceTracker.Invoicing.Api.Jobs;

public sealed record OverdueScanResult(
    DateOnly Today,
    int MarkedOverdue,
    int RemindersRaised
);
