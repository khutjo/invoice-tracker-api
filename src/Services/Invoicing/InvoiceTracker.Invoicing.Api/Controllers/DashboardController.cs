using InvoiceTracker.BuildingBlocks.ServiceDefaults.Authentication;
using InvoiceTracker.Invoicing.Api.Interfaces;
using InvoiceTracker.Invoicing.Api.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceTracker.Invoicing.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/dashboard")]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<DashboardDetailsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardDetailsAsync(CancellationToken cancellationToken)
    {
        var response = await dashboardService.GetDashboardDetailsAsync(User.GetBusinessId(), cancellationToken);
        return Ok(response);
    }
}
