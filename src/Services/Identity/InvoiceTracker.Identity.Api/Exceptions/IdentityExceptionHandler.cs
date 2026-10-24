using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceTracker.Identity.Api.Exceptions;

/// <summary>
/// Turns the service's expected failures into ProblemDetails, so controllers only describe the happy path.
/// Anything else falls through to the default handler and becomes a 500.
/// </summary>
public sealed class IdentityExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, System.Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails? problem = exception switch
        {
            RegisterException { Errors: not null } ex => new ValidationProblemDetails(ex.Errors)
            {
                Title = ex.Message,
                Status = ex.StatusCode
            },
            RegisterException ex => new ProblemDetails { Title = ex.Message, Status = ex.StatusCode },
            LoginException ex => new ProblemDetails { Title = ex.Message, Status = ex.StatusCode },
            _ => null
        };

        if (problem is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status400BadRequest;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}
