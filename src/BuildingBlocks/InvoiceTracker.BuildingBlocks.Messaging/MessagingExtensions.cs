using InvoiceTracker.BuildingBlocks.Messaging.Abstractions;
using InvoiceTracker.BuildingBlocks.Messaging.Consuming;
using InvoiceTracker.BuildingBlocks.Messaging.Outbox;
using InvoiceTracker.BuildingBlocks.Messaging.RabbitMq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InvoiceTracker.BuildingBlocks.Messaging;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddOptions<RabbitMqOptions>()
            .BindConfiguration(RabbitMqOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

        return services;
    }

    public static IServiceCollection AddOutboxPublisher<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddHostedService<OutboxPublisherService<TDbContext>>();
        return services;
    }

    /// <summary>
    /// Consumes <paramref name="queue"/> in the background. Chain <c>AddHandler</c> for each event type it carries.
    /// Needs <see cref="AddMessaging"/> for the connection.
    /// </summary>
    public static EventConsumerBuilder AddEventConsumer(this IServiceCollection services, string queue)
    {
        if (!RabbitMqTopology.Queues.ContainsKey(queue))
        {
            throw new ArgumentException($"Queue '{queue}' is not declared in RabbitMqTopology.Queues.", nameof(queue));
        }

        var registry = new EventHandlerRegistry();

        // Not AddHostedService: it de-duplicates by implementation type, which would drop a second queue's consumer
        services.AddSingleton<IHostedService>(provider => new RabbitMqConsumerService(
            queue,
            registry,
            provider.GetRequiredService<RabbitMqConnection>(),
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<ILogger<RabbitMqConsumerService>>()));

        return new EventConsumerBuilder(services, queue, registry);
    }
}
