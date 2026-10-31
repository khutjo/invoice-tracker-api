using System.ComponentModel.DataAnnotations;
using InvoiceTracker.Business.Domain.Common;

namespace InvoiceTracker.Business.Api.Models.Common;

public sealed record ContactNumberModel
{
    [Required, RegularExpression(@"^\+[1-9]\d{7,14}$", ErrorMessage = "Use international format, for example +27821234567.")]
    public required string Number
    {
        get; init;
    }

    [Required]
    public required ContactNumberType Type
    {
        get; init;
    }
}
