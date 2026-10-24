namespace InvoiceTracker.Identity.Infrastructure.Persistence;

/// <summary>
/// Only a SHA-256 hash of the token is stored, so a leaked database cannot be replayed.
/// Every token rotated from one login shares a <see cref="FamilyId"/>: if a revoked token
/// is presented again, the whole family is revoked and that session ends.
/// </summary>
public sealed class RefreshToken
{
    public Guid Id
    {
        get; set;
    }

    public Guid UserId
    {
        get; set;
    }

    public Guid FamilyId
    {
        get; set;
    }

    public string TokenHash
    {
        get; set;
    } = string.Empty;

    public DateTime CreatedAt
    {
        get; set;
    }

    public DateTime ExpiresAt
    {
        get; set;
    }

    public DateTime? RevokedAt
    {
        get; set;
    }
}
