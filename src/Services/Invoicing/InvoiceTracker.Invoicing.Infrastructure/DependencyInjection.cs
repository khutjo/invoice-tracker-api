using InvoiceTracker.Invoicing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceTracker.Invoicing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInvoicingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("InvoicingDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'ConnectionStrings:InvoicingDb' is not configured.");
        }

        services.AddDbContext<InvoicingDbContext>(options => options
            .UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure())
            .UseSnakeCaseNamingConvention());

        services.AddHealthChecks()
            .AddDbContextCheck<InvoicingDbContext>(tags: ["ready"]);

        return services;
    }
}
