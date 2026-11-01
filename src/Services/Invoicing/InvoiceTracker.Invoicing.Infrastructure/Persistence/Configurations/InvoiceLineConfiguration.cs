using InvoiceTracker.Invoicing.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceTracker.Invoicing.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("invoice_lines");

        builder.HasKey(l => l.Id).HasName("pk_invoice_lines");
        builder.Property(l => l.Id).HasColumnName("invoice_line_id");

        builder.Property(l => l.Description).HasMaxLength(500);
        builder.Property(l => l.Quantity).HasPrecision(18, 3);
        builder.Property(l => l.VatRate).HasPrecision(5, 4);

        // Worked out from LineTotal and VatRate, not stored
        builder.Ignore(l => l.VatAmount);

        builder.HasIndex(l => l.InvoiceId).HasDatabaseName("ix_invoice_lines_invoice_id");
    }
}
