namespace InvoiceTracker.Identity.Api.Exceptions;

public sealed class LoginException(string message, int statusCode) : System.Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
