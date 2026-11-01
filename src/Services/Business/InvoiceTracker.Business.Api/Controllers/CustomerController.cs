using InvoiceTracker.BuildingBlocks.ServiceDefaults.Authentication;
using InvoiceTracker.Business.Api.Interfaces;
using InvoiceTracker.Business.Api.Models.Request;
using InvoiceTracker.Business.Api.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceTracker.Business.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/customers")]
public sealed class CustomerController(ICustomerService customerService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<CustomerSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchCustomersAsync([FromQuery] CustomerSearchQuery query, CancellationToken cancellationToken)
    {
        var response = await customerService.SearchCustomersAsync(query, User.GetBusinessId(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{customerId:guid}")]
    [ProducesResponseType<CustomerDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var response = await customerService.GetCustomerAsync(customerId, User.GetBusinessId(), cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType<CustomerDetailsResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomerAsync([FromBody] CustomerDetailsRequest request, CancellationToken cancellationToken)
    {
        var response = await customerService.CreateCustomerAsync(request, User.GetBusinessId(), User.GetUserId(), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("{customerId:guid}")]
    [ProducesResponseType<CustomerDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCustomerAsync(Guid customerId, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var response = await customerService.UpdateCustomerAsync(customerId, request, User.GetBusinessId(), User.GetUserId(), cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{customerId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveCustomerAsync(Guid customerId, CancellationToken cancellationToken)
    {
        await customerService.ArchiveCustomerAsync(customerId, User.GetBusinessId(), User.GetUserId(), cancellationToken);
        return NoContent();
    }
}
