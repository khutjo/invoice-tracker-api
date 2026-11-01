using InvoiceTracker.Invoicing.Api.Models.Request;
using InvoiceTracker.Invoicing.Api.Models.Response;

namespace InvoiceTracker.Invoicing.Api.Interfaces;

public interface IInvoiceService
{
    Task<PagedResponse<InvoiceSummaryResponse>> GetInvoicesListAsync(
        InvoiceSearchQuery query,
        Guid businessId,
        CancellationToken cancellationToken);

    Task<InvoiceDetailsResponse> GetInvoiceAsync(
        Guid invoiceId,
        Guid businessId,
        CancellationToken cancellationToken);

    Task<InvoiceDetailsResponse> CreateInvoiceAsync(
        InvoiceDetailsRequest request,
        Guid businessId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<InvoiceDetailsResponse> UpdateInvoiceAsync(
        Guid invoiceId,
        UpdateInvoiceRequest request,
        Guid businessId,
        Guid userId,
        CancellationToken cancellationToken);

    Task DeleteInvoiceAsync(
        Guid invoiceId,
        Guid businessId,
        CancellationToken cancellationToken);

    Task<InvoiceDetailsResponse> SendInvoiceAsync(
        Guid invoiceId,
        Guid businessId,
        Guid userId,
        CancellationToken cancellationToken);
}
