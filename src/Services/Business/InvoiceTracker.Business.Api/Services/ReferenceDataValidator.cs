using InvoiceTracker.Business.Api.Models.Common;
using InvoiceTracker.Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Business.Api.Services;

/// <summary>
/// Checks addresses and bank details against the seeded reference data (postal_codes, banks).
/// DataAnnotations only check the shape (4 digits, 6 digits); this checks the value exists.
/// Reusable for customers, which have addresses too.
/// </summary>
public sealed class ReferenceDataValidator(BusinessDbContext dbContext)
{
    public async Task ValidateAddressAsync(AddressModel? address, string field, IDictionary<string, string[]> errors, CancellationToken cancellationToken)
    {
        if (address is null)
        {
            return;
        }

        var postalCode = address.PostalCode.Trim();
        if (!await dbContext.PostalCodes.AnyAsync(p => p.Code == postalCode, cancellationToken))
        {
            errors[$"{field}.postalCode"] = [$"{postalCode} is not a South African postal code."];
        }
    }

    public async Task ValidateBankDetailsAsync(BankDetailsModel? bankDetails, string field, IDictionary<string, string[]> errors, CancellationToken cancellationToken)
    {
        if (bankDetails is null)
        {
            return;
        }

        // The database collation is case-insensitive, so "fnb" matches "FNB"
        var bankName = bankDetails.BankName.Trim();
        var bank = await dbContext.Banks
            .AsNoTracking()
            .SingleOrDefaultAsync(b => b.Name == bankName, cancellationToken);

        if (bank is null)
        {
            errors[$"{field}.bankName"] = ["Choose a bank from the supported list."];
            return;
        }

        if (bankDetails.BranchCode.Trim() != bank.UniversalBranchCode)
        {
            errors[$"{field}.branchCode"] = [$"Use {bank.Name}'s universal branch code {bank.UniversalBranchCode}."];
        }
    }
}
