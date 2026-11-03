using System.Text;
using InvoiceTracker.Invoicing.Api.Interfaces;
using InvoiceTracker.Invoicing.Api.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceTracker.Invoicing.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/webhooks/payments")]
public sealed class PaymentWebhookController(IPaymentWebhookService paymentWebhookService) : ControllerBase
{
    private const string TimestampHeader = "Webhook-Timestamp";
    private const string SignatureHeader = "Webhook-Signature";

    [HttpPost]
    [RequestSizeLimit(64 * 1024)]
    [ProducesResponseType<PaymentWebhookResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReceiveAsync(
        [FromHeader(Name = TimestampHeader)] string? timestamp,
        [FromHeader(Name = SignatureHeader)] string? signature,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync(cancellationToken);

        var response = await paymentWebhookService.HandleAsync(body, timestamp, signature, cancellationToken);
        return Ok(response);
    }
}
