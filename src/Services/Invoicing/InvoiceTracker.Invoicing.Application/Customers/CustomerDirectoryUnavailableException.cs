namespace InvoiceTracker.Invoicing.Application.Customers;

/// <summary>The Business service is down, timed out or answered with an error. The API answers 503.</summary>
public sealed class CustomerDirectoryUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
