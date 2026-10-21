namespace InvoiceTracker.Business.Domain.Businesses;

/// <summary>Printed on the tax invoice so customers can pay by EFT.</summary>
public sealed class BankDetails
{
    public Guid Id { get; private set; }
    public string BankName { get; private set; } = string.Empty;
    public string AccountHolder { get; private set; } = string.Empty;
    public string AccountNumber { get; private set; } = string.Empty;
    public BankAccountType AccountType { get; private set; }
    public string BranchCode { get; private set; } = string.Empty;
    public string? BranchName { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public Guid? ModifiedBy { get; private set; }

    public byte[] RowVersion { get; private set; } = [];
}
