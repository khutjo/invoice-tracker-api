using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.Identity.Api.Models.Request;

public sealed class LogInRequest
{
    [Required, EmailAddress, StringLength(256)]
    public required string Email
    {
        get; init;
    }

    [Required, StringLength(128)]
    public required string Password
    {
        get; init;
    }
}
