using InvoiceTracker.Notification.Infrastructure.Persistence;

namespace InvoiceTracker.Notification.Api.Hubs;

/// <summary>The <c>notificationSent</c> payload. The portal's NotificationSentMessage type mirrors it.</summary>
public sealed record NotificationSentMessage(
    Guid NotificationId,
    Guid InvoiceId,
    string InvoiceNumber,
    string CustomerName,
    NotificationType NotificationType,
    NotificationChannel Channel,
    string Recipient,
    DateTime SentAt)
{
    public static NotificationSentMessage From(CustomerNotification notification)
    {
        return new NotificationSentMessage(
            notification.Id,
            notification.InvoiceId,
            notification.InvoiceNumber,
            notification.CustomerName,
            notification.NotificationType,
            notification.Channel,
            notification.Recipient,
            DateTime.SpecifyKind(notification.SentAt ?? default, DateTimeKind.Utc));
    }
}
