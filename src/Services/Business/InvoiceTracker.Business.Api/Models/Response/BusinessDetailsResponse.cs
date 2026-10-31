using InvoiceTracker.Business.Api.Models.Common;

namespace InvoiceTracker.Business.Api.Models.Response;

public sealed record BusinessDetailsResponse
{
    public required Guid BusinessId
    {
        get; init;
    }

    public required string Name
    {
        get; init;
    }

    public string? RegistrationNumber
    {
        get; init;
    }

    public string? VatNumber
    {
        get; init;
    }

    public string? SupportEmail
    {
        get; init;
    }

    public ContactNumberModel? ContactNumber
    {
        get; init;
    }

    public AddressModel? Address
    {
        get; init;
    }

    public BankDetailsModel? BankDetails
    {
        get; init;
    }

    public required bool HasLogo
    {
        get; init;
    }

    public required string Version
    {
        get; init;
    }
}
