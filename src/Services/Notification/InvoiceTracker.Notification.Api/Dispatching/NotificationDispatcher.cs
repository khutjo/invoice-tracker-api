using InvoiceTracker.Notification.Api.Hubs;
using InvoiceTracker.Notification.Infrastructure.Delivery;
using InvoiceTracker.Notification.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Notification.Api.Dispatching;

/// <summary>
/// Picks up due Pending notifications in batches and hands each to the sender.
/// Designed for a single Notification instance: rows are not locked or claimed, so running several
/// instances would need a distributed lock before they could share the work.
/// After the batch is saved, each Sent notification is pushed to its business's portal users over SignalR.
/// </summary>
public sealed class NotificationDispatcher(
    IServiceScopeFactory scopeFactory,
    IHubContext<NotificationsHub> hubContext,
    TimeProvider timeProvider,
    ILogger<NotificationDispatcher> logger) : BackgroundService
{
    private const int BatchSize = 20;
    private static readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval, timeProvider);

        do
        {
            try
            {
                await DispatchBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Notification dispatch batch failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task DispatchBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<INotificationSender>();

        var now = timeProvider.GetUtcNow().UtcDateTime;

        // Served by ix_notifications_status_next_attempt_at
        var due = await dbContext.Notifications
            .Where(n => n.Status == NotificationStatus.Pending && n.NextAttemptAt <= now)
            .OrderBy(n => n.NextAttemptAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (due.Count == 0)
        {
            return;
        }

        var sent = new List<CustomerNotification>();
        foreach (var notification in due)
        {
            try
            {
                await sender.SendAsync(notification, cancellationToken);
                notification.MarkSent(timeProvider.GetUtcNow().UtcDateTime);
                sent.Add(notification);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                notification.RecordFailedAttempt(timeProvider.GetUtcNow().UtcDateTime);
                logger.LogWarning(ex,
                    "Delivering notification {NotificationId} failed (attempt {Attempts} of {MaxAttempts}); now {Status}",
                    notification.Id, notification.Attempts, CustomerNotification.MaxAttempts, notification.Status);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Only after saving, so the portal never hears about a send that wasn't recorded
        await PushAsync(sent, cancellationToken);
    }

    private async Task PushAsync(IReadOnlyList<CustomerNotification> sent, CancellationToken cancellationToken)
    {
        foreach (var notification in sent)
        {
            try
            {
                await hubContext.Clients
                    .Group(NotificationsHub.GroupFor(notification.BusinessId))
                    .SendAsync(NotificationsHub.NotificationSentMethod, NotificationSentMessage.From(notification), cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // A lost push is harmless: the portal refetches the log on reconnect
                logger.LogWarning(ex, "Pushing notification {NotificationId} to the portal failed", notification.Id);
            }
        }
    }
}
