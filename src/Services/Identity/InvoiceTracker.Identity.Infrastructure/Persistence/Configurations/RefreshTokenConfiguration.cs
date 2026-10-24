using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceTracker.Identity.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(t => t.Id).HasName("pk_refresh_tokens");
        builder.Property(t => t.Id).HasColumnName("refresh_token_id");

        // Hex SHA-256
        builder.Property(t => t.TokenHash).HasMaxLength(64).IsFixedLength();

        builder.HasIndex(t => t.TokenHash).IsUnique().HasDatabaseName("uq_refresh_tokens_token_hash");
        builder.HasIndex(t => t.FamilyId).HasDatabaseName("ix_refresh_tokens_family_id");
        builder.HasIndex(t => t.UserId).HasDatabaseName("ix_refresh_tokens_user_id");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .HasConstraintName("fk_refresh_tokens_users")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
