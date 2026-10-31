namespace InvoiceTracker.Business.Application.Logos;

public sealed record StoredLogo(byte[] Content, LogoImageType ImageType, string FileName);
