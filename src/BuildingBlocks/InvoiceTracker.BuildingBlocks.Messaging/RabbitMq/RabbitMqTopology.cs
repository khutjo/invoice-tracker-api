using InvoiceTracker.BuildingBlocks.Contracts.Events.IdentityModels;
using RabbitMQ.Client;

namespace InvoiceTracker.BuildingBlocks.Messaging.RabbitMq;

public static class RabbitMqTopology
{
    public const string Exchange = "invoice-tracker.events";
    public const string BusinessQueue = "business.events";
    public const string NotificationQueue = "notification.events";

    public static readonly IReadOnlyDictionary<string, string[]> Queues = new Dictionary<string, string[]>
    {
        [BusinessQueue] = [nameof(BusinessRegistered)],
        [NotificationQueue] = [nameof(EmailVerificationRequested)]
    };

    public static async Task DeclareAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            exchange: Exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        foreach (var (queue, routingKeys) in Queues)
        {
            await channel.QueueDeclareAsync(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            foreach (var routingKey in routingKeys)
            {
                await channel.QueueBindAsync(
                    queue: queue,
                    exchange: Exchange,
                    routingKey: routingKey,
                    cancellationToken: cancellationToken);
            }
        }
    }
}
