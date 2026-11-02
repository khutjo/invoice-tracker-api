namespace InvoiceTracker.Invoicing.Api.Models.Common;

public sealed record DraftPipeline(
    decimal Total,
    int DraftCount
);
