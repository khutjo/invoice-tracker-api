namespace InvoiceTracker.Invoicing.Domain.Invoices;

/// <summary>
/// The customer's details as they were when the invoice was sent. A sent tax invoice must not change
/// when the customer is edited later, and background jobs (overdue scanner, notifications) read them from here.
/// </summary>
public sealed class BilledCustomer
{
    private BilledCustomer()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? VatNumber { get; private set; }
    public string? AddressLine1 { get; private set; }
    public string? AddressLine2 { get; private set; }
    public string? Suburb { get; private set; }
    public string? City { get; private set; }
    public string? PostalCode { get; private set; }

    public static BilledCustomer Create(
        string name,
        string email,
        string? vatNumber,
        string? addressLine1,
        string? addressLine2,
        string? suburb,
        string? city,
        string? postalCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return new BilledCustomer
        {
            Name = name.Trim(),
            Email = email.Trim(),
            VatNumber = NullIfBlank(vatNumber),
            AddressLine1 = NullIfBlank(addressLine1),
            AddressLine2 = NullIfBlank(addressLine2),
            Suburb = NullIfBlank(suburb),
            City = NullIfBlank(city),
            PostalCode = NullIfBlank(postalCode)
        };
    }

    private static string? NullIfBlank(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
