using InvoiceTracker.Business.Domain.Common;

namespace InvoiceTracker.Business.Domain.Customers;

public sealed class Customer
{
    public Guid Id { get; private set; }
    public Guid BusinessId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    /// <summary>Required: notifications go to this address.</summary>
    public Guid EmailAddressId { get; private set; }
    public EmailAddress EmailAddress { get; private set; } = null!;

    public Guid? ContactNumberId { get; private set; }
    public ContactNumber? ContactNumber { get; private set; }

    public Guid? BillingAddressId { get; private set; }
    public Address? BillingAddress { get; private set; }

    /// <summary>The business's own reference for this customer, such as an account code.</summary>
    public string? CustomerReference { get; private set; }

    public string? VatNumber { get; private set; }

    /// <summary>Archived customers keep their invoices but no longer appear in pickers.</summary>
    public bool IsArchived { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public Guid? ModifiedBy { get; private set; }

    public byte[] RowVersion { get; private set; } = [];
}
