using InvoiceTracker.Invoicing.Application.Invoices;
using InvoiceTracker.Invoicing.Domain.Invoices;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Invoicing.Infrastructure.Persistence;

public sealed class InvoiceNumberAllocator(InvoicingDbContext dbContext) : IInvoiceNumberAllocator
{
    public async Task<string> NextAsync(Guid businessId, int year, CancellationToken cancellationToken)
    {
        if (dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("Invoice numbers must be allocated inside the send transaction.");
        }

        var sequence = await dbContext.InvoiceNumberSequences
            .FromSql($"SELECT * FROM invoice_number_sequences WITH (UPDLOCK, HOLDLOCK) WHERE business_id = {businessId} AND year = {year}")
            .SingleOrDefaultAsync(cancellationToken);

        if (sequence is null)
        {
            sequence = InvoiceNumberSequence.Start(businessId, year);
            dbContext.InvoiceNumberSequences.Add(sequence);
        }

        return sequence.Next();
    }
}
