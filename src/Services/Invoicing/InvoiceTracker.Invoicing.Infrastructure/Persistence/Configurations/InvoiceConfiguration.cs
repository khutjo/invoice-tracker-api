using InvoiceTracker.Invoicing.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceTracker.Invoicing.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(i => i.Id).HasName("pk_invoices");
        builder.Property(i => i.Id).HasColumnName("invoice_id");

        builder.Property(i => i.InvoiceNumber).HasMaxLength(32);
        builder.Property(i => i.RowVersion).IsRowVersion();

        builder.HasIndex(i => new { i.BusinessId, i.InvoiceNumber })
            .IsUnique()
            .HasFilter("[invoice_number] IS NOT NULL")
            .HasDatabaseName("uq_invoices_business_id_invoice_number");

        builder.HasIndex(i => new { i.BusinessId, i.Status, i.DueDate })
            .HasDatabaseName("ix_invoices_business_id_status_due_date");

        builder.HasIndex(i => new { i.BusinessId, i.CustomerId })
            .HasDatabaseName("ix_invoices_business_id_customer_id");

        builder.HasMany(i => i.Lines)
            .WithOne()
            .HasForeignKey(l => l.InvoiceId)
            .HasConstraintName("fk_invoice_lines_invoices")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Payments)
            .WithOne()
            .HasForeignKey(p => p.InvoiceId)
            .HasConstraintName("fk_payments_invoices")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Events)
            .WithOne()
            .HasForeignKey(e => e.InvoiceId)
            .HasConstraintName("fk_invoice_events_invoices")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
