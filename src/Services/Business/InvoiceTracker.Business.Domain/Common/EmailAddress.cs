namespace InvoiceTracker.Business.Domain.Common;

public sealed class EmailAddress
{
    private EmailAddress()
    {
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public Guid? ModifiedBy { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public static EmailAddress Create(string email, Guid createdBy, DateTime now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return new EmailAddress
        {
            Email = email.Trim(),
            CreatedAt = now,
            CreatedBy = createdBy
        };
    }

    public void Update(string email, Guid modifiedBy, DateTime now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        Email = email.Trim();
        ModifiedAt = now;
        ModifiedBy = modifiedBy;
    }
}
