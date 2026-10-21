namespace InvoiceTracker.Business.Domain.Common;

public sealed class ContactNumber
{
    public Guid Id { get; private set; }

    /// <summary>Stored in E.164 form, for example +27821234567.</summary>
    public string Number { get; private set; } = string.Empty;

    public ContactNumberType ContactNumberType { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public Guid? ModifiedBy { get; private set; }

    public byte[] RowVersion { get; private set; } = [];
}
