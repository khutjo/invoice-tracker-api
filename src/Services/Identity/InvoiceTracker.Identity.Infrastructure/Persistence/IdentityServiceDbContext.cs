using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Identity.Infrastructure.Persistence;

/// <summary>
/// IdentityUserContext rather than IdentityDbContext: roles are a column on the user,
/// so the Identity role tables are not needed.
/// </summary>
public sealed class IdentityServiceDbContext(DbContextOptions<IdentityServiceDbContext> options)
    : IdentityUserContext<ApplicationUser, Guid>(options)
{
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

            // Identity names these EmailIndex and UserNameIndex; rename to match the rest of the schema
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
    }
}
