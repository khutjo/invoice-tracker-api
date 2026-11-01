using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.Business.Api.Models.Request;

public sealed class CustomerSearchQuery
{
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

    public bool IncludeArchived
    {
        get; init;
    }
}
