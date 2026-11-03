using InvoiceTracker.BuildingBlocks.Contracts.Events.InvoicingModels;
using InvoiceTracker.BuildingBlocks.Messaging.Abstractions;
using InvoiceTracker.Notification.Api.Templates;
using InvoiceTracker.Notification.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Notification.Api.Consumers;

/// <summary>One "payment received" message per payment, keyed on the payment id so redeliveries are ignored.</summary>
public sealed class PaymentRecordedHandler(
    NotificationDbContext dbContext,
    TimeProvider timeProvider,
    ILogger<PaymentRecordedHandler> logger) : IIntegrationEventHandler<PaymentRecorded>
{
    public async Task HandleAsync(PaymentRecorded integrationEvent, CancellationToken cancellationToken)
    {
        var deduplicationKey = integrationEvent.PaymentId.ToString("N");

        var alreadyCreated = await dbContext.Notifications.AnyAsync(
            n => n.InvoiceId == integrationEvent.InvoiceId
                && n.NotificationType == NotificationType.PaymentReceived
                && n.DeduplicationKey == deduplicationKey,
            cancellationToken);

        if (alreadyCreated)
        {
            logger.LogInformation("PaymentReceived for payment {PaymentId} already exists; skipping duplicate {EventId}",
                integrationEvent.PaymentId, integrationEvent.EventId);
            return;
        }

        var notification = CustomerNotification.CreatePending(
            integrationEvent.BusinessId,
            integrationEvent.InvoiceId,
            integrationEvent.InvoiceNumber,
            integrationEvent.CustomerName,
            NotificationType.PaymentReceived,
            NotificationChannel.Email,
            integrationEvent.CustomerEmail,
            NotificationTemplates.PaymentReceived(integrationEvent),
            timeProvider.GetUtcNow().UtcDateTime,
            deduplicationKey);

        dbContext.Notifications.Add(notification);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            logger.LogInformation("PaymentReceived for payment {PaymentId} was created by a concurrent delivery; skipping {EventId}",
                integrationEvent.PaymentId, integrationEvent.EventId);
            return;
        }

        logger.LogInformation("Queued {NotificationType} {NotificationId} for invoice {InvoiceNumber}",
            notification.NotificationType, notification.Id, notification.InvoiceNumber);
    }
}
