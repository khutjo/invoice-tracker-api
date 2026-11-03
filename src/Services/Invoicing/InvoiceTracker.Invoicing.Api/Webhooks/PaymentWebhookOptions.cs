using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.Invoicing.Api.Webhooks;

public sealed class PaymentWebhookOptions
{
    public const string SectionName = "PaymentWebhook";

    /// <summary>Shared with the payment processor. User-secrets locally, PAYMENT_WEBHOOK_SECRET in Docker.</summary>
    [Required]
    [MinLength(32)]
    public string Secret { get; init; } = string.Empty;

    /// <summary>How far the signed timestamp may be from now before the request counts as a replay.</summary>
    [Range(typeof(TimeSpan), "00:00:30", "01:00:00")]
    public TimeSpan Tolerance { get; init; } = TimeSpan.FromMinutes(5);
}
