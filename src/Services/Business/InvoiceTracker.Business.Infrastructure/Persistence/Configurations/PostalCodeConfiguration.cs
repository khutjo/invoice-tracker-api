using InvoiceTracker.Business.Domain.ReferenceData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceTracker.Business.Infrastructure.Persistence.Configurations;

internal sealed class PostalCodeConfiguration : IEntityTypeConfiguration<PostalCode>
{
    public void Configure(EntityTypeBuilder<PostalCode> builder)
    {
        builder.ToTable("postal_codes");

        builder.HasKey(p => p.Id).HasName("pk_postal_codes");
        builder.Property(p => p.Id).HasColumnName("postal_code_id");

        builder.Property(p => p.Code).HasMaxLength(4).IsFixedLength().IsUnicode(false);
        builder.Property(p => p.Suburb).HasMaxLength(64);
        builder.Property(p => p.Town).HasMaxLength(64);

        builder.HasIndex(p => p.Code).HasDatabaseName("ix_postal_codes_code");
    }
}
