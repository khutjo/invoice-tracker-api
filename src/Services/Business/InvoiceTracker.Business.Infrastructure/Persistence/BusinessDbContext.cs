using InvoiceTracker.Business.Domain.Businesses;
using InvoiceTracker.Business.Domain.Customers;
using InvoiceTracker.Business.Domain.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Business.Infrastructure.Persistence;

public sealed class BusinessDbContext(DbContextOptions<BusinessDbContext> options) : DbContext(options)
{
    // Addresses, contact numbers, emails and bank details have their own tables but no DbSet:
    // they carry no business_id, so they are only reached through a business or customer,
    // which keeps them inside the tenant filter.
    public DbSet<BusinessProfile> Businesses => Set<BusinessProfile>();
    public DbSet<Customer> Customers => Set<Customer>();

    // Reference data shared by every business (no business_id), seeded by the AddReferenceData migration
    public DbSet<PostalCode> PostalCodes => Set<PostalCode>();
    public DbSet<Bank> Banks => Set<Bank>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(32);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BusinessDbContext).Assembly);
    }
}
