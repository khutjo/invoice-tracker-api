using InvoiceTracker.Business.Domain.Businesses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceTracker.Business.Infrastructure.Persistence.Configurations;

internal sealed class BusinessProfileConfiguration : IEntityTypeConfiguration<BusinessProfile>
{
    public void Configure(EntityTypeBuilder<BusinessProfile> builder)
    {
        builder.ToTable("businesses");

        builder.HasKey(b => b.Id).HasName("pk_businesses");
        builder.Property(b => b.Id).HasColumnName("business_id");

        builder.Property(b => b.Name).HasMaxLength(200);
        builder.Property(b => b.RegistrationNumber).HasMaxLength(20);
        builder.Property(b => b.VatNumber).HasMaxLength(10);
        builder.Property(b => b.TimeZone).HasMaxLength(64);
        builder.Property(b => b.LogoBlobPath).HasMaxLength(400);
        builder.Property(b => b.RowVersion).IsRowVersion();

        // Each detail row belongs to one business; Restrict so removing a detail never removes the business
        builder.HasOne(b => b.SupportEmail)
            .WithOne()
            .HasForeignKey<BusinessProfile>(b => b.SupportEmailId)
            .HasConstraintName("fk_businesses_email_addresses")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.ContactNumber)
            .WithOne()
            .HasForeignKey<BusinessProfile>(b => b.ContactNumberId)
            .HasConstraintName("fk_businesses_contact_numbers")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Address)
            .WithOne()
            .HasForeignKey<BusinessProfile>(b => b.AddressId)
            .HasConstraintName("fk_businesses_addresses")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.BankDetails)
            .WithOne()
            .HasForeignKey<BusinessProfile>(b => b.BankDetailsId)
            .HasConstraintName("fk_businesses_bank_details")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
