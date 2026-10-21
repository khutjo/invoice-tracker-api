using InvoiceTracker.Business.Domain.Businesses;
using InvoiceTracker.Business.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceTracker.Business.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id).HasName("pk_customers");
        builder.Property(c => c.Id).HasColumnName("customer_id");

        builder.Property(c => c.Name).HasMaxLength(200);
        builder.Property(c => c.CustomerReference).HasMaxLength(50);
        builder.Property(c => c.VatNumber).HasMaxLength(10);
        builder.Property(c => c.RowVersion).IsRowVersion();

        builder.HasIndex(c => new { c.BusinessId, c.Name })
            .HasDatabaseName("ix_customers_business_id_name");

        builder.HasOne<BusinessProfile>()
            .WithMany()
            .HasForeignKey(c => c.BusinessId)
            .HasConstraintName("fk_customers_businesses")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.EmailAddress)
            .WithOne()
            .HasForeignKey<Customer>(c => c.EmailAddressId)
            .HasConstraintName("fk_customers_email_addresses")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ContactNumber)
            .WithOne()
            .HasForeignKey<Customer>(c => c.ContactNumberId)
            .HasConstraintName("fk_customers_contact_numbers")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.BillingAddress)
            .WithOne()
            .HasForeignKey<Customer>(c => c.BillingAddressId)
            .HasConstraintName("fk_customers_addresses")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
