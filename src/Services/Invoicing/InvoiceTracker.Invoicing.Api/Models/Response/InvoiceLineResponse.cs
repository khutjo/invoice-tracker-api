namespace InvoiceTracker.Invoicing.Api.Models.Response;

public sealed record InvoiceLineResponse
{
    public required Guid InvoiceLineId
    {
        get; init;
    }

    public required int LineNumber
    {
        get; init;
    }

    public required string Description
    {
        get; init;
    }

    public required decimal Quantity
    {
        get; init;
    }

    public required decimal UnitPrice
    {
        get; init;
    }

    public required decimal VatRate
    {
        get; init;
    }

    public required decimal LineTotal
    {
        get; init;
    }

    public required decimal VatAmount
    {
        get; init;
    }
}
