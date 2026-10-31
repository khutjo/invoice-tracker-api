using System.Net;
using System.Net.Http.Json;
using InvoiceTracker.BuildingBlocks.Contracts.Events.IdentityModels;
using InvoiceTracker.BuildingBlocks.Messaging.Outbox;
using InvoiceTracker.Identity.Api.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Identity.IntegrationTests;

public sealed class AuthEndpointsTests(IdentityApiFactory factory) : IClassFixture<IdentityApiFactory>
{
    private readonly AuthApiClient _api = new(factory);

    [Fact]
    public async Task Register_creates_an_unverified_owner_and_queues_both_events()
    {
        var email = AuthApiClient.NewEmail();

        var response = await _api.RegisterAsync(email);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = (await response.Content.ReadFromJsonAsync<RegisterUserResponse>())!;
        body.Email.ShouldBe(email);
        body.Role.ShouldBe("Owner");
        body.EmailVerified.ShouldBeFalse();

        var eventTypes = await _api.QueryAsync(db => db.Set<OutboxMessage>()
            .Where(m => m.Payload.Contains(body.UserId.ToString()))
            .Select(m => m.Type)
            .ToListAsync());
        eventTypes.ShouldBe(new[] { nameof(BusinessRegistered), nameof(EmailVerificationRequested) }, ignoreOrder: true);

        var verification = await _api.GetVerificationEventAsync(body.UserId);
        verification.UserId.ShouldBe(body.UserId);
        verification.Token.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_with_an_email_already_in_use_returns_a_field_error()
    {
        var email = AuthApiClient.NewEmail();
        (await _api.RegisterAsync(email)).EnsureSuccessStatusCode();

        var response = await _api.RegisterAsync(email);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!;
        problem.Errors.ShouldContainKey("email");
    }

    [Fact]
    public async Task Register_with_a_weak_password_returns_a_field_error()
    {
        var response = await _api.RegisterAsync(AuthApiClient.NewEmail(), password: "short");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!;
        problem.Errors.ShouldContainKey("password");
    }

    [Fact]
    public async Task Login_before_verifying_the_email_returns_403()
    {
        var email = AuthApiClient.NewEmail();
        (await _api.RegisterAsync(email)).EnsureSuccessStatusCode();

        var response = await _api.LoginAsync(email);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Verify_email_with_a_tampered_token_returns_400()
    {
        var email = AuthApiClient.NewEmail();
        var registered = (await (await _api.RegisterAsync(email)).Content.ReadFromJsonAsync<RegisterUserResponse>())!;

        var response = await _api.VerifyEmailAsync(registered.UserId, "bm90LWEtcmVhbC10b2tlbg");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_after_verifying_returns_tokens_for_the_owner()
    {
        var email = AuthApiClient.NewEmail();
        var registered = await _api.RegisterVerifiedAsync(email);

        var response = await _api.LoginAsync(email);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<LoginResponse>())!;
        body.UserId.ShouldBe(registered.UserId);
        body.BusinessId.ShouldBe(registered.BusinessId);
        body.Role.ShouldBe("Owner");
        body.JwtToken.ShouldNotBeNullOrWhiteSpace();
        body.RefreshToken.ShouldNotBeNullOrWhiteSpace();
        body.ExpiresIn.ShouldBeInRange(14 * 60, 15 * 60);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("wrong-password")]
    public async Task Login_failures_look_the_same_whatever_the_cause(string failure)
    {
        var email = AuthApiClient.NewEmail();
        await _api.RegisterVerifiedAsync(email);

        var response = failure == "unknown"
            ? await _api.LoginAsync(AuthApiClient.NewEmail())
            : await _api.LoginAsync(email, "Wrong-Password-1");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        var problem = (await response.Content.ReadFromJsonAsync<ProblemDetails>())!;
        problem.Title.ShouldBe("Invalid email or password.");
    }

    [Fact]
    public async Task Five_wrong_passwords_lock_the_account_even_for_the_right_password()
    {
        var email = AuthApiClient.NewEmail();
        await _api.RegisterVerifiedAsync(email);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            (await _api.LoginAsync(email, "Wrong-Password-1")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        var response = await _api.LoginAsync(email);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_rotates_the_refresh_token()
    {
        var first = await LoginVerifiedOwnerAsync();

        var response = await _api.RefreshAsync(first.RefreshToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var rotated = (await response.Content.ReadFromJsonAsync<RefreshTokenResponse>())!;
        rotated.RefreshToken.ShouldNotBe(first.RefreshToken);
        rotated.JwtToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Reusing_a_rotated_refresh_token_ends_the_whole_session()
    {
        var first = await LoginVerifiedOwnerAsync();
        var rotated = (await (await _api.RefreshAsync(first.RefreshToken)).Content.ReadFromJsonAsync<RefreshTokenResponse>())!;

        var replay = await _api.RefreshAsync(first.RefreshToken);

        replay.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await _api.RefreshAsync(rotated.RefreshToken)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_revokes_the_refresh_token()
    {
        var session = await LoginVerifiedOwnerAsync();

        var logout = await _api.Http.PostAsJsonAsync("/api/v1/auth/logout", new { refreshToken = session.RefreshToken });

        logout.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await _api.RefreshAsync(session.RefreshToken)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private async Task<LoginResponse> LoginVerifiedOwnerAsync()
    {
        var email = AuthApiClient.NewEmail();
        await _api.RegisterVerifiedAsync(email);

        var response = await _api.LoginAsync(email);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResponse>())!;
    }
}
