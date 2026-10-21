using InvoiceTracker.Business.Domain.Common;

namespace InvoiceTracker.Business.Domain.Businesses;

/// <summary>
/// The tenant root. Named BusinessProfile because "Business" clashes with the
/// InvoiceTracker.Business namespace; the table is still "businesses".
/// Contact details are null until the Owner completes the profile after registration.
/// </summary>
public sealed class BusinessProfile
{
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

    /// <summary>IANA zone used to decide when an invoice becomes overdue.</summary>
    public string TimeZone { get; private set; } = "Africa/Johannesburg";

    public string? LogoBlobPath { get; private set; }
    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public Guid? ModifiedBy { get; private set; }

    public byte[] RowVersion { get; private set; } = [];
}
