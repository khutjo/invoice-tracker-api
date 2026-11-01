using System.Text;
using System.Text.Json;
using InvoiceTracker.BuildingBlocks.Messaging.Consuming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace InvoiceTracker.BuildingBlocks.Messaging.RabbitMq;

/// <summary>
/// Consumes one queue with manual acknowledgements and dispatches each message to its handler in a new DI scope.
/// Unreadable JSON is rejected; any other failure is requeued once, then rejected (a dead-letter queue is future work).
/// </summary>
public sealed class RabbitMqConsumerService(
    string queue,
    EventHandlerRegistry registry,
    RabbitMqConnection connection,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqConsumerService> logger) : BackgroundService
{
    private const ushort PrefetchCount = 10;
    private static readonly TimeSpan _connectRetryDelay = TimeSpan.FromSeconds(5);

    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var channel = await OpenChannelAsync(stoppingToken);
            _channel = channel;

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (_, delivery) => HandleDeliveryAsync(channel, delivery, stoppingToken);

            await channel.BasicConsumeAsync(queue, autoAck: false, consumer, stoppingToken);
            logger.LogInformation("Consuming {Queue} for {EventTypes}", queue, string.Join(", ", registry.EventTypes));

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host is stopping
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        if (_channel is not null)
        {
            // Closing the channel hands any unacknowledged messages back to the broker
            await _channel.DisposeAsync();
            _channel = null;
        }
    }

    private async Task<IChannel> OpenChannelAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            IChannel? channel = null;
            try
            {
                var openConnection = await connection.GetAsync(stoppingToken);
                channel = await openConnection.CreateChannelAsync(cancellationToken: stoppingToken);

                // Declare the topology here too, so this service can start before any publisher has run
                await RabbitMqTopology.DeclareAsync(channel, stoppingToken);
                await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: PrefetchCount, global: false, stoppingToken);

                return channel;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (channel is not null)
                {
                    await channel.DisposeAsync();
                }

                logger.LogWarning(ex, "Could not open a RabbitMQ channel for {Queue}; retrying in {Delay}",
                    queue, _connectRetryDelay);
                await Task.Delay(_connectRetryDelay, stoppingToken);
            }
        }
    }

    private async Task HandleDeliveryAsync(IChannel channel, BasicDeliverEventArgs delivery, CancellationToken stoppingToken)
    {
        var eventType = delivery.BasicProperties.Type ?? string.Empty;
        var eventId = delivery.BasicProperties.MessageId ?? string.Empty;

        using var logScope = logger.BeginScope(new Dictionary<string, object>
        {
            ["Queue"] = queue,
            ["EventType"] = eventType,
            ["EventId"] = eventId
        });

        if (!registry.TryGetHandler(eventType, out var handler))
        {
            logger.LogWarning("No handler for {EventType} {EventId} on {Queue}; acknowledging and skipping it",
                eventType, eventId, queue);
            await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, CancellationToken.None);
            return;
        }

        try
        {
            // The body buffer is only valid during this callback, so decode it first
            var payload = Encoding.UTF8.GetString(delivery.Body.Span);

            await using var scope = scopeFactory.CreateAsyncScope();
            await handler(scope.ServiceProvider, payload, stoppingToken);

            await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, CancellationToken.None);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutting down: leave it unacknowledged so the broker redelivers it
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "{EventType} {EventId} could not be read; rejecting it without requeue", eventType, eventId);
            await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false, CancellationToken.None);
        }
        catch (Exception ex)
        {
            var requeue = !delivery.Redelivered;
            logger.LogError(ex, "Handling {EventType} {EventId} failed; requeue: {Requeue}", eventType, eventId, requeue);
            await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue, CancellationToken.None);
        }
    }
}
