using InvoiceTracker.Identity.Infrastructure.Persistence;
using InvoiceTracker.Identity.Infrastructure.Tokens;

namespace InvoiceTracker.Identity.Infrastructure.Interfaces;

public interface IAccessTokenIssuer
{
    AccessToken Issue(ApplicationUser user);
}
