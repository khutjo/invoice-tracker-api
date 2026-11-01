namespace InvoiceTracker.Invoicing.Application.Customers;

/// <summary>
/// Looks customers up in the Business service, as the signed-in user, so Business applies its own tenant filter.
/// Only usable inside an HTTP request: background jobs read the details frozen on the invoice instead.
/// </summary>
public interface ICustomerDirectory
{
    /// <summary>Null when the customer doesn't exist or belongs to another business.</summary>
    /// <exception cref="CustomerDirectoryUnavailableException">The Business service could not be reached or failed.</exception>
    Task<CustomerInfo?> GetAsync(Guid customerId, CancellationToken cancellationToken);
}
