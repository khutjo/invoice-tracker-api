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

        builder.HasIndex(p => p.InvoiceId).HasDatabaseName("ix_payments_invoice_id");
    }
}
