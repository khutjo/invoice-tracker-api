using System.ComponentModel.DataAnnotations;
using InvoiceTracker.Invoicing.Domain.Invoices;

namespace InvoiceTracker.Invoicing.Api.Models.Request;

public sealed class InvoiceSearchQuery
{
    /// <summary>Matches part of the invoice number.</summary>
    [StringLength(100)]
    public string? Search
    {
        get; init;
    }

    [Range(1, int.MaxValue)]
    public int Page
    {
        get; init;
    } = 1;

    [Range(1, 100)]
    public int PageSize
    {
        get; init;
    } = 20;

    public InvoiceStatus? Status
    {
        get; init;
    }

    public Guid? CustomerId
    {
        get; init;
    }

    /// <summary>Issue date on or after this day (yyyy-MM-dd).</summary>
    public DateOnly? IssuedFrom
    {
        get; init;
    }

    /// <summary>Issue date on or before this day (yyyy-MM-dd).</summary>
    public DateOnly? IssuedTo
    {
        get; init;
    }

    public bool OverdueOnly
    {
        get; init;
    }
}
