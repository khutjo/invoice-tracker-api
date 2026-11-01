namespace InvoiceTracker.Business.Api.Models.Response;

public sealed record CustomerSummaryResponse
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

    public string? PhoneNumber
    {
        get; init;
    }

    public string? City
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
}
