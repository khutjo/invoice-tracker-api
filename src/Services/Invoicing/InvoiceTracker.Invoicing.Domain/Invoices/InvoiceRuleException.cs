namespace InvoiceTracker.Invoicing.Domain.Invoices;

/// <summary>An invoice rule refused the change, for example editing a sent invoice. The API answers 409.</summary>
public sealed class InvoiceRuleException(string message) : Exception(message);
