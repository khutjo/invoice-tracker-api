using System.Diagnostics.CodeAnalysis;
using InvoiceTracker.BuildingBlocks.Contracts;
using InvoiceTracker.BuildingBlocks.Messaging.Abstractions;
using InvoiceTracker.BuildingBlocks.Messaging.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceTracker.BuildingBlocks.Messaging.Consuming;

/// <summary>
/// Maps an event type name (the message's <c>type</c> property) to a typed handler,
/// so the consumer can dispatch without reflection.
/// </summary>
public sealed class EventHandlerRegistry
{
    private readonly Dictionary<string, Func<IServiceProvider, string, CancellationToken, Task>> _handlers =
        new(StringComparer.Ordinal);

    public IReadOnlyCollection<string> EventTypes => _handlers.Keys;

    public void Register<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventType = typeof(TEvent).Name;
        if (!_handlers.TryAdd(eventType, HandleAsync))
        {
            throw new InvalidOperationException($"A handler for {eventType} is already registered.");
        }

        static Task HandleAsync(IServiceProvider services, string payload, CancellationToken cancellationToken)
        {
            var integrationEvent = EventSerializer.Deserialize<TEvent>(payload);
            return services.GetRequiredService<THandler>().HandleAsync(integrationEvent, cancellationToken);
        }
    }

    public bool TryGetHandler(
        string eventType,
        [NotNullWhen(true)] out Func<IServiceProvider, string, CancellationToken, Task>? handler)
    {
        return _handlers.TryGetValue(eventType, out handler);
    }
}
