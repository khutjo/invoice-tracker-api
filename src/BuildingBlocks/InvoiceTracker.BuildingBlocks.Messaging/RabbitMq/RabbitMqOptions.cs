using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.BuildingBlocks.Messaging.RabbitMq;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    [Required]
    public string HostName { get; init; } = "localhost";

    [Range(1, 65535)]
    public int Port { get; init; } = 5672;

    [Required]
    public string VirtualHost { get; init; } = "/";

    [Required]
    public string UserName { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;

    [Required]
    public string ClientName { get; init; } = string.Empty;
}
