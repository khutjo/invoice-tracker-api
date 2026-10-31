using InvoiceTracker.Business.Domain.ReferenceData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceTracker.Business.Infrastructure.Persistence.Configurations;

internal sealed class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        builder.ToTable("banks");

        builder.HasKey(b => b.Id).HasName("pk_banks");
        builder.Property(b => b.Id).HasColumnName("bank_id");

        builder.Property(b => b.Name).HasMaxLength(100);
        builder.Property(b => b.UniversalBranchCode).HasMaxLength(6).IsFixedLength().IsUnicode(false);

        builder.HasIndex(b => b.Name).IsUnique().HasDatabaseName("uq_banks_name");
    }
}
