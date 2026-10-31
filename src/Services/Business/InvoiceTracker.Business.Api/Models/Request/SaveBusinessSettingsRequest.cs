using System.ComponentModel.DataAnnotations;
using InvoiceTracker.Business.Domain.Businesses;

namespace InvoiceTracker.Business.Api.Models.Request;

public sealed class SaveBusinessSettingsRequest
{
    [Range(0, 365)]
    public required int DefaultPaymentTermsDays
    {
        get; init;
    }

    [Required, StringLength(64)]
    public required string TimeZone
    {
        get; init;
    }

    [Required]
    public required string Version
    {
        get; init;
    }
}
