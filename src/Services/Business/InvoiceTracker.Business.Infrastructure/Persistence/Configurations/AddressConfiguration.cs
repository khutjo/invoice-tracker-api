using InvoiceTracker.Business.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceTracker.Business.Infrastructure.Persistence.Configurations;

internal sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("addresses");

        builder.HasKey(a => a.Id).HasName("pk_addresses");
        builder.Property(a => a.Id).HasColumnName("address_id");

        builder.Property(a => a.Line1).HasMaxLength(200);
        builder.Property(a => a.Line2).HasMaxLength(200);
        builder.Property(a => a.Suburb).HasMaxLength(100);
        builder.Property(a => a.City).HasMaxLength(100);
        builder.Property(a => a.PostalCode).HasMaxLength(10);
        builder.Property(a => a.CountryCode).HasMaxLength(3).IsFixedLength();
        builder.Property(a => a.RowVersion).IsRowVersion();
    }
}
