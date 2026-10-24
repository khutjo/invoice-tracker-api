using System.Net.Http.Json;
using InvoiceTracker.BuildingBlocks.Contracts.Events.IdentityModels;
using InvoiceTracker.BuildingBlocks.Messaging.Outbox;
using InvoiceTracker.BuildingBlocks.Messaging.Serialization;
using InvoiceTracker.Identity.Api.Models.Response;
using InvoiceTracker.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceTracker.Identity.IntegrationTests;

/// <summary>Small helpers so each test reads as the user journey it checks.</summary>
public sealed class AuthApiClient(IdentityApiFactory factory)
{
    public const string Password = "Correct-Horse-42";

    public HttpClient Http { get; } = factory.CreateClient();

    public static string NewEmail()
    {
        return $"owner-{Guid.NewGuid():N}@example.test";
    }

    public Task<HttpResponseMessage> RegisterAsync(string email, string password = Password)
    {
        return Http.PostAsJsonAsync("/api/v1/auth/register", new
        {
            businessName = "Acme Plumbing",
            firstName = "Thandi",
            lastName = "Mokoena",
            email,
            password
        });
    }

    public Task<HttpResponseMessage> LoginAsync(string email, string password = Password)
    {
        return Http.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
    }

    public Task<HttpResponseMessage> RefreshAsync(string refreshToken)
    {
        return Http.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken });
    }

    public Task<HttpResponseMessage> VerifyEmailAsync(Guid userId, string token)
    {
        return Http.PostAsJsonAsync("/api/v1/auth/verify-email", new { userId, token });
    }

    /// <summary>Registers, then follows the emailed link, the way the portal's verify screen would.</summary>
    public async Task<RegisterUserResponse> RegisterVerifiedAsync(string email)
    {
        var response = await RegisterAsync(email);
        response.EnsureSuccessStatusCode();
        var registered = (await response.Content.ReadFromJsonAsync<RegisterUserResponse>())!;

        var link = await GetVerificationEventAsync(registered.UserId);
        var token = QueryHelpers.ParseQuery(new Uri(link.VerificationUrl).Query)["token"].ToString();

        (await VerifyEmailAsync(registered.UserId, token)).EnsureSuccessStatusCode();
        return registered;
    }

    public async Task<EmailVerificationRequested> GetVerificationEventAsync(Guid userId)
    {
        var payloads = await QueryAsync(db => db.Set<OutboxMessage>()
            .Where(m => m.Type == nameof(EmailVerificationRequested))
            .Select(m => m.Payload)
            .ToListAsync());

        return payloads
            .Select(EventSerializer.Deserialize<EmailVerificationRequested>)
            .Single(e => e.UserId == userId);
    }

    public async Task<T> QueryAsync<T>(Func<IdentityServiceDbContext, Task<T>> query)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await query(scope.ServiceProvider.GetRequiredService<IdentityServiceDbContext>());
    }
}
