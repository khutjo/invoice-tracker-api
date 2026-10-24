namespace InvoiceTracker.BuildingBlocks.Messaging.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync(string eventType, Guid eventId, string payload, CancellationToken cancellationToken);
}
