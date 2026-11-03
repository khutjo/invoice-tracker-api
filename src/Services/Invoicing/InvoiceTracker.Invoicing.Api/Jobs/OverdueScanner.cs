using InvoiceTracker.Invoicing.Api.Interfaces;
using Microsoft.Extensions.Options;

namespace InvoiceTracker.Invoicing.Api.Jobs;

public sealed class OverdueScanner(
    IServiceScopeFactory scopeFactory,
    IOptions<OverdueScanOptions> options,
    TimeProvider timeProvider,
    ILogger<OverdueScanner> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.Interval, timeProvider);
        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var scan = scope.ServiceProvider.GetRequiredService<IOverdueScanService>();
                await scan.RunAsync(businessId: null, asOf: null, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Overdue scan failed; retrying on the next run");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
