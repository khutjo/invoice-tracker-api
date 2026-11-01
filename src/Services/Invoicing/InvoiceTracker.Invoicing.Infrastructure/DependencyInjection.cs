using InvoiceTracker.Invoicing.Application.Customers;
using InvoiceTracker.Invoicing.Application.Invoices;
using InvoiceTracker.Invoicing.Application.Time;
using InvoiceTracker.Invoicing.Infrastructure.Customers;
using InvoiceTracker.Invoicing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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

        services.AddScoped<IInvoiceNumberAllocator, InvoiceNumberAllocator>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<BusinessCalendar>();

        AddCustomerDirectory(services);

        return services;
    }

    private static void AddCustomerDirectory(IServiceCollection services)
    {
        services.AddOptions<BusinessApiOptions>()
            .BindConfiguration(BusinessApiOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpContextAccessor();
        services.AddTransient<ForwardAuthorizationHandler>();

        // Standard resilience: timeouts, retries with backoff on 5xx and network errors, circuit breaker.
        // Only GETs go through this client, so retrying is safe.
        services.AddHttpClient<ICustomerDirectory, BusinessApiCustomerDirectory>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<BusinessApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
            })
            .AddHttpMessageHandler<ForwardAuthorizationHandler>()
            .AddStandardResilienceHandler();
    }
}
