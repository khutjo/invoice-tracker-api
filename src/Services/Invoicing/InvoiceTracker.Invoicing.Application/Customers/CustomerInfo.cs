namespace InvoiceTracker.Invoicing.Application.Customers;

/// <summary>Invoicing's view of a customer owned by the Business service.</summary>
public sealed record CustomerInfo(
    Guid CustomerId,
    string Name,
    string Email,
    string? VatNumber,
    CustomerAddress? BillingAddress,
    bool IsArchived);

public sealed record CustomerAddress(string Line1, string? Line2, string Suburb, string City, string PostalCode);
