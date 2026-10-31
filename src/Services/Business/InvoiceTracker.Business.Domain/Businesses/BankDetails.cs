namespace InvoiceTracker.Business.Domain.Businesses;

/// <summary>Printed on the tax invoice so customers can pay by EFT.</summary>
public sealed class BankDetails
{
    private BankDetails()
    {
    }

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

    public static BankDetails Create(
        string bankName,
        string accountHolder,
        string accountNumber,
        BankAccountType accountType,
        string branchCode,
        string? branchName,
        Guid createdBy,
        DateTime now)
    {
        var bankDetails = new BankDetails
        {
            CreatedAt = now,
            CreatedBy = createdBy
        };
        bankDetails.SetAccount(bankName, accountHolder, accountNumber, accountType, branchCode, branchName);

        return bankDetails;
    }

    public void Update(
        string bankName,
        string accountHolder,
        string accountNumber,
        BankAccountType accountType,
        string branchCode,
        string? branchName,
        Guid modifiedBy,
        DateTime now)
    {
        SetAccount(bankName, accountHolder, accountNumber, accountType, branchCode, branchName);
        ModifiedAt = now;
        ModifiedBy = modifiedBy;
    }

    private void SetAccount(
        string bankName,
        string accountHolder,
        string accountNumber,
        BankAccountType accountType,
        string branchCode,
        string? branchName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bankName);
        ArgumentException.ThrowIfNullOrWhiteSpace(accountHolder);
        ArgumentException.ThrowIfNullOrWhiteSpace(accountNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(branchCode);

        BankName = bankName.Trim();
        AccountHolder = accountHolder.Trim();
        AccountNumber = accountNumber.Trim();
        AccountType = accountType;
        BranchCode = branchCode.Trim();
        BranchName = string.IsNullOrWhiteSpace(branchName) ? null : branchName.Trim();
    }
}
