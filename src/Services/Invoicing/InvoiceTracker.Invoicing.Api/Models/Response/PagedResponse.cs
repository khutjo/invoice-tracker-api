namespace InvoiceTracker.Invoicing.Api.Models.Response;

public sealed record PagedResponse<T>
{
    public required IReadOnlyList<T> Items
    {
        get; init;
    }

    public required int Page
    {
        get; init;
    }

    public required int PageSize
    {
        get; init;
    }

    public required int TotalCount
    {
        get; init;
    }
}

