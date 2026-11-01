using System.ComponentModel.DataAnnotations;
using InvoiceTracker.Business.Api.Models.Common;

namespace InvoiceTracker.Business.Api.Models.Request;

public class CustomerDetailsRequest
{
    [Required, StringLength(200)]
    public required string Name
    {
        get; init;
    }

    [Required, EmailAddress, StringLength(256)]
    public required string Email
    {
        get; init;
    }

    public ContactNumberModel? ContactNumber
    {
        get; init;
    }

    public AddressModel? BillingAddress
    {
        get; init;
    }

    [RegularExpression(@"^4\d{9}$", ErrorMessage = "VAT number must be 10 digits starting with 4.")]
    public string? VatNumber
    {
        get; init;
    }

    [StringLength(50)]
    public string? CustomerReference
    {
        get; init;
    }
}
