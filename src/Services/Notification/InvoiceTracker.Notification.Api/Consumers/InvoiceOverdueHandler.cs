using InvoiceTracker.BuildingBlocks.Contracts.Events.InvoicingModels;
using InvoiceTracker.BuildingBlocks.Messaging.Abstractions;
using InvoiceTracker.Notification.Api.Templates;
using InvoiceTracker.Notification.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Notification.Api.Consumers;

/// <summary>
/// One reminder per stage per invoice: the unique (invoice_id, notification_type) index backs up the
/// exists-check, so a redelivered event never sends a second reminder.
/// </summary>
public sealed class InvoiceOverdueHandler(
    NotificationDbContext dbContext,
    TimeProvider timeProvider,
    ILogger<InvoiceOverdueHandler> logger) : IIntegrationEventHandler<InvoiceOverdue>
{
    public async Task HandleAsync(InvoiceOverdue integrationEvent, CancellationToken cancellationToken)
    {
        var notificationType = integrationEvent.ReminderStage switch
        {
            OverdueReminderStage.First => NotificationType.OverdueReminder1,
            OverdueReminderStage.Second => NotificationType.OverdueReminder2,
            OverdueReminderStage.Final => NotificationType.OverdueFinal,
            _ => throw new InvalidOperationException($"Unknown reminder stage {integrationEvent.ReminderStage} on event {integrationEvent.EventId}.")
        };

        var alreadyCreated = await dbContext.Notifications.AnyAsync(
            n => n.InvoiceId == integrationEvent.InvoiceId && n.NotificationType == notificationType,
            cancellationToken);

        if (alreadyCreated)
        {
            logger.LogInformation("{NotificationType} for {InvoiceId} already exists; skipping duplicate {EventId}",
                notificationType, integrationEvent.InvoiceId, integrationEvent.EventId);
            return;
        }

        var notification = CustomerNotification.CreatePending(
            integrationEvent.BusinessId,
            integrationEvent.InvoiceId,
            integrationEvent.InvoiceNumber,
            integrationEvent.CustomerName,
            notificationType,
            NotificationChannel.Email,
            integrationEvent.CustomerEmail,
            NotificationTemplates.InvoiceOverdue(integrationEvent),
            timeProvider.GetUtcNow().UtcDateTime);

        dbContext.Notifications.Add(notification);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            logger.LogInformation("{NotificationType} for {InvoiceId} was created by a concurrent delivery; skipping {EventId}",
                notificationType, integrationEvent.InvoiceId, integrationEvent.EventId);
            return;
        }

        logger.LogInformation("Queued {NotificationType} {NotificationId} for invoice {InvoiceNumber}",
            notification.NotificationType, notification.Id, notification.InvoiceNumber);
    }
}
