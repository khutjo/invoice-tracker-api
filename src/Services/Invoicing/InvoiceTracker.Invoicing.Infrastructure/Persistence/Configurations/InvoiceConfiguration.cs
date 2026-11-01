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

        // Frozen at send, stored on the invoice row itself (null columns while Draft)
        builder.OwnsOne(i => i.BilledCustomer, customer =>
        {
            customer.Property(c => c.Name).HasColumnName("billed_customer_name").HasMaxLength(200);
            customer.Property(c => c.Email).HasColumnName("billed_customer_email").HasMaxLength(256);
            customer.Property(c => c.VatNumber).HasColumnName("billed_customer_vat_number").HasMaxLength(10);
            customer.Property(c => c.AddressLine1).HasColumnName("billed_customer_address_line1").HasMaxLength(200);
            customer.Property(c => c.AddressLine2).HasColumnName("billed_customer_address_line2").HasMaxLength(200);
            customer.Property(c => c.Suburb).HasColumnName("billed_customer_suburb").HasMaxLength(100);
            customer.Property(c => c.City).HasColumnName("billed_customer_city").HasMaxLength(100);
            customer.Property(c => c.PostalCode).HasColumnName("billed_customer_postal_code").HasMaxLength(4);
        });

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
