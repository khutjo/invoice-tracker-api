using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace InvoiceTracker.Invoicing.Infrastructure.Customers;

/// <summary>
/// Sends the caller's bearer token on to the Business service, so Business decides what this user may see.
/// That keeps tenant isolation in one place: another business's customer id simply comes back 404.
/// </summary>
public sealed class ForwardAuthorizationHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authorization = httpContextAccessor.HttpContext?.Request.Headers[HeaderNames.Authorization].ToString();
        if (string.IsNullOrEmpty(authorization))
        {
            throw new InvalidOperationException("Customer lookups need the signed-in user's token; they cannot run outside an HTTP request.");
        }

        request.Headers.TryAddWithoutValidation(HeaderNames.Authorization, authorization);
        return base.SendAsync(request, cancellationToken);
    }
}
