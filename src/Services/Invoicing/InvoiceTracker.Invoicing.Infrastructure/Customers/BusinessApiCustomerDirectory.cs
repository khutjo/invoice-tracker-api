using System.Net;
using System.Net.Http.Json;
using InvoiceTracker.Invoicing.Application.Customers;

namespace InvoiceTracker.Invoicing.Infrastructure.Customers;

/// <summary>Reads customers from the Business service's GET /api/v1/customers/{customerId}.</summary>
public sealed class BusinessApiCustomerDirectory(HttpClient httpClient) : ICustomerDirectory
{
    public async Task<CustomerInfo?> GetAsync(Guid customerId, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync($"api/v1/customers/{customerId}", cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            // Unreachable, timed out or circuit open after the resilience handler's retries.
            // A cancelled request (the caller went away) is left to propagate as it is.
            throw new CustomerDirectoryUnavailableException("The Business service could not be reached.", ex);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new CustomerDirectoryUnavailableException(
                    $"The Business service answered {(int)response.StatusCode} for a customer lookup.");
            }

            var customer = await response.Content.ReadFromJsonAsync<BusinessCustomer>(cancellationToken)
                ?? throw new CustomerDirectoryUnavailableException("The Business service returned an empty customer.");

            return new CustomerInfo(
                customer.CustomerId,
                customer.Name,
                customer.Email,
                customer.VatNumber,
                customer.BillingAddress is { } address
                    ? new CustomerAddress(address.Line1, address.Line2, address.Suburb, address.City, address.PostalCode)
                    : null,
                customer.IsArchived);
        }
    }

    // The fields of Business's CustomerDetailsResponse that Invoicing uses. Kept private: this is the
    // wire format, and the rest of Invoicing only sees CustomerInfo.
    private sealed record BusinessCustomer(
        Guid CustomerId,
        string Name,
        string Email,
        string? VatNumber,
        BusinessAddress? BillingAddress,
        bool IsArchived);

    private sealed record BusinessAddress(string Line1, string? Line2, string Suburb, string City, string PostalCode);
}
