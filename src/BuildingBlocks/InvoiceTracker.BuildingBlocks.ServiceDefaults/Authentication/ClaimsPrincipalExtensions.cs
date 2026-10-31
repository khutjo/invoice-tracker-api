using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace InvoiceTracker.BuildingBlocks.ServiceDefaults.Authentication;

/// <summary>
/// The token is the only source of tenant and user identity. A validated token without these claims
/// means Identity and this service disagree on the contract, so it fails loudly instead of guessing.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        return GetGuid(principal, JwtRegisteredClaimNames.Sub);
    }

    public static Guid GetBusinessId(this ClaimsPrincipal principal)
    {
        return GetGuid(principal, InvoiceTrackerClaimTypes.BusinessId);
    }

    private static Guid GetGuid(ClaimsPrincipal principal, string claimType)
    {
        var value = principal.FindFirst(claimType)?.Value;
        if (!Guid.TryParse(value, out var id))
        {
            throw new InvalidOperationException($"The access token has no valid '{claimType}' claim.");
        }

        return id;
    }
}
