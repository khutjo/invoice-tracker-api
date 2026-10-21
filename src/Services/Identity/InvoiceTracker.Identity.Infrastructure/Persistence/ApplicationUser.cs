using Microsoft.AspNetCore.Identity;

namespace InvoiceTracker.Identity.Infrastructure.Persistence;

/// <summary>
/// A user belongs to exactly one business. Email, password hash and the
/// verified flag (EmailConfirmed) come from IdentityUser.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public Guid BusinessId { get; set; }
    public UserRole Role { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>The Owner who invited this user; null for the Owner created at registration.</summary>
    public Guid? InvitedBy { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
