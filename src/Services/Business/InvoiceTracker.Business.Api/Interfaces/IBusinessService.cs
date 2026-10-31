using InvoiceTracker.Business.Api.Models.Request;
using InvoiceTracker.Business.Api.Models.Response;

namespace InvoiceTracker.Business.Api.Interfaces;

public interface IBusinessService
{
    Task<BusinessDetailsResponse> CreateBusinessAsync(CreateBusinessRequest request, Guid businessId, Guid userId, CancellationToken cancellationToken);
    Task<BusinessDetailsResponse> GetBusinessDetailsAsync(Guid businessId, CancellationToken cancellationToken);
    Task<BusinessDetailsResponse> UpdateBusinessDetailsAsync(UpdateBusinessDetailsRequest request, Guid businessId, Guid userId, CancellationToken cancellationToken);
    Task<BusinessSettingsResponse> GetBusinessSettingsAsync(Guid businessId, CancellationToken cancellationToken);
    Task<BusinessSettingsResponse> SaveBusinessSettingAsync(SaveBusinessSettingsRequest request, Guid businessId, Guid userId, CancellationToken cancellationToken);
    Task<SaveLogoResponse> SaveLogoAsync(SaveLogoRequest request, Guid businessId, Guid userId, CancellationToken cancellationToken);
    Task<LogoResponse> GetLogoAsync(Guid businessId, CancellationToken cancellationToken);
}
