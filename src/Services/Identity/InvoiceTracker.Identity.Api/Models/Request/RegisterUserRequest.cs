using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.Identity.Api.Models.Request;

public sealed class RegisterUserRequest
{
    [Required, StringLength(200)]
    public required string BusinessName
    {
        get; init;
    }

    [Required, StringLength(100)]
    public required string FirstName
    {
        get; init;
    }

    [Required, StringLength(100)]
    public required string LastName
    {
        get; init;
    }

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
