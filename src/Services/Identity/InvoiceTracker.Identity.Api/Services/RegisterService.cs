using System.Text;
using InvoiceTracker.BuildingBlocks.Contracts.Events.IdentityModels;
using InvoiceTracker.BuildingBlocks.Messaging.Outbox;
using InvoiceTracker.Identity.Api.Exceptions;
using InvoiceTracker.Identity.Api.Interfaces;
using InvoiceTracker.Identity.Api.Models.Request;
using InvoiceTracker.Identity.Api.Models.Response;
using InvoiceTracker.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Identity.Api.Services;

public sealed class RegisterService(
    UserManager<ApplicationUser> userManager,
    IdentityServiceDbContext dbContext,
    TimeProvider timeProvider) : IRegisterService
{
    public async Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        var user = await strategy.ExecuteAsync(async () =>
        {
            dbContext.ChangeTracker.Clear();
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var now = timeProvider.GetUtcNow();

            var newUser = new ApplicationUser
            {
                BusinessId = Guid.CreateVersion7(),
                Role = UserRole.Owner,
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedAt = now.UtcDateTime
            };

            var result = await userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                throw new RegisterException("Registration failed.", StatusCodes.Status400BadRequest, ToFieldErrors(result));
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(newUser);

            dbContext.AddOutboxMessage(new BusinessRegistered
            {
                OccurredAt = now,
                BusinessId = newUser.BusinessId,
                BusinessName = request.BusinessName,
                OwnerUserId = newUser.Id,
                OwnerEmail = request.Email,
                OwnerFirstName = request.FirstName,
                OwnerLastName = request.LastName
            });

            dbContext.AddOutboxMessage(new EmailVerificationRequested
            {
                OccurredAt = now,
                BusinessId = newUser.BusinessId,
                UserId = newUser.Id,
                Email = request.Email,
                FirstName = request.FirstName,
                Token = EncodeToken(token)
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return newUser;
        });

        return new RegisterUserResponse
        {
            UserId = user.Id,
            BusinessId = user.BusinessId,
            Email = request.Email,
            Role = user.Role.ToString(),
            EmailVerified = false
        };
    }

    public async Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null || !TryDecodeToken(request.Token, out var token))
        {
            throw InvalidVerificationLink();
        }

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            throw InvalidVerificationLink();
        }
    }

    private static string EncodeToken(string token)
    {
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    }

    private static bool TryDecodeToken(string encoded, out string token)
    {
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encoded));
            return true;
        }
        catch (FormatException)
        {
            token = string.Empty;
            return false;
        }
    }

    private static RegisterException InvalidVerificationLink()
    {
        return new RegisterException("The verification link is invalid or has expired.", StatusCodes.Status400BadRequest);
    }

    private static Dictionary<string, string[]> ToFieldErrors(IdentityResult result)
    {
        return result.Errors
            .GroupBy(e => e.Code.StartsWith("Password", StringComparison.Ordinal) ? "password" : "email")
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
    }
}
