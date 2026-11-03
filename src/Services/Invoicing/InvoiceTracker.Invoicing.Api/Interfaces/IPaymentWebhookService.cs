using InvoiceTracker.Invoicing.Api.Models.Response;

namespace InvoiceTracker.Invoicing.Api.Interfaces;

public interface IPaymentWebhookService
{
    Task<PaymentWebhookResponse> HandleAsync(string body, string? timestamp, string? signature, CancellationToken cancellationToken);
}
