using InvoiceTracker.Business.Domain.Common;

namespace InvoiceTracker.Business.Domain.Customers;

public sealed class Customer
{
    private Customer()
    {
    }

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

    /// <summary>The id is left empty for EF Core's sequential GUID generator to fill on Add.</summary>
    public static Customer Create(Guid businessId, string name, string email, Guid createdBy, DateTime now)
    {
        if (businessId == Guid.Empty)
        {
            throw new ArgumentException("A customer belongs to a business.", nameof(businessId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Customer
        {
            BusinessId = businessId,
            Name = name.Trim(),
            EmailAddress = EmailAddress.Create(email, createdBy, now),
            CreatedAt = now,
            CreatedBy = createdBy
        };
    }

    public void UpdateDetails(string name, string? customerReference, string? vatNumber, Guid modifiedBy, DateTime now)
    {
        EnsureNotArchived();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        CustomerReference = NullIfBlank(customerReference);
        VatNumber = NullIfBlank(vatNumber);
        Touch(modifiedBy, now);
    }

    public void SetEmail(string email, Guid modifiedBy, DateTime now)
    {
        EnsureNotArchived();
        EmailAddress.Update(email, modifiedBy, now);
        Touch(modifiedBy, now);
    }

    public void SetContactNumber(string number, ContactNumberType type, Guid modifiedBy, DateTime now)
    {
        EnsureNotArchived();

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

    /// <summary>Returns the removed row so the caller can delete it.</summary>
    public ContactNumber? ClearContactNumber(Guid modifiedBy, DateTime now)
    {
        EnsureNotArchived();

        var removed = ContactNumber;
        ContactNumber = null;
        ContactNumberId = null;
        Touch(modifiedBy, now);

        return removed;
    }

    public void SetBillingAddress(string line1, string? line2, string suburb, string city, string postalCode, Guid modifiedBy, DateTime now)
    {
        EnsureNotArchived();

        if (BillingAddress is null)
        {
            BillingAddress = Address.Create(line1, line2, suburb, city, postalCode, modifiedBy, now);
        }
        else
        {
            BillingAddress.Update(line1, line2, suburb, city, postalCode, modifiedBy, now);
        }

        Touch(modifiedBy, now);
    }

    /// <summary>Returns the removed row so the caller can delete it.</summary>
    public Address? ClearBillingAddress(Guid modifiedBy, DateTime now)
    {
        EnsureNotArchived();

        var removed = BillingAddress;
        BillingAddress = null;
        BillingAddressId = null;
        Touch(modifiedBy, now);

        return removed;
    }

    /// <summary>Archiving twice is a no-op, so DELETE stays idempotent.</summary>
    public void Archive(Guid modifiedBy, DateTime now)
    {
        if (IsArchived)
        {
            return;
        }

        IsArchived = true;
        Touch(modifiedBy, now);
    }

    private void EnsureNotArchived()
    {
        if (IsArchived)
        {
            throw new InvalidOperationException("An archived customer cannot be changed.");
        }
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
