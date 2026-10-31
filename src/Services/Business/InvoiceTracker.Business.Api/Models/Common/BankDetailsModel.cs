using System.ComponentModel.DataAnnotations;
using InvoiceTracker.Business.Domain.Businesses;

namespace InvoiceTracker.Business.Api.Models.Common;

public sealed record BankDetailsModel
{
    [Required, StringLength(100)]
    public required string BankName
    {
        get; init;
    }

    [Required, StringLength(200)]
    public required string AccountHolder
    {
        get; init;
    }

    [Required, RegularExpression(@"^\d{6,16}$", ErrorMessage = "Account number must be 6 to 16 digits.")]
    public required string AccountNumber
    {
        get; init;
    }

    [Required]
    public required BankAccountType AccountType
    {
        get; init;
    }

    [Required, RegularExpression(@"^\d{6}$", ErrorMessage = "Branch code must be 6 digits.")]
    public required string BranchCode
    {
        get; init;
    }

    [StringLength(100)]
    public string? BranchName
    {
        get; init;
    }
}
