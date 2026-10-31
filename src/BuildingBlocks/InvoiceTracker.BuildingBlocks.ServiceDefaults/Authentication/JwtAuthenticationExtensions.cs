using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace InvoiceTracker.BuildingBlocks.ServiceDefaults.Authentication;

public static class JwtAuthenticationExtensions
{
    /// <summary>
    /// Validates Identity's access tokens locally with its RSA public key (Jwt:PublicKey, base64 SubjectPublicKeyInfo).
    /// Claims keep their JWT names (sub, business_id, role) instead of being remapped to long URIs.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection("Jwt");
        var issuer = GetRequired(jwt, "Issuer");
        var audience = GetRequired(jwt, "Audience");
        var publicKey = GetRequired(jwt, "PublicKey");

        // Lives for the whole process: the security key holds a reference to it
        var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new RsaSecurityKey(rsa),
                    ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = InvoiceTrackerClaimTypes.Role,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.OwnerOnly, policy => policy.RequireRole(InvoiceTrackerRoles.Owner));

        return services;
    }

    private static string GetRequired(IConfigurationSection section, string key)
    {
        var value = section[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"'{section.Path}:{key}' is not configured.");
        }

        return value;
    }
}
