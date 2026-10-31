namespace InvoiceTracker.Business.Api.Models.Response;

/// <summary>Everything the portal needs to fill its business dropdowns, in one call.</summary>
public sealed record LookupsResponse
{
    public required IReadOnlyList<LookupOption> ContactNumberTypes
    {
        get; init;
    }

    public required IReadOnlyList<LookupOption> BankAccountTypes
    {
        get; init;
    }

    public required IReadOnlyList<BankOption> Banks
    {
        get; init;
    }

    public required IReadOnlyList<LookupOption> TimeZones
    {
        get; init;
    }

    public required string DefaultTimeZone
    {
        get; init;
    }

    public required IReadOnlyList<PaymentTermOption> PaymentTerms
    {
        get; init;
    }

    public required int DefaultPaymentTermsDays
    {
        get; init;
    }
}

/// <summary>Value is what the API accepts; Label is what the portal shows.</summary>
public sealed record LookupOption(string Value, string Label);

/// <summary>Picking a bank fills in its universal branch code.</summary>
public sealed record BankOption(string Value, string Label, string BranchCode);

public sealed record PaymentTermOption(int Days, string Label);

/// <summary>A suburb for a postal code, so the portal can fill suburb and city.</summary>
public sealed record PostalCodeOption(string Code, string Suburb, string Town, string Kind);
