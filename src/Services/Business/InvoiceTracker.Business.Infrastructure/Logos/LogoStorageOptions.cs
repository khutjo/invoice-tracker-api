using System.ComponentModel.DataAnnotations;

namespace InvoiceTracker.Business.Infrastructure.Logos;

public sealed class LogoStorageOptions
{
    public const string SectionName = "LogoStorage";

    /// <summary>Folder for logo files; a relative path is resolved against the content root.</summary>
    [Required]
    public string RootPath
    {
        get; init;
    } = string.Empty;
}
