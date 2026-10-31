using InvoiceTracker.Business.Domain.Common;

namespace InvoiceTracker.Business.Domain.Businesses;

public sealed class BusinessProfile
{
    public const int MaxPaymentTermsDays = 365;

    private BusinessProfile()
    {
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? RegistrationNumber { get; private set; }
    public string? VatNumber { get; private set; }
    public Guid? SupportEmailId { get; private set; }
    public EmailAddress? SupportEmail { get; private set; }
    public Guid? ContactNumberId { get; private set; }
    public ContactNumber? ContactNumber { get; private set; }

    public Guid? AddressId { get; private set; }
    public Address? Address { get; private set; }

    public Guid? BankDetailsId { get; private set; }
    public BankDetails? BankDetails { get; private set; }

    public int DefaultPaymentTermsDays { get; private set; } = 30;

    public string TimeZone { get; private set; } = "Africa/Johannesburg";

    public string? LogoBlobPath { get; private set; }
    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public Guid? ModifiedBy { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public static BusinessProfile Create(Guid businessId, string name, Guid createdBy, DateTime now)
    {
        if (businessId == Guid.Empty)
        {
            throw new ArgumentException("A business needs the id Identity issued.", nameof(businessId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new BusinessProfile
        {
            Id = businessId,
            Name = name.Trim(),
            CreatedAt = now,
            CreatedBy = createdBy
        };
    }

    public void UpdateDetails(string name, string? registrationNumber, string? vatNumber, Guid modifiedBy, DateTime now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        RegistrationNumber = NullIfBlank(registrationNumber);
        VatNumber = NullIfBlank(vatNumber);
        Touch(modifiedBy, now);
    }

    public void UpdateSettings(int defaultPaymentTermsDays, string timeZone, Guid modifiedBy, DateTime now)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(defaultPaymentTermsDays);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(defaultPaymentTermsDays, MaxPaymentTermsDays);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZone);

        DefaultPaymentTermsDays = defaultPaymentTermsDays;
        TimeZone = timeZone.Trim();
        Touch(modifiedBy, now);
    }

    public void SetSupportEmail(string email, Guid modifiedBy, DateTime now)
    {
        if (SupportEmail is null)
        {
            SupportEmail = EmailAddress.Create(email, modifiedBy, now);
        }
        else
        {
            SupportEmail.Update(email, modifiedBy, now);
        }

        Touch(modifiedBy, now);
    }

    public EmailAddress? ClearSupportEmail(Guid modifiedBy, DateTime now)
    {
        var removed = SupportEmail;
        SupportEmail = null;
        SupportEmailId = null;
        Touch(modifiedBy, now);

        return removed;
    }

    public void SetContactNumber(string number, ContactNumberType type, Guid modifiedBy, DateTime now)
    {
        if (ContactNumber is null)
        {
            ContactNumber = ContactNumber.Create(number, type, modifiedBy, now);
        }
        else
        {
            ContactNumber.Update(number, type, modifiedBy, now);
        }

        Touch(modifiedBy, now);
    }

    public ContactNumber? ClearContactNumber(Guid modifiedBy, DateTime now)
    {
        var removed = ContactNumber;
        ContactNumber = null;
        ContactNumberId = null;
        Touch(modifiedBy, now);

        return removed;
    }

    public void SetAddress(string line1, string? line2, string suburb, string city, string postalCode, Guid modifiedBy, DateTime now)
    {
        if (Address is null)
        {
            Address = Address.Create(line1, line2, suburb, city, postalCode, modifiedBy, now);
        }
        else
        {
            Address.Update(line1, line2, suburb, city, postalCode, modifiedBy, now);
        }

        Touch(modifiedBy, now);
    }

    public Address? ClearAddress(Guid modifiedBy, DateTime now)
    {
        var removed = Address;
        Address = null;
        AddressId = null;
        Touch(modifiedBy, now);

        return removed;
    }

    public void SetBankDetails(
        string bankName,
        string accountHolder,
        string accountNumber,
        BankAccountType accountType,
        string branchCode,
        string? branchName,
        Guid modifiedBy,
        DateTime now)
    {
        if (BankDetails is null)
        {
            BankDetails = BankDetails.Create(bankName, accountHolder, accountNumber, accountType, branchCode, branchName, modifiedBy, now);
        }
        else
        {
            BankDetails.Update(bankName, accountHolder, accountNumber, accountType, branchCode, branchName, modifiedBy, now);
        }

        Touch(modifiedBy, now);
    }

    public BankDetails? ClearBankDetails(Guid modifiedBy, DateTime now)
    {
        var removed = BankDetails;
        BankDetails = null;
        BankDetailsId = null;
        Touch(modifiedBy, now);

        return removed;
    }

    public string? ReplaceLogo(string logoBlobPath, Guid modifiedBy, DateTime now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logoBlobPath);

        var previous = LogoBlobPath;
        LogoBlobPath = logoBlobPath;
        Touch(modifiedBy, now);

        return previous;
    }

    private void Touch(Guid modifiedBy, DateTime now)
    {
        ModifiedAt = now;
        ModifiedBy = modifiedBy;
    }

    private static string? NullIfBlank(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
