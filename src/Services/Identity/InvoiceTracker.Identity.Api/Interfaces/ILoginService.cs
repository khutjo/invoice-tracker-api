using InvoiceTracker.Identity.Api.Models.Request;
using InvoiceTracker.Identity.Api.Models.Response;

namespace InvoiceTracker.Identity.Api.Interfaces;

public interface ILoginService
{
    Task<LoginResponse> LoginAsync(LogInRequest request, CancellationToken cancellationToken);

    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken);

    Task LogoutAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
}
