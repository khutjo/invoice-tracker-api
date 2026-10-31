namespace InvoiceTracker.Business.Domain.ReferenceData;

/// <summary>
/// A South African postal code from the SAPO list. One code covers several suburbs, so the same
/// code appears once per suburb and kind. Read-only: seeded by migration, never changed by the API.
/// </summary>
public sealed class PostalCode
{
    private PostalCode()
    {
    }

    public int Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Suburb { get; private set; } = string.Empty;
    public string Town { get; private set; } = string.Empty;
    public PostalCodeKind Kind { get; private set; }
}
