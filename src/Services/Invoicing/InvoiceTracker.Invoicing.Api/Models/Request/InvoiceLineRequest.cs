using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.Invoicing.Api.Models.Request;

public sealed class InvoiceLineRequest
{
    [Required, StringLength(500)]
    public required string Description
    {
        get; init;
    }

    /// <summary>Up to 3 decimals, for example 1.5 hours.</summary>
    [Range(0.001, 1_000_000)]
    public decimal Quantity
    {
        get; init;
    }

    /// <summary>Excluding VAT, in rand, up to 2 decimals.</summary>
    [Range(0.0, 100_000_000.0)]
    public decimal UnitPrice
    {
        get; init;
    }

    /// <summary>0.15 for standard VAT, 0 for zero-rated or exempt.</summary>
    [Range(0.0, 0.9999)]
    public decimal VatRate
    {
        get; init;
    } = 0.15m;
}
