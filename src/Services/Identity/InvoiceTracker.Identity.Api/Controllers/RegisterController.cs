using InvoiceTracker.Identity.Api.Interfaces;
using InvoiceTracker.Identity.Api.Models.Request;
using InvoiceTracker.Identity.Api.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceTracker.Identity.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class RegisterController(IRegisterService registerService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<RegisterUserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var response = await registerService.RegisterUserAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmailAsync([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        await registerService.VerifyEmailAsync(request, cancellationToken);
        return NoContent();
    }
}
