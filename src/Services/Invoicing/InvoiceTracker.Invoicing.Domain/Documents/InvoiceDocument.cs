namespace InvoiceTracker.Invoicing.Domain.Documents;

/// <summary>The stored PDF of a sent invoice. Written once, never changed.</summary>
public sealed class InvoiceDocument
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public string BlobPath { get; private set; } = string.Empty;

    /// <summary>SHA-256 of the file, hex encoded.</summary>
    public string ContentHash { get; private set; } = string.Empty;

    public long SizeBytes { get; private set; }
    public DateTime GeneratedAt { get; private set; }
}
