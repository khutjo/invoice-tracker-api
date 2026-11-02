using InvoiceTracker.Notification.Infrastructure.Persistence;

namespace InvoiceTracker.Notification.Api.Models.Response;

public sealed record NotificationLogItemResponse
{
    public required Guid NotificationId { get; init; }
    public required Guid InvoiceId { get; init; }
    public required string InvoiceNumber { get; init; }
    public required string CustomerName { get; init; }
    public required NotificationType NotificationType { get; init; }
    public required NotificationChannel Channel { get; init; }
    public required string Recipient { get; init; }
    public required NotificationStatus Status { get; init; }
    public required int Attempts { get; init; }

    /// <summary>The rendered plain-text message. The portal shows it as text, never as HTML.</summary>
    public required string Body { get; init; }

    public required DateTime CreatedAt { get; init; }
    public DateTime? SentAt { get; init; }
}
