using InvoiceTracker.Notification.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace InvoiceTracker.Notification.Infrastructure.Delivery;

/// <summary>
/// Stands in for real email and SMS providers (out of scope for this system): it logs the message and reports success.
/// The rendered body stays in notification_db, where the portal shows it.
/// </summary>
public sealed class SimulatedNotificationSender(ILogger<SimulatedNotificationSender> logger) : INotificationSender
{
    public Task SendAsync(CustomerNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Simulated {Channel} {NotificationType} for invoice {InvoiceNumber} delivered to {Recipient}",
            notification.Channel,
            notification.NotificationType,
            notification.InvoiceNumber,
            notification.Recipient);

        return Task.CompletedTask;
    }
}
