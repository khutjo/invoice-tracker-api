using InvoiceTracker.Invoicing.Api.Jobs;

namespace InvoiceTracker.Invoicing.Api.Interfaces;

public interface IOverdueScanService
{
    Task<OverdueScanResult> RunAsync(Guid? businessId, DateOnly? asOf, CancellationToken cancellationToken);
}
