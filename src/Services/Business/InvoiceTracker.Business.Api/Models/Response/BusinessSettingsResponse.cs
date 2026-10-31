namespace InvoiceTracker.Business.Api.Models.Response;

public sealed record BusinessSettingsResponse
{
    public required int DefaultPaymentTermsDays
    {
        get; init;
    }

    public required string TimeZone
    {
        get; init;
    }

    /// <summary>Send back on the next update; it changes on every save.</summary>
    public required string Version
    {
        get; init;
    }
}
