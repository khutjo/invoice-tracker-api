using InvoiceTracker.BuildingBlocks.Contracts;
using InvoiceTracker.BuildingBlocks.Messaging.Abstractions;
using InvoiceTracker.BuildingBlocks.Messaging.RabbitMq;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceTracker.BuildingBlocks.Messaging.Consuming;

public sealed class EventConsumerBuilder(IServiceCollection services, string queue, EventHandlerRegistry registry)
{
    public EventConsumerBuilder AddHandler<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : class, IIntegrationEventHandler<TEvent>
    {
        var eventType = typeof(TEvent).Name;
        if (!RabbitMqTopology.Queues[queue].Contains(eventType))
        {
            throw new InvalidOperationException(
                $"Queue '{queue}' is not bound to {eventType}. Add it to RabbitMqTopology.Queues.");
        }

        services.AddScoped<THandler>();
        registry.Register<TEvent, THandler>();

        return this;
    }
}
