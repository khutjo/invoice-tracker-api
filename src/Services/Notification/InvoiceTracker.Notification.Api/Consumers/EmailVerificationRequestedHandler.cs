using InvoiceTracker.BuildingBlocks.Contracts.Events.IdentityModels;
using InvoiceTracker.BuildingBlocks.Messaging.Abstractions;
using InvoiceTracker.Notification.Api.Options;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace InvoiceTracker.Notification.Api.Consumers;

public sealed class EmailVerificationRequestedHandler(
    IOptions<PortalOptions> portalOptions,
    ILogger<EmailVerificationRequestedHandler> logger) : IIntegrationEventHandler<EmailVerificationRequested>
{
    public Task HandleAsync(EmailVerificationRequested integrationEvent, CancellationToken cancellationToken)
    {
        var portal = portalOptions.Value;
        var verifyPage = $"{portal.BaseUrl.TrimEnd('/')}/{portal.VerifyEmailPath.TrimStart('/')}";

        var verificationLink = QueryHelpers.AddQueryString(verifyPage, new Dictionary<string, string?>
        {
            ["userId"] = integrationEvent.UserId.ToString(),
            ["token"] = integrationEvent.Token
        });

        // Simulated delivery: the link carries a one-time token, which is only acceptable in a log because no real email is sent.
        // A real IEmailSender replaces this line.
        logger.LogInformation(
            "Simulated email to {Email}: hi {FirstName}, verify your account at {VerificationLink}",
            integrationEvent.Email,
            integrationEvent.FirstName,
            verificationLink);

        return Task.CompletedTask;
    }
}
