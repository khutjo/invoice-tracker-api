using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.Identity.Api.Options;

/// <summary>The web portal (SPA). Links in emails point here, never at the API.</summary>
public sealed class PortalOptions
{
    public const string SectionName = "Portal";

    [Required, Url]
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// Portal screen that reads userId and token from the query string and posts them to
    /// POST /api/v1/auth/verify-email.
    /// </summary>
    [Required]
    public string VerifyEmailPath { get; init; } = "/verify-email";
}
