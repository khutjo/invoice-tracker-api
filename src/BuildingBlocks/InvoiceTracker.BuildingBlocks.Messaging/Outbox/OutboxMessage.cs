using InvoiceTracker.BuildingBlocks.Contracts;
using InvoiceTracker.BuildingBlocks.Messaging.Serialization;

namespace InvoiceTracker.BuildingBlocks.Messaging.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int Attempts { get; set; }
    public string? Error { get; set; }

    public static OutboxMessage From(IntegrationEvent integrationEvent)
    {
        return new OutboxMessage
        {
            Id = integrationEvent.EventId,
            Type = EventSerializer.TypeName(integrationEvent),
            Payload = EventSerializer.Serialize(integrationEvent),
            OccurredAt = integrationEvent.OccurredAt.UtcDateTime
        };
    }
}
