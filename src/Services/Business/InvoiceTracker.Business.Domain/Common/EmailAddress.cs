namespace InvoiceTracker.Business.Domain.Common;

public sealed class EmailAddress
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public Guid? ModifiedBy { get; private set; }

    public byte[] RowVersion { get; private set; } = [];
}
