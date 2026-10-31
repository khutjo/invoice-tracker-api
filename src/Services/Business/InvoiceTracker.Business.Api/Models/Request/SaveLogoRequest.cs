using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.Business.Api.Models.Request;

public sealed class SaveLogoRequest
{
    [Required]
    public required IFormFile File
    {
        get; init;
    }
}
