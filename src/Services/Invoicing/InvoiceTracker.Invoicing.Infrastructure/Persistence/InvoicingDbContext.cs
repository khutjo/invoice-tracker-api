using InvoiceTracker.Invoicing.Domain.Documents;
using InvoiceTracker.Invoicing.Domain.Invoices;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Invoicing.Infrastructure.Persistence;

public sealed class InvoicingDbContext(DbContextOptions<InvoicingDbContext> options) : DbContext(options)
{
    // Aggregate roots only: lines, payments and events are reached through Invoice.
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceDocument> InvoiceDocuments => Set<InvoiceDocument>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(32);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InvoicingDbContext).Assembly);
    }
}
