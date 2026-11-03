using InvoiceTracker.Invoicing.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceTracker.Invoicing.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id).HasName("pk_payments");
        builder.Property(p => p.Id).HasColumnName("payment_id");

        builder.Property(p => p.Reference).HasMaxLength(100);
        builder.Property(p => p.ExternalId).HasMaxLength(100);

        builder.HasIndex(p => p.InvoiceId).HasDatabaseName("ix_payments_invoice_id");

        // One payment per processor event: a redelivered webhook hits this index instead of paying twice
        builder.HasIndex(p => p.ExternalId)
            .IsUnique()
            .HasFilter("[external_id] IS NOT NULL")
            .HasDatabaseName("uq_payments_external_id");
    }
}
