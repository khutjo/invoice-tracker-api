using InvoiceTracker.Notification.Infrastructure.Persistence;

namespace InvoiceTracker.Notification.Infrastructure.Delivery;

/// <summary>Delivers one rendered notification over its channel. Throws when delivery fails, so the dispatcher can retry.</summary>
public interface INotificationSender
{
    Task SendAsync(CustomerNotification notification, CancellationToken cancellationToken);
}
