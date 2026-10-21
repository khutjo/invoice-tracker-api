using InvoiceTracker.Invoicing.Domain.Documents;
using InvoiceTracker.Invoicing.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceTracker.Invoicing.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceDocumentConfiguration : IEntityTypeConfiguration<InvoiceDocument>
{
    public void Configure(EntityTypeBuilder<InvoiceDocument> builder)
    {
        builder.ToTable("invoice_documents");

        builder.HasKey(d => d.Id).HasName("pk_invoice_documents");
        builder.Property(d => d.Id).HasColumnName("invoice_document_id");

        builder.Property(d => d.BlobPath).HasMaxLength(400);
        builder.Property(d => d.ContentHash).HasMaxLength(64).IsFixedLength();

        // One stored PDF per sent invoice
        builder.HasIndex(d => d.InvoiceId)
            .IsUnique()
            .HasDatabaseName("uq_invoice_documents_invoice_id");

        builder.HasOne<Invoice>()
            .WithMany()
            .HasForeignKey(d => d.InvoiceId)
            .HasConstraintName("fk_invoice_documents_invoices")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
