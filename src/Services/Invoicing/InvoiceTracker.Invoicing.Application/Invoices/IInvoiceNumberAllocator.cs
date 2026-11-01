namespace InvoiceTracker.Invoicing.Application.Invoices;

public interface IInvoiceNumberAllocator
{
    /// <summary>
    /// Reserves the next invoice number for the business and year. Call it inside the send transaction:
    /// the sequence row stays locked until commit, and a rollback hands the number back.
    /// </summary>
    Task<string> NextAsync(Guid businessId, int year, CancellationToken cancellationToken);
}
