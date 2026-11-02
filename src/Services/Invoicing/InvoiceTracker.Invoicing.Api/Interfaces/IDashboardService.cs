using InvoiceTracker.Invoicing.Api.Models.Response;

namespace InvoiceTracker.Invoicing.Api.Interfaces;

public interface IDashboardService
{
    Task<DashboardDetailsResponse> GetDashboardDetailsAsync(Guid businessId, CancellationToken cancellationToken);
}
