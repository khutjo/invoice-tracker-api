using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;

namespace InvoiceTracker.BuildingBlocks.ServiceDefaults.HealthChecks;

public static class HealthCheckEndpoints
{
    public const string ReadyTag = "ready";

    /// <summary>
    /// /health/live answers while the process runs and checks nothing, so a slow database never restarts the container.
    /// /health/ready runs the checks tagged "ready" (the database), so traffic waits until dependencies answer.
    /// </summary>
    public static IEndpointRouteBuilder MapDefaultHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(ReadyTag)
        });

        return endpoints;
    }
}
