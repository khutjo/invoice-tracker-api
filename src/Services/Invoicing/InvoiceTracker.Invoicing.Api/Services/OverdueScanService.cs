using InvoiceTracker.BuildingBlocks.Messaging.Outbox;
using InvoiceTracker.Invoicing.Api.Interfaces;
using InvoiceTracker.Invoicing.Api.Jobs;
using InvoiceTracker.Invoicing.Application.Time;
using InvoiceTracker.Invoicing.Domain.Invoices;
using InvoiceTracker.Invoicing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ContractReminderStage = InvoiceTracker.BuildingBlocks.Contracts.Events.InvoicingModels.OverdueReminderStage;
using InvoiceOverdue = InvoiceTracker.BuildingBlocks.Contracts.Events.InvoicingModels.InvoiceOverdue;

namespace InvoiceTracker.Invoicing.Api.Services;

public sealed class OverdueScanService(
    InvoicingDbContext dbContext,
    BusinessCalendar businessCalendar,
    TimeProvider timeProvider,
    IOptions<OverdueScanOptions> options,
    ILogger<OverdueScanService> logger) : IOverdueScanService
{
    public async Task<OverdueScanResult> RunAsync(Guid? businessId, DateOnly? asOf, CancellationToken cancellationToken)
    {
        var today = asOf ?? businessCalendar.Today();
        var batchSize = options.Value.BatchSize;
        var markedOverdue = 0;
        var remindersRaised = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var batch = await DueForAction(today, businessId)
                .OrderBy(i => i.DueDate)
                .ThenBy(i => i.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (batch.Count == 0)
            {
                break;
            }

            var changed = 0;
            var now = timeProvider.GetUtcNow().UtcDateTime;

            foreach (var invoice in batch)
            {
                var wasSent = invoice.Status == InvoiceStatus.Sent;
                var stage = invoice.CheckOverdue(today, now);

                if (wasSent && invoice.Status == InvoiceStatus.Overdue)
                {
                    markedOverdue++;
                    changed++;
                }

                if (stage is { } raised)
                {
                    dbContext.AddOutboxMessage(ToInvoiceOverdue(invoice, raised, today));
                    remindersRaised++;
                    changed++;
                }
            }

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogInformation("An invoice changed during the overdue scan; it will be checked again on the next run");
            }

            dbContext.ChangeTracker.Clear();

            if (batch.Count < batchSize || changed == 0)
            {
                break;
            }
        }

        if (markedOverdue > 0 || remindersRaised > 0)
        {
            logger.LogInformation("Overdue scan for {Today}: {MarkedOverdue} marked overdue, {RemindersRaised} reminders raised",
                today, markedOverdue, remindersRaised);
        }

        return new OverdueScanResult(today, markedOverdue, remindersRaised);
    }

    private IQueryable<Invoice> DueForAction(DateOnly today, Guid? businessId)
    {
        var weekAgo = today.AddDays(-7);
        var fortnightAgo = today.AddDays(-14);

        var invoices = dbContext.Invoices.Where(i =>
            (i.Status == InvoiceStatus.Sent && i.DueDate < today)
            || (i.Status == InvoiceStatus.Overdue && i.ReminderStage < OverdueReminderStage.Second && i.DueDate <= weekAgo)
            || (i.Status == InvoiceStatus.Overdue && i.ReminderStage < OverdueReminderStage.Final && i.DueDate <= fortnightAgo));

        if (businessId is { } id)
        {
            invoices = invoices.Where(i => i.BusinessId == id);
        }

        return invoices;
    }

    private InvoiceOverdue ToInvoiceOverdue(Invoice invoice, OverdueReminderStage stage, DateOnly today)
    {
        var billedCustomer = invoice.BilledCustomer
            ?? throw new InvalidOperationException($"Overdue invoice {invoice.Id} has no billed customer.");

        return new InvoiceOverdue
        {
            OccurredAt = timeProvider.GetUtcNow(),
            BusinessId = invoice.BusinessId,
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber
                ?? throw new InvalidOperationException($"Overdue invoice {invoice.Id} has no number."),
            CustomerId = invoice.CustomerId,
            CustomerName = billedCustomer.Name,
            CustomerEmail = billedCustomer.Email,
            DueDate = invoice.DueDate
                ?? throw new InvalidOperationException($"Overdue invoice {invoice.Id} has no due date."),
            DaysOverdue = invoice.DaysOverdue(today),
            Balance = invoice.Balance,
            ReminderStage = (ContractReminderStage)(int)stage
        };
    }
}
