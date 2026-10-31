using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.Business.Api.Models.Request;

public sealed class UpdateBusinessDetailsRequest : BusinessDetailsRequest
{
    [Required]
    public required string Version
    {
        get; init;
    }
}
