using System.Security.Cryptography;
using InvoiceTracker.BuildingBlocks.Messaging.Outbox;
using InvoiceTracker.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.MsSql;

namespace InvoiceTracker.Identity.IntegrationTests;

public sealed class IdentityApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlServer =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        var connectionString = new SqlConnectionStringBuilder(_sqlServer.GetConnectionString())
        {
            InitialCatalog = "identity_db_tests"
        }.ConnectionString;

        using var rsa = RSA.Create(2048);

        builder.UseSetting("ConnectionStrings:IdentityDb", connectionString);
        builder.UseSetting("Jwt:SigningKey", Convert.ToBase64String(rsa.ExportPkcs8PrivateKey()));
        builder.UseSetting("RabbitMq:Password", "not-used-in-tests");
        builder.UseSetting("Portal:BaseUrl", "http://portal.test");

        builder.ConfigureServices(services =>
        {
            var outboxPublisher = services.Single(s =>
                s.ServiceType == typeof(IHostedService) &&
                s.ImplementationType == typeof(OutboxPublisherService<IdentityServiceDbContext>));
            services.Remove(outboxPublisher);
        });
    }

    public async Task InitializeAsync()
    {
        await _sqlServer.StartAsync();

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityServiceDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _sqlServer.DisposeAsync();
    }
}
