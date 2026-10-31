using System.Security.Cryptography;
using InvoiceTracker.Business.Application.Logos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace InvoiceTracker.Business.Infrastructure.Logos;

public sealed class FileSystemLogoStore(IOptions<LogoStorageOptions> options, IHostEnvironment environment) : ILogoStore
{
    private readonly string _rootPath = Path.GetFullPath(options.Value.RootPath, environment.ContentRootPath);

    // in this case the file will be stored in the container but this would link to a blob store on azure/aws in prod
    public async Task<string> SaveAsync(Guid businessId, byte[] content, LogoImageType imageType, CancellationToken cancellationToken)
    {
        var hash = Convert.ToHexStringLower(SHA256.HashData(content));
        var path = $"{businessId}/{hash}{LogoImage.Extension(imageType)}";
        var fullPath = ToFullPath(path);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, content, cancellationToken);

        return path;
    }

    public async Task<StoredLogo?> ReadAsync(string path, CancellationToken cancellationToken)
    {
        var fullPath = ToFullPath(path);
        var imageType = LogoImage.FromExtension(path);
        if (imageType is null || !File.Exists(fullPath))
        {
            return null;
        }

        var content = await File.ReadAllBytesAsync(fullPath, cancellationToken);

        return new StoredLogo(content, imageType.Value, Path.GetFileName(path));
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken)
    {
        File.Delete(ToFullPath(path));

        return Task.CompletedTask;
    }

    private string ToFullPath(string path)
    {
        var fullPath = Path.GetFullPath(path, _rootPath);
        if (!fullPath.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Logo path is outside the logo store.");
        }

        return fullPath;
    }
}
