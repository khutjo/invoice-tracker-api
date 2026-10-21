using InvoiceTracker.Business.Domain.Businesses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceTracker.Business.Infrastructure.Persistence.Configurations;

internal sealed class BankDetailsConfiguration : IEntityTypeConfiguration<BankDetails>
{
    public void Configure(EntityTypeBuilder<BankDetails> builder)
    {
        builder.ToTable("bank_details");

        builder.HasKey(b => b.Id).HasName("pk_bank_details");
        builder.Property(b => b.Id).HasColumnName("bank_details_id");

        builder.Property(b => b.BankName).HasMaxLength(100);
        builder.Property(b => b.AccountHolder).HasMaxLength(200);
        builder.Property(b => b.AccountNumber).HasMaxLength(20);
        builder.Property(b => b.BranchCode).HasMaxLength(10);
        builder.Property(b => b.BranchName).HasMaxLength(100);
        builder.Property(b => b.RowVersion).IsRowVersion();
    }
}
