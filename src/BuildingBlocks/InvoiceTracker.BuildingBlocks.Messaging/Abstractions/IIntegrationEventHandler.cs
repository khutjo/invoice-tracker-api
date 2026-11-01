using InvoiceTracker.BuildingBlocks.Contracts;

namespace InvoiceTracker.BuildingBlocks.Messaging.Abstractions;

/// <summary>
/// Handles one integration event type. Delivery is at-least-once, so implementations must be idempotent.
/// </summary>
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IntegrationEvent
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken);
}
