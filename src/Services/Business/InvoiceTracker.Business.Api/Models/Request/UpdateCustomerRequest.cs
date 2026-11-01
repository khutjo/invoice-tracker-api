using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.Business.Api.Models.Request;


public sealed class UpdateCustomerRequest : CustomerDetailsRequest
{
    [Required]
    public required string Version
    {
        get; init;
    }
}
