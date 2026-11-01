using InvoiceTracker.Business.Api.Models.Request;
using InvoiceTracker.Business.Api.Models.Response;

namespace InvoiceTracker.Business.Api.Interfaces;

public interface ICustomerService
{
    Task<PagedResponse<CustomerSummaryResponse>> SearchCustomersAsync(CustomerSearchQuery query, Guid businessId, CancellationToken cancellationToken);

    Task<CustomerDetailsResponse> GetCustomerAsync(Guid customerId, Guid businessId, CancellationToken cancellationToken);

    Task<CustomerDetailsResponse> CreateCustomerAsync(CustomerDetailsRequest request, Guid businessId, Guid userId, CancellationToken cancellationToken);

    Task<CustomerDetailsResponse> UpdateCustomerAsync(Guid customerId, UpdateCustomerRequest request, Guid businessId, Guid userId, CancellationToken cancellationToken);

    Task ArchiveCustomerAsync(Guid customerId, Guid businessId, Guid userId, CancellationToken cancellationToken);
}
