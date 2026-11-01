using InvoiceTracker.Invoicing.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceTracker.Invoicing.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceNumberSequenceConfiguration : IEntityTypeConfiguration<InvoiceNumberSequence>
{
    public void Configure(EntityTypeBuilder<InvoiceNumberSequence> builder)
    {
        builder.ToTable("invoice_number_sequences");

        builder.HasKey(s => new { s.BusinessId, s.Year }).HasName("pk_invoice_number_sequences");
    }
}
