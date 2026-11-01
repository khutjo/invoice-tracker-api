namespace InvoiceTracker.Invoicing.Api.Models.Response;

public sealed record BilledCustomerResponse
{
    public required string Name
    {
        get; init;
    }

    public required string Email
    {
        get; init;
    }

    public string? VatNumber
    {
        get; init;
    }

    public string? AddressLine1
    {
        get; init;
    }

    public string? AddressLine2
    {
        get; init;
    }

    public string? Suburb
    {
        get; init;
    }

    public string? City
    {
        get; init;
    }

    public string? PostalCode
    {
        get; init;
    }
}
