namespace InvoiceTracker.Invoicing.Application.Time;

/// <summary>
/// "Today" as the business sees it. Issue dates and overdue checks use the business's local date,
/// not the UTC date, so an invoice sent at 01:00 in Johannesburg is dated that day.
/// </summary>
public sealed class BusinessCalendar(TimeProvider timeProvider)
{
    // Every business is South African for now; a per-business zone comes with the business snapshots
    private static readonly TimeZoneInfo _southAfrica = TimeZoneInfo.FindSystemTimeZoneById("Africa/Johannesburg");

    public DateOnly Today()
    {
        var local = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), _southAfrica);
        return DateOnly.FromDateTime(local.DateTime);
    }
}
