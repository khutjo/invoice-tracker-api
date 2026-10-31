using InvoiceTracker.Business.Api.Exceptions;
using InvoiceTracker.Business.Api.Interfaces;
using InvoiceTracker.Business.Api.Lookups;
using InvoiceTracker.Business.Api.Models.Common;
using InvoiceTracker.Business.Api.Models.Request;
using InvoiceTracker.Business.Api.Models.Response;
using InvoiceTracker.Business.Application.Logos;
using InvoiceTracker.Business.Domain.Businesses;
using InvoiceTracker.Business.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Business.Api.Services;

public sealed class BusinessService(
    BusinessDbContext dbContext,
    ILogoStore logoStore,
    ReferenceDataValidator referenceDataValidator,
    TimeProvider timeProvider) : IBusinessService
{
    private const int RowVersionLength = 8;

    public async Task<BusinessDetailsResponse> CreateBusinessAsync(CreateBusinessRequest request, Guid businessId, Guid userId, CancellationToken cancellationToken)
    {
        if (await dbContext.Businesses.AnyAsync(b => b.Id == businessId, cancellationToken))
        {
            throw AlreadyExists();
        }

        await ValidateReferenceDataAsync(request, cancellationToken);

        var now = Now();
        var business = BusinessProfile.Create(businessId, request.Name, userId, now);
        ApplyDetails(business, request, userId, now);
        dbContext.Businesses.Add(business);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // Two create requests raced past the AnyAsync check; the primary key let only one in
            throw AlreadyExists();
        }

        return ToDetailsResponse(business);
    }

    public async Task<BusinessDetailsResponse> GetBusinessDetailsAsync(Guid businessId, CancellationToken cancellationToken)
    {
        var business = await WithDetails()
            .AsNoTracking()
            .SingleOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            ?? throw BusinessException.NotFound();

        return ToDetailsResponse(business);
    }

    public async Task<BusinessDetailsResponse> UpdateBusinessDetailsAsync(UpdateBusinessDetailsRequest request, Guid businessId, Guid userId, CancellationToken cancellationToken)
    {
        var business = await WithDetails()
            .SingleOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            ?? throw BusinessException.NotFound();

        await ValidateReferenceDataAsync(request, cancellationToken);
        ExpectVersion(business, request.Version);
        ApplyDetails(business, request, userId, Now());
        await SaveAsync(cancellationToken);

        return ToDetailsResponse(business);
    }

    public async Task<BusinessSettingsResponse> GetBusinessSettingsAsync(Guid businessId, CancellationToken cancellationToken)
    {
        var business = await dbContext.Businesses
            .AsNoTracking()
            .SingleOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            ?? throw BusinessException.NotFound();

        return ToSettingsResponse(business);
    }

    public async Task<BusinessSettingsResponse> SaveBusinessSettingAsync(SaveBusinessSettingsRequest request, Guid businessId, Guid userId, CancellationToken cancellationToken)
    {
        if (!LookupCatalog.IsSupportedTimeZone(request.TimeZone))
        {
            throw BusinessException.Invalid("timeZone", "Choose a time zone from GET /api/v1/business/lookups.");
        }

        var business = await dbContext.Businesses
            .SingleOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            ?? throw BusinessException.NotFound();

        ExpectVersion(business, request.Version);
        business.UpdateSettings(request.DefaultPaymentTermsDays, request.TimeZone, userId, Now());
        await SaveAsync(cancellationToken);

        return ToSettingsResponse(business);
    }

    public async Task<SaveLogoResponse> SaveLogoAsync(SaveLogoRequest request, Guid businessId, Guid userId, CancellationToken cancellationToken)
    {
        var file = request.File;
        if (file.Length is 0 or > LogoImage.MaxSizeBytes)
        {
            throw InvalidLogo();
        }

        byte[] content;
        await using (var buffer = new MemoryStream((int)file.Length))
        {
            await file.CopyToAsync(buffer, cancellationToken);
            content = buffer.ToArray();
        }

        var imageType = LogoImage.Detect(content) ?? throw InvalidLogo();

        var business = await dbContext.Businesses
            .SingleOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            ?? throw BusinessException.NotFound();

        var path = await logoStore.SaveAsync(businessId, content, imageType, cancellationToken);
        var previousPath = business.ReplaceLogo(path, userId, Now());
        await SaveAsync(cancellationToken);

        // Same bytes give the same content-hashed path, so only a different file is removed
        if (previousPath is not null && previousPath != path)
        {
            await logoStore.DeleteAsync(previousPath, cancellationToken);
        }

        return new SaveLogoResponse
        {
            ContentType = LogoImage.ContentType(imageType),
            SizeBytes = content.Length,
            Version = Convert.ToBase64String(business.RowVersion)
        };
    }

    public async Task<LogoResponse> GetLogoAsync(Guid businessId, CancellationToken cancellationToken)
    {
        var path = await dbContext.Businesses
            .AsNoTracking()
            .Where(b => b.Id == businessId)
            .Select(b => b.LogoBlobPath)
            .SingleOrDefaultAsync(cancellationToken);

        var logo = path is null ? null : await logoStore.ReadAsync(path, cancellationToken);
        if (logo is null)
        {
            throw new BusinessException("No logo has been uploaded.", StatusCodes.Status404NotFound);
        }

        return new LogoResponse
        {
            Content = logo.Content,
            ContentType = LogoImage.ContentType(logo.ImageType),
            FileName = logo.FileName
        };
    }

    private IQueryable<BusinessProfile> WithDetails()
    {
        return dbContext.Businesses
            .Include(b => b.SupportEmail)
            .Include(b => b.ContactNumber)
            .Include(b => b.Address)
            .Include(b => b.BankDetails);
    }

    /// <summary>Postal code and bank/branch code must exist in the reference data; all errors are returned together.</summary>
    private async Task ValidateReferenceDataAsync(BusinessDetailsRequest request, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        await referenceDataValidator.ValidateAddressAsync(request.Address, "address", errors, cancellationToken);
        await referenceDataValidator.ValidateBankDetailsAsync(request.BankDetails, "bankDetails", errors, cancellationToken);

        if (errors.Count > 0)
        {
            throw BusinessException.Invalid(errors);
        }
    }

    /// <summary>A detail the request leaves out is unlinked and its row deleted.</summary>
    private void ApplyDetails(BusinessProfile business, BusinessDetailsRequest request, Guid userId, DateTime now)
    {
        business.UpdateDetails(request.Name, request.RegistrationNumber, request.VatNumber, userId, now);

        if (!string.IsNullOrWhiteSpace(request.SupportEmail))
        {
            business.SetSupportEmail(request.SupportEmail, userId, now);
        }
        else
        {
            RemoveDetail(business.ClearSupportEmail(userId, now));
        }

        if (request.ContactNumber is { } contact)
        {
            business.SetContactNumber(contact.Number, contact.Type, userId, now);
        }
        else
        {
            RemoveDetail(business.ClearContactNumber(userId, now));
        }

        if (request.Address is { } address)
        {
            business.SetAddress(address.Line1, address.Line2, address.Suburb, address.City, address.PostalCode, userId, now);
        }
        else
        {
            RemoveDetail(business.ClearAddress(userId, now));
        }

        if (request.BankDetails is { } bank)
        {
            business.SetBankDetails(bank.BankName, bank.AccountHolder, bank.AccountNumber, bank.AccountType, bank.BranchCode, bank.BranchName, userId, now);
        }
        else
        {
            RemoveDetail(business.ClearBankDetails(userId, now));
        }
    }

    private void RemoveDetail(object? detail)
    {
        if (detail is not null)
        {
            dbContext.Remove(detail);
        }
    }

    /// <summary>EF compares this against row_version in the UPDATE's WHERE clause, so a stale version updates nothing.</summary>
    private void ExpectVersion(BusinessProfile business, string version)
    {
        var expected = new byte[RowVersionLength];
        if (!Convert.TryFromBase64String(version, expected, out var written) || written != RowVersionLength)
        {
            throw BusinessException.Invalid("version", "Version is not valid. Send the version from the last read.");
        }

        dbContext.Entry(business).Property(b => b.RowVersion).OriginalValue = expected;
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw BusinessException.StaleVersion();
        }
    }

    private DateTime Now()
    {
        return timeProvider.GetUtcNow().UtcDateTime;
    }

    private static BusinessException AlreadyExists()
    {
        return new BusinessException("The business profile already exists.", StatusCodes.Status409Conflict);
    }

    private static BusinessException InvalidLogo()
    {
        return BusinessException.Invalid("file", "The logo must be a PNG or JPEG image of at most 1 MB.");
    }

    private static BusinessDetailsResponse ToDetailsResponse(BusinessProfile business)
    {
        return new BusinessDetailsResponse
        {
            BusinessId = business.Id,
            Name = business.Name,
            RegistrationNumber = business.RegistrationNumber,
            VatNumber = business.VatNumber,
            SupportEmail = business.SupportEmail?.Email,
            ContactNumber = business.ContactNumber is { } contact
                ? new ContactNumberModel { Number = contact.Number, Type = contact.ContactNumberType }
                : null,
            Address = business.Address is { } address
                ? new AddressModel
                {
                    Line1 = address.Line1,
                    Line2 = address.Line2,
                    Suburb = address.Suburb,
                    City = address.City,
                    PostalCode = address.PostalCode
                }
                : null,
            BankDetails = business.BankDetails is { } bank
                ? new BankDetailsModel
                {
                    BankName = bank.BankName,
                    AccountHolder = bank.AccountHolder,
                    AccountNumber = bank.AccountNumber,
                    AccountType = bank.AccountType,
                    BranchCode = bank.BranchCode,
                    BranchName = bank.BranchName
                }
                : null,
            HasLogo = business.LogoBlobPath is not null,
            Version = Convert.ToBase64String(business.RowVersion)
        };
    }

    private static BusinessSettingsResponse ToSettingsResponse(BusinessProfile business)
    {
        return new BusinessSettingsResponse
        {
            DefaultPaymentTermsDays = business.DefaultPaymentTermsDays,
            TimeZone = business.TimeZone,
            Version = Convert.ToBase64String(business.RowVersion)
        };
    }
}
