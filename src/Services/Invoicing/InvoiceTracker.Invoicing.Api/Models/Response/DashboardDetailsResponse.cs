using InvoiceTracker.Invoicing.Api.Models.Common;

namespace InvoiceTracker.Invoicing.Api.Models.Response;

public sealed record DashboardDetailsResponse
{
    public required DateOnly AsOf { get; init; }

    public required Outstanding TotalOutstanding { get; init; }

    public required Overdue Overdue { get; init; }

    public required PaidThisMonth PaidThisMonth { get; init; }

    public required DraftPipeline Drafts { get; init; }
}

