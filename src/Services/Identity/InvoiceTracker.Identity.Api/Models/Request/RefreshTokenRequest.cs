using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.Identity.Api.Models.Request;

public sealed class RefreshTokenRequest
{
    [Required, StringLength(128)]
    public required string RefreshToken
    {
        get; init;
    }
}
