using InvoiceTracker.Identity.Api.Models.Request;
using InvoiceTracker.Identity.Api.Models.Response;

namespace InvoiceTracker.Identity.Api.Interfaces;

public interface IRegisterService
{
    Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest request, CancellationToken cancellationToken);

    Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken);
}
