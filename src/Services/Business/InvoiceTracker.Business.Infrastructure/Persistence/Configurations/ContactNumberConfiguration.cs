using InvoiceTracker.Business.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceTracker.Business.Infrastructure.Persistence.Configurations;

internal sealed class ContactNumberConfiguration : IEntityTypeConfiguration<ContactNumber>
{
    public void Configure(EntityTypeBuilder<ContactNumber> builder)
    {
        builder.ToTable("contact_numbers");

        builder.HasKey(c => c.Id).HasName("pk_contact_numbers");
        builder.Property(c => c.Id).HasColumnName("contact_number_id");

        builder.Property(c => c.Number).HasMaxLength(20);
        builder.Property(c => c.RowVersion).IsRowVersion();
    }
}
