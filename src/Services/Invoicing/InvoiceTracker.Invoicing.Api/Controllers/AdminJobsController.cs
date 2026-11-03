using InvoiceTracker.BuildingBlocks.ServiceDefaults.Authentication;
using InvoiceTracker.Invoicing.Api.Exceptions;
using InvoiceTracker.Invoicing.Api.Interfaces;
using InvoiceTracker.Invoicing.Api.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace InvoiceTracker.Invoicing.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.OwnerOnly)]
[Route("api/v1/admin/jobs")]
public sealed class AdminJobsController(IOverdueScanService overdueScanService, IOptions<OverdueScanOptions> options) : ControllerBase
{

    [HttpPost("overdue-scan")]
    [ProducesResponseType<OverdueScanResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RunOverdueScanAsync([FromQuery] DateOnly? asOf, CancellationToken cancellationToken)
    {
        if (asOf is not null && !options.Value.AllowAsOfOverride)
        {
            throw InvoiceException.Invalid("asOf", "Scanning as of another date is turned off on this server.");
        }

        var result = await overdueScanService.RunAsync(User.GetBusinessId(), asOf, cancellationToken);
        return Ok(result);
    }
}
