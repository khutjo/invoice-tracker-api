using InvoiceTracker.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceTracker.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IdentityDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'ConnectionStrings:IdentityDb' is not configured.");
        }

        services.AddDbContext<IdentityServiceDbContext>(options => options
            .UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure())
            .UseSnakeCaseNamingConvention());

        services.AddIdentityCore<ApplicationUser>()
            .AddEntityFrameworkStores<IdentityServiceDbContext>();

        services.AddHealthChecks()
            .AddDbContextCheck<IdentityServiceDbContext>(tags: ["ready"]);

        return services;
    }
}
