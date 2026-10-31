namespace InvoiceTracker.Business.Api.Exceptions;

public sealed class BusinessException(
    string message,
    int statusCode,
    IDictionary<string, string[]>? errors = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;

    public IDictionary<string, string[]>? Errors { get; } = errors;

    public static BusinessException NotFound()
    {
        return new BusinessException("The business profile has not been created yet.", StatusCodes.Status404NotFound);
    }

    public static BusinessException StaleVersion()
    {
        return new BusinessException(
            "The business profile was changed by someone else. Reload it and try again.",
            StatusCodes.Status409Conflict);
    }

    public static BusinessException Invalid(string field, string error)
    {
        return Invalid(new Dictionary<string, string[]> { [field] = [error] });
    }

    public static BusinessException Invalid(IDictionary<string, string[]> errors)
    {
        return new BusinessException("One or more validation errors occurred.", StatusCodes.Status400BadRequest, errors);
    }
}
