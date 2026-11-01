using InvoiceTracker.Invoicing.Application.Customers;
using InvoiceTracker.Invoicing.Domain.Invoices;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceTracker.Invoicing.Api.Exceptions;

public sealed class InvoiceExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails? problem = exception switch
        {
            InvoiceException { Errors: not null } ex => new ValidationProblemDetails(ex.Errors)
            {
                Title = ex.Message,
                Status = ex.StatusCode
            },
            InvoiceException ex => new ProblemDetails { Title = ex.Message, Status = ex.StatusCode },

            // The domain refused a transition, for example editing a sent invoice
            InvoiceRuleException ex => new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status409Conflict },

            // Customers live in the Business service; without it invoices can't be created or sent
            CustomerDirectoryUnavailableException => new ProblemDetails
            {
                Title = "Customer details are unavailable right now. Try again shortly.",
                Status = StatusCodes.Status503ServiceUnavailable
            },
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
