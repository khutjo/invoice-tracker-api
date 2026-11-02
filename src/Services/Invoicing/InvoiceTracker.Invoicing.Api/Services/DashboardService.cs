using InvoiceTracker.Invoicing.Api.Interfaces;
using InvoiceTracker.Invoicing.Api.Models.Common;
using InvoiceTracker.Invoicing.Api.Models.Response;
using InvoiceTracker.Invoicing.Application.Time;
using InvoiceTracker.Invoicing.Domain.Invoices;
using InvoiceTracker.Invoicing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Invoicing.Api.Services;

public sealed class DashboardService(InvoicingDbContext dbContext, BusinessCalendar calendar) : IDashboardService
{
    public async Task<DashboardDetailsResponse> GetDashboardDetailsAsync(Guid businessId, CancellationToken cancellationToken)
    {
        var today = calendar.Today();
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);

        var invoices = dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.BusinessId == businessId);

        var openInvoices = await invoices
            .Where(i => i.Status == InvoiceStatus.Sent || i.Status == InvoiceStatus.Overdue)
            .Select(i => new { i.Status, i.DueDate, i.Balance })
            .ToListAsync(cancellationToken);

        var overdueInvoices = openInvoices
            .Where(i => i.Status == InvoiceStatus.Overdue || i.DueDate < today)
            .ToList();

        var oldestDueDate = overdueInvoices.Min(i => i.DueDate);

        var amountReceived = await invoices
            .SelectMany(i => i.Payments)
            .Where(p => p.PaidOn >= monthStart && p.PaidOn < nextMonthStart)
            .SumAsync(p => p.Amount, cancellationToken);

        var paidInFullCount = await invoices
            .Where(i => i.Status == InvoiceStatus.Paid)
            .Where(i => i.Payments.Max(p => (DateOnly?)p.PaidOn) >= monthStart
                        && i.Payments.Max(p => (DateOnly?)p.PaidOn) < nextMonthStart)
            .CountAsync(cancellationToken);

        var drafts = await invoices
            .Where(i => i.Status == InvoiceStatus.Draft)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Total = g.Sum(i => i.Total) })
            .SingleOrDefaultAsync(cancellationToken);

        var outstanding = new Outstanding(openInvoices.Sum(i => i.Balance), openInvoices.Count);
        var overdue = new Overdue(
            overdueInvoices.Sum(i => i.Balance),
            overdueInvoices.Count,
            oldestDueDate is { } due ? Math.Max(0, today.DayNumber - due.DayNumber) : null);
        var paidThisMonth = new PaidThisMonth(amountReceived, paidInFullCount);
        var draftPipeline = new DraftPipeline(drafts?.Total ?? 0, drafts?.Count ?? 0);

        return new DashboardDetailsResponse
        {
            AsOf = today,
            TotalOutstanding = outstanding,
            Overdue = overdue,
            PaidThisMonth = paidThisMonth,
            Drafts = draftPipeline
        };
    }
}
