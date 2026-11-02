using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.Notification.Api.Options;

public sealed class PortalOptions
{
    public const string SectionName = "Portal";

    [Required]
    [Url]
    public string BaseUrl { get; init; } = string.Empty;

    [Required]
    public string VerifyEmailPath { get; init; } = "/verify-email";
}
