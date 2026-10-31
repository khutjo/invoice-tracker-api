using InvoiceTracker.Business.Api.Exceptions;
using InvoiceTracker.Business.Api.Interfaces;
using InvoiceTracker.Business.Api.Lookups;
using InvoiceTracker.Business.Api.Models.Response;
using InvoiceTracker.Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Business.Api.Services;

public sealed class LookupService(BusinessDbContext dbContext) : ILookupService
{
    public async Task<LookupsResponse> GetLookupsAsync(CancellationToken cancellationToken)
    {
        var banks = await dbContext.Banks
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new BankOption(b.Name, b.Name, b.UniversalBranchCode))
            .ToListAsync(cancellationToken);

        return new LookupsResponse
        {
            ContactNumberTypes = [.. LookupCatalog.ContactNumberTypes.Select(t => new LookupOption(t.Value.ToString(), t.Label))],
            BankAccountTypes = [.. LookupCatalog.BankAccountTypes.Select(t => new LookupOption(t.Value.ToString(), t.Label))],
            Banks = banks,
            TimeZones = [.. LookupCatalog.TimeZones.Select(t => new LookupOption(t.Value, t.Label))],
            DefaultTimeZone = LookupCatalog.DefaultTimeZone,
            PaymentTerms = [.. LookupCatalog.PaymentTerms.Select(t => new PaymentTermOption(t.Days, t.Label))],
            DefaultPaymentTermsDays = LookupCatalog.DefaultPaymentTermsDays
        };
    }

    public async Task<IReadOnlyList<PostalCodeOption>> FindPostalCodeAsync(string code, CancellationToken cancellationToken)
    {
        var options = await dbContext.PostalCodes
            .AsNoTracking()
            .Where(p => p.Code == code)
            .OrderBy(p => p.Suburb)
            .Select(p => new PostalCodeOption(p.Code, p.Suburb, p.Town, p.Kind.ToString()))
            .ToListAsync(cancellationToken);

        return options.Count > 0
            ? options
            : throw new BusinessException($"{code} is not a South African postal code.", StatusCodes.Status404NotFound);
    }
}
