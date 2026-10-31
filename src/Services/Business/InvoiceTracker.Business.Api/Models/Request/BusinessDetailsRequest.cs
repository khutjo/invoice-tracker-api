using System.ComponentModel.DataAnnotations;
using InvoiceTracker.Business.Api.Models.Common;

namespace InvoiceTracker.Business.Api.Models.Request;

public abstract class BusinessDetailsRequest
{
    [Required, StringLength(200)]
    public required string Name
    {
        get; init;
    }

    [RegularExpression(@"^\d{4}/\d{6}/\d{2}$", ErrorMessage = "Use the CIPC format, for example 2019/123456/07.")]
    public string? RegistrationNumber
    {
        get; init;
    }

    [RegularExpression(@"^4\d{9}$", ErrorMessage = "VAT number must be 10 digits starting with 4.")]
    public string? VatNumber
    {
        get; init;
    }

    [EmailAddress, StringLength(256)]
    public string? SupportEmail
    {
        get; init;
    }

    public ContactNumberModel? ContactNumber
    {
        get; init;
    }

    public AddressModel? Address
    {
        get; init;
    }

    public BankDetailsModel? BankDetails
    {
        get; init;
    }
}
