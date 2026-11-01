using InvoiceTracker.Business.Api.Models.Common;

namespace InvoiceTracker.Business.Api.Models.Response;

public sealed record CustomerDetailsResponse
{
    public required Guid CustomerId
    {
        get; init;
    }

    public required string Name
    {
        get; init;
    }

    public required string Email
    {
        get; init;
    }

    public ContactNumberModel? ContactNumber
    {
        get; init;
    }

    public AddressModel? BillingAddress
    {
        get; init;
    }

    public string? VatNumber
    {
        get; init;
    }

    public string? CustomerReference
    {
        get; init;
    }

    public required bool IsArchived
    {
        get; init;
    }

    public required string Version
    {
        get; init;
    }
}
