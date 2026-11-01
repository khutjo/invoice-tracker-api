namespace InvoiceTracker.Invoicing.Api.Exceptions;

public sealed class InvoiceException(
    string message,
    int statusCode,
    IDictionary<string, string[]>? errors = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;

    public IDictionary<string, string[]>? Errors { get; } = errors;

    /// <summary>Also the answer for another business's invoice, so ids from other tenants reveal nothing.</summary>
    public static InvoiceException NotFound()
    {
        return new InvoiceException("The invoice was not found.", StatusCodes.Status404NotFound);
    }

    public static InvoiceException StaleVersion()
    {
        return new InvoiceException(
            "The invoice was changed by someone else. Reload it and try again.",
            StatusCodes.Status409Conflict);
    }

    public static InvoiceException CustomerNotInvoiceable()
    {
        return new InvoiceException(
            "The customer on this invoice was archived or no longer exists. Choose another customer before sending.",
            StatusCodes.Status409Conflict);
    }

    public static InvoiceException Invalid(string field, string error)
    {
        return Invalid(new Dictionary<string, string[]> { [field] = [error] });
    }

    public static InvoiceException Invalid(IDictionary<string, string[]> errors)
    {
        return new InvoiceException("One or more validation errors occurred.", StatusCodes.Status400BadRequest, errors);
    }
}
