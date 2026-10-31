using InvoiceTracker.Business.Domain.Businesses;
using InvoiceTracker.Business.Domain.Common;

namespace InvoiceTracker.Business.Api.Lookups;

/// <summary>
/// The fixed lists the portal shows in dropdowns. Values are what the API accepts; labels are free to change
/// here without a portal deploy. Banks and postal codes come from the database instead.
/// </summary>
public static class LookupCatalog
{
    public const string DefaultTimeZone = "Africa/Johannesburg";
    public const int DefaultPaymentTermsDays = 30;

    public static readonly IReadOnlyList<(ContactNumberType Value, string Label)> ContactNumberTypes =
    [
        (ContactNumberType.Mobile, "Mobile"),
        (ContactNumberType.Landline, "Landline"),
        (ContactNumberType.Work, "Work")
    ];

    public static readonly IReadOnlyList<(BankAccountType Value, string Label)> BankAccountTypes =
    [
        (BankAccountType.Cheque, "Cheque / current"),
        (BankAccountType.Savings, "Savings"),
        (BankAccountType.Transmission, "Transmission")
    ];

    /// <summary>The only time zones settings accept: southern Africa first, then a few for businesses with clients abroad.</summary>
    public static readonly IReadOnlyList<(string Value, string Label)> TimeZones =
    [
        ("Africa/Johannesburg", "South Africa (SAST, UTC+2)"),
        ("Africa/Windhoek", "Namibia (UTC+2)"),
        ("Africa/Gaborone", "Botswana (UTC+2)"),
        ("Africa/Maseru", "Lesotho (UTC+2)"),
        ("Africa/Mbabane", "Eswatini (UTC+2)"),
        ("Africa/Maputo", "Mozambique (UTC+2)"),
        ("Africa/Harare", "Zimbabwe (UTC+2)"),
        ("Africa/Lusaka", "Zambia (UTC+2)"),
        ("Africa/Nairobi", "Kenya (UTC+3)"),
        ("Africa/Lagos", "Nigeria (UTC+1)"),
        ("Europe/London", "United Kingdom (GMT/BST)"),
        ("UTC", "UTC")
    ];

    public static readonly IReadOnlyList<(int Days, string Label)> PaymentTerms =
    [
        (0, "Due on receipt"),
        (7, "7 days"),
        (14, "14 days"),
        (30, "30 days"),
        (60, "60 days"),
        (90, "90 days")
    ];

    public static bool IsSupportedTimeZone(string timeZone)
    {
        return TimeZones.Any(t => t.Value == timeZone);
    }
}
