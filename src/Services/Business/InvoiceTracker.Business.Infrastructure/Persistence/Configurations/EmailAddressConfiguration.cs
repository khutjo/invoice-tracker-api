using InvoiceTracker.Business.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceTracker.Business.Infrastructure.Persistence.Configurations;

internal sealed class EmailAddressConfiguration : IEntityTypeConfiguration<EmailAddress>
{
    public void Configure(EntityTypeBuilder<EmailAddress> builder)
    {
        builder.ToTable("email_addresses");

        builder.HasKey(e => e.Id).HasName("pk_email_addresses");
        builder.Property(e => e.Id).HasColumnName("email_address_id");

        builder.Property(e => e.Email).HasMaxLength(256);
        builder.Property(e => e.RowVersion).IsRowVersion();
    }
}
