using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.Invoicing.Api.Jobs;

public sealed class OverdueScanOptions
{
    public const string SectionName = "OverdueScan";

    [Range(typeof(TimeSpan), "00:00:05", "1.00:00:00")]
    public TimeSpan Interval
    {
        get; init;
    } = TimeSpan.FromMinutes(1);

    [Range(1, 1000)]
    public int BatchSize
    {
        get; init;
    } = 100;

    public bool AllowAsOfOverride
    {
        get; init;
    }
}
