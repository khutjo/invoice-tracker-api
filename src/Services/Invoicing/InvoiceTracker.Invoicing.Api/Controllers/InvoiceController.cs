using InvoiceTracker.BuildingBlocks.ServiceDefaults.Authentication;
using InvoiceTracker.Invoicing.Api.Interfaces;
using InvoiceTracker.Invoicing.Api.Models.Request;
using InvoiceTracker.Invoicing.Api.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceTracker.Invoicing.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/invoices")]
public sealed class InvoiceController(IInvoiceService invoiceService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<InvoiceSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInvoicesListAsync([FromQuery] InvoiceSearchQuery query, CancellationToken cancellationToken)
    {
        var response = await invoiceService.GetInvoicesListAsync(query, User.GetBusinessId(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{invoiceId:guid}")]
    [ProducesResponseType<InvoiceDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        var response = await invoiceService.GetInvoiceAsync(invoiceId, User.GetBusinessId(), cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType<InvoiceDetailsResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInvoiceAsync([FromBody] InvoiceDetailsRequest request, CancellationToken cancellationToken)
    {
        var response = await invoiceService.CreateInvoiceAsync(request, User.GetBusinessId(), User.GetUserId(), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("{invoiceId:guid}")]
    [ProducesResponseType<InvoiceDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateInvoiceAsync(Guid invoiceId, [FromBody] UpdateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var response = await invoiceService.UpdateInvoiceAsync(invoiceId, request, User.GetBusinessId(), User.GetUserId(), cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{invoiceId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        await invoiceService.DeleteInvoiceAsync(invoiceId, User.GetBusinessId(), cancellationToken);
        return NoContent();
    }

    [HttpPost("{invoiceId:guid}/send")]
    [ProducesResponseType<InvoiceDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SendInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        var response = await invoiceService.SendInvoiceAsync(invoiceId, User.GetBusinessId(), User.GetUserId(), cancellationToken);
        return Ok(response);
    }
}
