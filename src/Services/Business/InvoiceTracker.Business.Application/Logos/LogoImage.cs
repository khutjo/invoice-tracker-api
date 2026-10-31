namespace InvoiceTracker.Business.Application.Logos;

/// <summary>
/// Logo rules from the brief: PNG or JPEG, at most 1 MB. The type comes from the file's signature,
/// never from the name or the Content-Type the client sent.
/// </summary>
public static class LogoImage
{
    public const long MaxSizeBytes = 1024 * 1024;

    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];

    public static LogoImageType? Detect(ReadOnlySpan<byte> content)
    {
        if (content.StartsWith(PngSignature))
        {
            return LogoImageType.Png;
        }

        if (content.StartsWith(JpegSignature))
        {
            return LogoImageType.Jpeg;
        }

        return null;
    }

    public static string ContentType(LogoImageType imageType)
    {
        return imageType == LogoImageType.Png ? "image/png" : "image/jpeg";
    }

    public static string Extension(LogoImageType imageType)
    {
        return imageType == LogoImageType.Png ? ".png" : ".jpg";
    }

    public static LogoImageType? FromExtension(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => LogoImageType.Png,
            ".jpg" => LogoImageType.Jpeg,
            _ => null
        };
    }
}
