namespace InvoiceTracker.Business.Api.Models.Response;

public sealed record SaveLogoResponse
{
    public required string ContentType
    {
        get; init;
    }

    public required long SizeBytes
    {
        get; init;
    }

    public required string Version
    {
        get; init;
    }
}
