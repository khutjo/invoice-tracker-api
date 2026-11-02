using InvoiceTracker.BuildingBlocks.Contracts.Events.InvoicingModels;
using InvoiceTracker.BuildingBlocks.Messaging.Abstractions;
using InvoiceTracker.Notification.Api.Templates;
using InvoiceTracker.Notification.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Notification.Api.Consumers;

public sealed class InvoiceSentHandler(
    NotificationDbContext dbContext,
    TimeProvider timeProvider,
    ILogger<InvoiceSentHandler> logger) : IIntegrationEventHandler<InvoiceSent>
{
    public async Task HandleAsync(InvoiceSent integrationEvent, CancellationToken cancellationToken)
    {
        var alreadyCreated = await dbContext.Notifications.AnyAsync(
            n => n.InvoiceId == integrationEvent.InvoiceId && n.NotificationType == NotificationType.InvoiceSent,
            cancellationToken);

        if (alreadyCreated)
        {
            logger.LogInformation("InvoiceSent notification for {InvoiceId} already exists; skipping duplicate {EventId}",
                integrationEvent.InvoiceId, integrationEvent.EventId);
            return;
        }

        var notification = CustomerNotification.CreatePending(
            integrationEvent.BusinessId,
            integrationEvent.InvoiceId,
            integrationEvent.InvoiceNumber,
            integrationEvent.CustomerName,
            NotificationType.InvoiceSent,
            NotificationChannel.Email,
            integrationEvent.CustomerEmail,
            NotificationTemplates.InvoiceSent(integrationEvent),
            timeProvider.GetUtcNow().UtcDateTime);

        dbContext.Notifications.Add(notification);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
           logger.LogInformation("InvoiceSent notification for {InvoiceId} was created by a concurrent delivery; skipping {EventId}",
                integrationEvent.InvoiceId, integrationEvent.EventId);
            return;
        }

        logger.LogInformation("Queued {NotificationType} {NotificationId} for invoice {InvoiceNumber}",
            notification.NotificationType, notification.Id, notification.InvoiceNumber);
    }
}
