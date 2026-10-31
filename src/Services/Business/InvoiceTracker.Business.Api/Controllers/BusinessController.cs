using InvoiceTracker.BuildingBlocks.ServiceDefaults.Authentication;
using InvoiceTracker.Business.Api.Interfaces;
using InvoiceTracker.Business.Api.Models.Request;
using InvoiceTracker.Business.Api.Models.Response;
using InvoiceTracker.Business.Application.Logos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceTracker.Business.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/business")]
public sealed class BusinessController(IBusinessService businessService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<BusinessDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBusinessDetailsAsync(CancellationToken cancellationToken)
    {
        var response = await businessService.GetBusinessDetailsAsync(User.GetBusinessId(), cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.OwnerOnly)]
    [ProducesResponseType<BusinessDetailsResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateBusinessAsync([FromBody] CreateBusinessRequest request, CancellationToken cancellationToken)
    {
        var response = await businessService.CreateBusinessAsync(request, User.GetBusinessId(), User.GetUserId(), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut]
    [Authorize(Policy = AuthorizationPolicies.OwnerOnly)]
    [ProducesResponseType<BusinessDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateBusinessDetailsAsync([FromBody] UpdateBusinessDetailsRequest request, CancellationToken cancellationToken)
    {
        var response = await businessService.UpdateBusinessDetailsAsync(request, User.GetBusinessId(), User.GetUserId(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("settings")]
    [ProducesResponseType<BusinessSettingsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBusinessSettingsAsync(CancellationToken cancellationToken)
    {
        var response = await businessService.GetBusinessSettingsAsync(User.GetBusinessId(), cancellationToken);
        return Ok(response);
    }

    [HttpPut("settings")]
    [Authorize(Policy = AuthorizationPolicies.OwnerOnly)]
    [ProducesResponseType<BusinessSettingsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SaveBusinessSettingAsync([FromBody] SaveBusinessSettingsRequest request, CancellationToken cancellationToken)
    {
        var response = await businessService.SaveBusinessSettingAsync(request, User.GetBusinessId(), User.GetUserId(), cancellationToken);
        return Ok(response);
    }

    [HttpPut("logo")]
    [Authorize(Policy = AuthorizationPolicies.OwnerOnly)]
    [RequestSizeLimit(LogoImage.MaxSizeBytes + 64 * 1024)]
    [ProducesResponseType<SaveLogoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveLogoAsync([FromForm] SaveLogoRequest request, CancellationToken cancellationToken)
    {
        var response = await businessService.SaveLogoAsync(request, User.GetBusinessId(), User.GetUserId(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("logo")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK, "image/png", "image/jpeg")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLogoAsync(CancellationToken cancellationToken)
    {
        var logo = await businessService.GetLogoAsync(User.GetBusinessId(), cancellationToken);
        return File(logo.Content, logo.ContentType, logo.FileName);
    }
}
