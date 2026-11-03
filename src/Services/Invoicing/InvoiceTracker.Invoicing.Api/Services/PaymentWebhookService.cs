using System.Text.Json;
using InvoiceTracker.BuildingBlocks.Contracts.Events.InvoicingModels;
using InvoiceTracker.BuildingBlocks.Messaging.Outbox;
using InvoiceTracker.Invoicing.Api.Exceptions;
using InvoiceTracker.Invoicing.Api.Interfaces;
using InvoiceTracker.Invoicing.Api.Models.Request;
using InvoiceTracker.Invoicing.Api.Models.Response;
using InvoiceTracker.Invoicing.Api.Webhooks;
using InvoiceTracker.Invoicing.Application.Payments;
using InvoiceTracker.Invoicing.Application.Time;
using InvoiceTracker.Invoicing.Domain.Invoices;
using InvoiceTracker.Invoicing.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InvoiceTracker.Invoicing.Api.Services;

public sealed class PaymentWebhookService(
    InvoicingDbContext dbContext,
    TimeProvider timeProvider,
    IOptions<PaymentWebhookOptions> options,
    ILogger<PaymentWebhookService> logger) : IPaymentWebhookService
{
    private const string PaymentSucceeded = "payment.succeeded";

    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public async Task<PaymentWebhookResponse> HandleAsync(string body, string? timestamp, string? signature, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (!PaymentWebhookSignature.IsValid(settings.Secret, timestamp, signature, body, timeProvider.GetUtcNow(), settings.Tolerance))
        {
            logger.LogWarning("Rejected a payment webhook with a missing, invalid or expired signature");
            throw InvoiceException.InvalidWebhookSignature();
        }

        var request = Parse(body);

        if (!string.Equals(request.EventType, PaymentSucceeded, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("Ignored payment webhook {EventId} of type {EventType}", request.EventId, request.EventType);
            return new PaymentWebhookResponse { Result = PaymentWebhookResponse.Ignored };
        }

        var eventId = request.EventId!.Trim();
        var invoiceId = request.InvoiceId!.Value;

        var existing = await dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.Id == invoiceId)
            .SelectMany(i => i.Payments)
            .Where(p => p.ExternalId == eventId)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is { } existingPaymentId)
        {
            return await DuplicateAsync(eventId, invoiceId, existingPaymentId, cancellationToken);
        }

        var invoice = await dbContext.Invoices
            .SingleOrDefaultAsync(i => i.Id == invoiceId, cancellationToken)
            ?? throw InvoiceException.NotFound();

        var payment = invoice.RecordPayment(
            request.Amount!.Value,
            BusinessCalendar.DateOf(request.PaidAt ?? timeProvider.GetUtcNow()),
            ToPaymentMethod(request.Method),
            request.Reference ?? invoice.InvoiceNumber,
            eventId,
            recordedBy: null,
            timeProvider.GetUtcNow().UtcDateTime);

        dbContext.ChangeTracker.DetectChanges();
        dbContext.AddOutboxMessage(ToPaymentRecorded(invoice, payment));

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            dbContext.ChangeTracker.Clear();
            return await DuplicateAsync(eventId, invoiceId, null, cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw InvoiceException.StaleVersion();
        }

        logger.LogInformation("Recorded payment {PaymentId} of {Amount} on invoice {InvoiceNumber} from webhook {EventId}; balance {Balance}",
            payment.Id, payment.Amount, invoice.InvoiceNumber, eventId, invoice.Balance);

        return new PaymentWebhookResponse
        {
            Result = PaymentWebhookResponse.Processed,
            PaymentId = payment.Id,
            InvoiceStatus = invoice.Status,
            Balance = invoice.Balance
        };
    }

    private static PaymentWebhookRequest Parse(string body)
    {
        PaymentWebhookRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<PaymentWebhookRequest>(body, _json);
        }
        catch (JsonException)
        {
            throw InvoiceException.Invalid("body", "The webhook body is not valid JSON.");
        }

        if (request is null)
        {
            throw InvoiceException.Invalid("body", "The webhook body is empty.");
        }

        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.EventId) || request.EventId.Length > 100)
        {
            errors["eventId"] = ["eventId is required, at most 100 characters."];
        }

        if (string.IsNullOrWhiteSpace(request.EventType))
        {
            errors["eventType"] = ["eventType is required."];
        }

        if (string.Equals(request.EventType, PaymentSucceeded, StringComparison.OrdinalIgnoreCase))
        {
            if (request.InvoiceId is null || request.InvoiceId == Guid.Empty)
            {
                errors["invoiceId"] = ["invoiceId is required."];
            }

            if (request.Amount is null)
            {
                errors["amount"] = ["amount is required."];
            }

            if (!string.Equals(request.Currency, "ZAR", StringComparison.OrdinalIgnoreCase))
            {
                errors["currency"] = ["Only ZAR payments are accepted."];
            }

            if (request.Reference is { Length: > 100 })
            {
                errors["reference"] = ["reference is at most 100 characters."];
            }
        }

        if (errors.Count > 0)
        {
            throw InvoiceException.Invalid(errors);
        }

        return request;
    }

    private async Task<PaymentWebhookResponse> DuplicateAsync(string eventId, Guid invoiceId, Guid? paymentId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Payment webhook {EventId} was already recorded; ignoring the redelivery", eventId);

        var invoice = await dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.Id == invoiceId)
            .Select(i => new { i.Status, i.Balance })
            .SingleOrDefaultAsync(cancellationToken);

        return new PaymentWebhookResponse
        {
            Result = PaymentWebhookResponse.Duplicate,
            PaymentId = paymentId,
            InvoiceStatus = invoice?.Status,
            Balance = invoice?.Balance
        };
    }

    private static PaymentMethod ToPaymentMethod(string? method)
    {
        if (string.IsNullOrWhiteSpace(method))
        {
            return PaymentMethod.Card;
        }

        return Enum.TryParse<PaymentMethod>(method.Trim(), ignoreCase: true, out var parsed) && Enum.IsDefined(parsed)
            ? parsed
            : PaymentMethod.Other;
    }

    private PaymentRecorded ToPaymentRecorded(Invoice invoice, Payment payment)
    {
        var billedCustomer = invoice.BilledCustomer
            ?? throw new InvalidOperationException($"Invoice {invoice.Id} has a payment but no billed customer.");

        return new PaymentRecorded
        {
            OccurredAt = timeProvider.GetUtcNow(),
            BusinessId = invoice.BusinessId,
            InvoiceId = invoice.Id,
            PaymentId = payment.Id,
            InvoiceNumber = invoice.InvoiceNumber
                ?? throw new InvalidOperationException($"Invoice {invoice.Id} has a payment but no number."),
            CustomerName = billedCustomer.Name,
            CustomerEmail = billedCustomer.Email,
            Amount = payment.Amount,
            PaidOn = payment.PaidOn,
            Balance = invoice.Balance
        };
    }
}
