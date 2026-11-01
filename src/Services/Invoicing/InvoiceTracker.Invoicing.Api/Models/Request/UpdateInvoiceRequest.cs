using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.Invoicing.Api.Models.Request;

public sealed class UpdateInvoiceRequest : InvoiceDetailsRequest
{
    [Required]
    public required string Version
    {
        get; init;
    }
}
