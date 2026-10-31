namespace InvoiceTracker.Business.Application.Logos;

/// <summary>
/// Where business logos live. Paths are relative and content-hashed ({businessId}/{sha256}.{ext}),
/// so a stored file never changes and can be cached for a long time.
/// </summary>
public interface ILogoStore
{
    Task<string> SaveAsync(Guid businessId, byte[] content, LogoImageType imageType, CancellationToken cancellationToken);

    Task<StoredLogo?> ReadAsync(string path, CancellationToken cancellationToken);

    Task DeleteAsync(string path, CancellationToken cancellationToken);
}
