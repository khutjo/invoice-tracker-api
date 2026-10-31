using System.ComponentModel.DataAnnotations;
using InvoiceTracker.Business.Api.Interfaces;
using InvoiceTracker.Business.Api.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceTracker.Business.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/business/lookups")]
public sealed class LookupsController(ILookupService lookupService) : ControllerBase
{
    [HttpGet]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType<LookupsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLookupsAsync(CancellationToken cancellationToken)
    {
        var response = await lookupService.GetLookupsAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("postal-codes/{code}")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Client)]
    [ProducesResponseType<IReadOnlyList<PostalCodeOption>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FindPostalCodeAsync(
        [FromRoute, RegularExpression(@"^\d{4}$", ErrorMessage = "Postal code must be 4 digits.")] string code,
        CancellationToken cancellationToken)
    {
        var response = await lookupService.FindPostalCodeAsync(code, cancellationToken);
        return Ok(response);
    }
}
