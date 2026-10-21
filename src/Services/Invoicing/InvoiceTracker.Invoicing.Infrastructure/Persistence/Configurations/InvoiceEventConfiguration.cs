using InvoiceTracker.Invoicing.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceTracker.Invoicing.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceEventConfiguration : IEntityTypeConfiguration<InvoiceEvent>
{
    public void Configure(EntityTypeBuilder<InvoiceEvent> builder)
    {
        builder.ToTable("invoice_events");

        builder.HasKey(e => e.Id).HasName("pk_invoice_events");
        builder.Property(e => e.Id).HasColumnName("invoice_event_id");

        builder.Property(e => e.Detail).HasMaxLength(1000);

        builder.HasIndex(e => new { e.InvoiceId, e.OccurredAt })
            .HasDatabaseName("ix_invoice_events_invoice_id_occurred_at");
    }
}
