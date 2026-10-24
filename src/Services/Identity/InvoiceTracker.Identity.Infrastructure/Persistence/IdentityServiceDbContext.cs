using InvoiceTracker.BuildingBlocks.Messaging.Outbox;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Identity.Infrastructure.Persistence;

public sealed class IdentityServiceDbContext(DbContextOptions<IdentityServiceDbContext> options)
    : IdentityUserContext<ApplicationUser, Guid>(options), IDataProtectionKeyContext
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>Keys that sign email verification tokens, kept here so links survive restarts.</summary>
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(32);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(user =>
        {
            user.ToTable("users");
            user.HasKey(u => u.Id).HasName("pk_users");
            user.Property(u => u.Id).HasColumnName("user_id");
            user.Property(u => u.FirstName).HasMaxLength(100);
            user.Property(u => u.LastName).HasMaxLength(100);
            user.HasIndex(u => u.BusinessId).HasDatabaseName("ix_users_business_id");
            user.HasIndex(u => u.NormalizedEmail).HasDatabaseName("ix_users_normalized_email");
            user.HasIndex(u => u.NormalizedUserName).HasDatabaseName("uq_users_normalized_user_name");
        });

        builder.Entity<IdentityUserClaim<Guid>>(claim =>
        {
            claim.ToTable("user_claims");
            claim.Property(c => c.Id).HasColumnName("user_claim_id");
        });
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");

        builder.Entity<DataProtectionKey>(key =>
        {
            key.ToTable("data_protection_keys");
            key.HasKey(k => k.Id).HasName("pk_data_protection_keys");
            key.Property(k => k.Id).HasColumnName("data_protection_key_id");
        });

        builder.ApplyConfigurationsFromAssembly(typeof(IdentityServiceDbContext).Assembly);
        builder.AddOutbox();
    }
}
