using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InvoiceTracker.BuildingBlocks.ServiceDefaults.Database;

public static class MigrationExtensions
{
    public const string MigrateOnStartupKey = "Database:MigrateOnStartup";

    /// <summary>
    /// Applies pending EF Core migrations when Database:MigrateOnStartup is true (set by Docker Compose).
    /// Safe because each service runs as a single instance; with replicas, migrations would move to a one-shot job.
    /// </summary>
    public static async Task MigrateDbContextAsync<TContext>(this WebApplication app)
        where TContext : DbContext
    {
        if (!app.Configuration.GetValue<bool>(MigrateOnStartupKey))
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();

        logger.LogInformation("Applying migrations for {DbContext}", typeof(TContext).Name);
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Migrations applied for {DbContext}", typeof(TContext).Name);
    }
}
