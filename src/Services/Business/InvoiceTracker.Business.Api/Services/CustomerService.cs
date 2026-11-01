using InvoiceTracker.Business.Api.Exceptions;
using InvoiceTracker.Business.Api.Interfaces;
using InvoiceTracker.Business.Api.Models.Common;
using InvoiceTracker.Business.Api.Models.Request;
using InvoiceTracker.Business.Api.Models.Response;
using InvoiceTracker.Business.Domain.Customers;
using InvoiceTracker.Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Business.Api.Services;

public sealed class CustomerService(
    BusinessDbContext dbContext,
    ReferenceDataValidator referenceDataValidator,
    TimeProvider timeProvider) : ICustomerService
{
    private const int RowVersionLength = 8;

    public async Task<PagedResponse<CustomerSummaryResponse>> SearchCustomersAsync(CustomerSearchQuery query, Guid businessId, CancellationToken cancellationToken)
    {
        var customers = dbContext.Customers
            .AsNoTracking()
            .Where(c => c.BusinessId == businessId);

        if (!query.IncludeArchived)
        {
            customers = customers.Where(c => !c.IsArchived);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            customers = customers.Where(c =>
                c.Name.Contains(term)
                || c.EmailAddress.Email.Contains(term)
                || (c.CustomerReference != null && c.CustomerReference.Contains(term)));
        }

        var totalCount = await customers.CountAsync(cancellationToken);

        var items = await customers
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new CustomerSummaryResponse
            {
                CustomerId = c.Id,
                Name = c.Name,
                Email = c.EmailAddress.Email,
                PhoneNumber = c.ContactNumber != null ? c.ContactNumber.Number : null,
                City = c.BillingAddress != null ? c.BillingAddress.City : null,
                CustomerReference = c.CustomerReference,
                IsArchived = c.IsArchived
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<CustomerSummaryResponse>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<CustomerDetailsResponse> GetCustomerAsync(Guid customerId, Guid businessId, CancellationToken cancellationToken)
    {
        var customer = await WithDetails()
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == customerId && c.BusinessId == businessId, cancellationToken)
            ?? throw BusinessException.CustomerNotFound();

        return ToDetailsResponse(customer);
    }

    public async Task<CustomerDetailsResponse> CreateCustomerAsync(CustomerDetailsRequest request, Guid businessId, Guid userId, CancellationToken cancellationToken)
    {
        // customers.business_id references businesses, so a missing profile would otherwise be a 500 from the FK
        if (!await dbContext.Businesses.AnyAsync(b => b.Id == businessId, cancellationToken))
        {
            throw BusinessException.NotFound();
        }

        await ValidateReferenceDataAsync(request, cancellationToken);

        var now = Now();
        var customer = Customer.Create(businessId, request.Name, request.Email, userId, now);
        ApplyDetails(customer, request, userId, now);
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDetailsResponse(customer);
    }

    public async Task<CustomerDetailsResponse> UpdateCustomerAsync(Guid customerId, UpdateCustomerRequest request, Guid businessId, Guid userId, CancellationToken cancellationToken)
    {
        var customer = await WithDetails()
            .SingleOrDefaultAsync(c => c.Id == customerId && c.BusinessId == businessId, cancellationToken)
            ?? throw BusinessException.CustomerNotFound();

        if (customer.IsArchived)
        {
            throw BusinessException.CustomerArchived();
        }

        await ValidateReferenceDataAsync(request, cancellationToken);
        ExpectVersion(customer, request.Version);
        ApplyDetails(customer, request, userId, Now());
        await SaveAsync(cancellationToken);

        return ToDetailsResponse(customer);
    }

    public async Task ArchiveCustomerAsync(Guid customerId, Guid businessId, Guid userId, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers
            .SingleOrDefaultAsync(c => c.Id == customerId && c.BusinessId == businessId, cancellationToken)
            ?? throw BusinessException.CustomerNotFound();

        customer.Archive(userId, Now());
        await SaveAsync(cancellationToken);
    }

    private IQueryable<Customer> WithDetails()
    {
        return dbContext.Customers
            .Include(c => c.EmailAddress)
            .Include(c => c.ContactNumber)
            .Include(c => c.BillingAddress);
    }

    private async Task ValidateReferenceDataAsync(CustomerDetailsRequest request, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        await referenceDataValidator.ValidateAddressAsync(request.BillingAddress, "billingAddress", errors, cancellationToken);

        if (errors.Count > 0)
        {
            throw BusinessException.Invalid(errors);
        }
    }

    private void ApplyDetails(Customer customer, CustomerDetailsRequest request, Guid userId, DateTime now)
    {
        customer.UpdateDetails(request.Name, request.CustomerReference, request.VatNumber, userId, now);
        customer.SetEmail(request.Email, userId, now);

        if (request.ContactNumber is { } contact)
        {
            customer.SetContactNumber(contact.Number, contact.Type, userId, now);
        }
        else
        {
            RemoveDetail(customer.ClearContactNumber(userId, now));
        }

        if (request.BillingAddress is { } address)
        {
            customer.SetBillingAddress(address.Line1, address.Line2, address.Suburb, address.City, address.PostalCode, userId, now);
        }
        else
        {
            RemoveDetail(customer.ClearBillingAddress(userId, now));
        }
    }

    private void RemoveDetail(object? detail)
    {
        if (detail is not null)
        {
            dbContext.Remove(detail);
        }
    }

    private void ExpectVersion(Customer customer, string version)
    {
        var expected = new byte[RowVersionLength];
        if (!Convert.TryFromBase64String(version, expected, out var written) || written != RowVersionLength)
        {
            throw BusinessException.Invalid("version", "Version is not valid. Send the version from the last read.");
        }

        dbContext.Entry(customer).Property(c => c.RowVersion).OriginalValue = expected;
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw BusinessException.CustomerStaleVersion();
        }
    }

    private DateTime Now()
    {
        return timeProvider.GetUtcNow().UtcDateTime;
    }

    private static CustomerDetailsResponse ToDetailsResponse(Customer customer)
    {
        return new CustomerDetailsResponse
        {
            CustomerId = customer.Id,
            Name = customer.Name,
            Email = customer.EmailAddress.Email,
            ContactNumber = customer.ContactNumber is { } contact
                ? new ContactNumberModel { Number = contact.Number, Type = contact.ContactNumberType }
                : null,
            BillingAddress = customer.BillingAddress is { } address
                ? new AddressModel
                {
                    Line1 = address.Line1,
                    Line2 = address.Line2,
                    Suburb = address.Suburb,
                    City = address.City,
                    PostalCode = address.PostalCode
                }
                : null,
            VatNumber = customer.VatNumber,
            CustomerReference = customer.CustomerReference,
            IsArchived = customer.IsArchived,
            Version = Convert.ToBase64String(customer.RowVersion)
        };
    }
}
