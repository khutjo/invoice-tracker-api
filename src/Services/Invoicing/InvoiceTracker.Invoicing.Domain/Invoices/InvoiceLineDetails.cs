namespace InvoiceTracker.Invoicing.Domain.Invoices;

/// <summary>What the caller supplies for a line. Line totals are always worked out by the domain.</summary>
public sealed record InvoiceLineDetails(string Description, decimal Quantity, decimal UnitPrice, decimal VatRate);
