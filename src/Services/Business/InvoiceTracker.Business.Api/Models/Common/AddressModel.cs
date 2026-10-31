using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.Business.Api.Models.Common;

public sealed record AddressModel
{
    [Required, StringLength(200)]
    public required string Line1
    {
        get; init;
    }

    [StringLength(200)]
    public string? Line2
    {
        get; init;
    }

    [Required, StringLength(100)]
    public required string Suburb
    {
        get; init;
    }

    [Required, StringLength(100)]
    public required string City
    {
        get; init;
    }

    [Required, RegularExpression(@"^\d{4}$", ErrorMessage = "Postal code must be 4 digits.")]
    public required string PostalCode
    {
        get; init;
    }
}
