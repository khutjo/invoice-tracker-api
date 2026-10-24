using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.Identity.Infrastructure.Tokens;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer
    {
        get; init;
    } = string.Empty;

    [Required]
    public string Audience
    {
        get; init;
    } = string.Empty;

    [Range(1, 60)]
    public int AccessTokenMinutes
    {
        get; init;
    } = 15;

    [Range(1, 90)]
    public int RefreshTokenDays
    {
        get; init;
    } = 14;

    [Required]
    public string KeyId
    {
        get; init;
    } = string.Empty;

    [Required]
    public string SigningKey
    {
        get; init;
    } = string.Empty;
}
