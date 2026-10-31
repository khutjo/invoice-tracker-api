namespace InvoiceTracker.Business.Domain.Common;

/// <summary>
/// South African postal address. Line1 holds the unit, street or PO Box line;
/// Line2 the next line when there is one.
/// </summary>
public sealed class Address
{
    private Address()
    {
    }

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

    public static Address Create(string line1, string? line2, string suburb, string city, string postalCode, Guid createdBy, DateTime now)
    {
        var address = new Address
        {
            CreatedAt = now,
            CreatedBy = createdBy
        };
        address.SetLines(line1, line2, suburb, city, postalCode);

        return address;
    }

    public void Update(string line1, string? line2, string suburb, string city, string postalCode, Guid modifiedBy, DateTime now)
    {
        SetLines(line1, line2, suburb, city, postalCode);
        ModifiedAt = now;
        ModifiedBy = modifiedBy;
    }

    private void SetLines(string line1, string? line2, string suburb, string city, string postalCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(line1);
        ArgumentException.ThrowIfNullOrWhiteSpace(suburb);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(postalCode);

        Line1 = line1.Trim();
        Line2 = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim();
        Suburb = suburb.Trim();
        City = city.Trim();
        PostalCode = postalCode.Trim();
    }
}
