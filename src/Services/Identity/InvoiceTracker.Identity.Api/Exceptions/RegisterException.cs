namespace InvoiceTracker.Identity.Api.Exceptions;

public sealed class RegisterException(
    string message,
    int statusCode,
    IDictionary<string, string[]>? errors = null) : System.Exception(message)
{
    public int StatusCode { get; } = statusCode;

    public IDictionary<string, string[]>? Errors { get; } = errors;
}
