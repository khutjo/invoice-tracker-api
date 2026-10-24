using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.Identity.Api.Models.Request;

public sealed class VerifyEmailRequest
{
    [Required]
    public required Guid UserId
    {
        get; init;
    }

    [Required, StringLength(1024)]
    public required string Token
    {
        get; init;
    }
}
