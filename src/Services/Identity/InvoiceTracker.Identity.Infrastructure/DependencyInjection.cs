using InvoiceTracker.BuildingBlocks.Messaging;
using InvoiceTracker.BuildingBlocks.ServiceDefaults.HealthChecks;
using InvoiceTracker.Identity.Infrastructure.Interfaces;
using InvoiceTracker.Identity.Infrastructure.Persistence;
using InvoiceTracker.Identity.Infrastructure.Tokens;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<SigningKeyProvider>();
        services.AddSingleton<IAccessTokenIssuer, AccessTokenIssuer>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 10;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<IdentityServiceDbContext>()
            .AddDefaultTokenProviders();

        // Signs the email verification tokens. Keys live in identity_db, so links survive a container
        // restart and work on every instance; the fixed name keeps all instances on one key ring
        services.AddDataProtection()
            .SetApplicationName("invoice-tracker-identity")
            .PersistKeysToDbContext<IdentityServiceDbContext>();

        services.AddMessaging();
        services.AddOutboxPublisher<IdentityServiceDbContext>();

        services.AddHealthChecks()
            .AddDbContextCheck<IdentityServiceDbContext>(tags: [HealthCheckEndpoints.ReadyTag]);

        return services;
    }
}
