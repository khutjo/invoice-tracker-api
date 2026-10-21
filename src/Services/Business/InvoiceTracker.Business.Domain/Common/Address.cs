namespace InvoiceTracker.Business.Domain.Common;

/// <summary>
/// South African postal address. Line1 holds the unit, street or PO Box line;
/// Line2 the next line when there is one.
/// </summary>
public sealed class Address
{
    public Guid Id { get; private set; }
    public string Line1 { get; private set; } = string.Empty;
    public string? Line2 { get; private set; }
    public string Suburb { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = "ZAF";

    public DateTime CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public Guid? ModifiedBy { get; private set; }

    public byte[] RowVersion { get; private set; } = [];
}
