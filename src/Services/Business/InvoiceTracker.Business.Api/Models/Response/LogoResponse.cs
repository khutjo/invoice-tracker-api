namespace InvoiceTracker.Business.Api.Models.Response;

public sealed record LogoResponse
{
    public required byte[] Content
    {
        get; init;
    }

    public required string ContentType
    {
        get; init;
    }

    public required string FileName
    {
        get; init;
    }
}
