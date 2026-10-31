namespace InvoiceTracker.Business.Domain.ReferenceData;

/// <summary>
/// A South African bank and its universal branch code. Read-only: seeded by migration,
/// never changed by the API.
/// </summary>
public sealed class Bank
{
    private Bank()
    {
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string UniversalBranchCode { get; private set; } = string.Empty;
}
