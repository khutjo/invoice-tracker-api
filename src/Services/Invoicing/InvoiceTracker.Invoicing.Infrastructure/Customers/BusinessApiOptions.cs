using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.Invoicing.Infrastructure.Customers;

public sealed class BusinessApiOptions
{
    public const string SectionName = "Services:Business";

    /// <summary>The Business service itself, not the gateway: http://localhost:5224 locally, http://business:8080 in Docker.</summary>
    [Required, Url]
    public string BaseUrl { get; init; } = string.Empty;
}
