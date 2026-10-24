using InvoiceTracker.BuildingBlocks.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InvoiceTracker.BuildingBlocks.Messaging.Outbox;

public sealed class OutboxPublisherService<TDbContext>(
    IServiceScopeFactory scopeFactory,
    IEventPublisher publisher,
    TimeProvider timeProvider,
    ILogger<OutboxPublisherService<TDbContext>> logger) : BackgroundService
    where TDbContext : DbContext
{
    private const int BatchSize = 50;
    private const int MaxErrorLength = 2000;
    private static readonly TimeSpan _interval = TimeSpan.FromSeconds(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval, timeProvider);

        do
        {
            try
            {
                await PublishBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Outbox batch for {DbContext} failed", typeof(TDbContext).Name);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task PublishBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            dbContext.ChangeTracker.Clear();
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var messages = await dbContext.Set<OutboxMessage>()
                .FromSql($"""
                    SELECT TOP ({BatchSize}) *
                    FROM outbox_messages WITH (UPDLOCK, READPAST, ROWLOCK)
                    WHERE processed_at IS NULL
                    ORDER BY occurred_at
                    """)
                .ToListAsync(cancellationToken);

            if (messages.Count == 0)
            {
                return;
            }

            foreach (var message in messages)
            {
                try
                {
                    await publisher.PublishAsync(message.Type, message.Id, message.Payload, cancellationToken);
                    message.ProcessedAt = timeProvider.GetUtcNow().UtcDateTime;
                    message.Error = null;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    message.Attempts++;
                    message.Error = ex.Message.Length > MaxErrorLength ? ex.Message[..MaxErrorLength] : ex.Message;
                    logger.LogWarning(ex, "Publishing {EventType} {EventId} failed (attempt {Attempts})",
                        message.Type, message.Id, message.Attempts);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }
}
