using InvoiceTracker.Business.Application.Logos;
using InvoiceTracker.Business.Infrastructure.Logos;
using InvoiceTracker.Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InvoiceTracker.Business.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BusinessDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'ConnectionStrings:BusinessDb' is not configured.");
        }

        services.AddDbContext<BusinessDbContext>(options => options
            .UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure())
            .UseSnakeCaseNamingConvention());

        services.AddHealthChecks()
            .AddDbContextCheck<BusinessDbContext>(tags: ["ready"]);

        services.AddOptions<LogoStorageOptions>()
            .BindConfiguration(LogoStorageOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<ILogoStore, FileSystemLogoStore>();
        services.TryAddSingleton(TimeProvider.System);

        return services;
    }
}
