using System.Text.Json;
using InvoiceTracker.BuildingBlocks.Contracts;

namespace InvoiceTracker.BuildingBlocks.Messaging.Serialization;

public static class EventSerializer
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    public static string TypeName(IntegrationEvent integrationEvent)
    {
        return integrationEvent.GetType().Name;
    }

    public static string Serialize(IntegrationEvent integrationEvent)
    {
        return JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), _options);
    }

    public static TEvent Deserialize<TEvent>(string payload)
        where TEvent : IntegrationEvent
    {
        return JsonSerializer.Deserialize<TEvent>(payload, _options)
            ?? throw new JsonException($"Payload could not be read as {typeof(TEvent).Name}.");
    }
}
