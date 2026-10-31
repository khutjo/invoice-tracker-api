using InvoiceTracker.Business.Api.Models.Response;

namespace InvoiceTracker.Business.Api.Interfaces;

public interface ILookupService
{
    Task<LookupsResponse> GetLookupsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<PostalCodeOption>> FindPostalCodeAsync(string code, CancellationToken cancellationToken);
}
