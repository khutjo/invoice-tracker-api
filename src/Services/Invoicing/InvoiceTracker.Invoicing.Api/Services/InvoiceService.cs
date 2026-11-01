using InvoiceTracker.Invoicing.Api.Exceptions;
using InvoiceTracker.Invoicing.Api.Interfaces;
using InvoiceTracker.Invoicing.Api.Models.Request;
using InvoiceTracker.Invoicing.Api.Models.Response;
using InvoiceTracker.Invoicing.Application.Customers;
using InvoiceTracker.Invoicing.Application.Invoices;
using InvoiceTracker.Invoicing.Application.Time;
using InvoiceTracker.Invoicing.Domain.Invoices;
using InvoiceTracker.Invoicing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Invoicing.Api.Services;

public sealed class InvoiceService(
    InvoicingDbContext dbContext,
    IInvoiceNumberAllocator invoiceNumberAllocator,
    ICustomerDirectory customerDirectory,
    BusinessCalendar businessCalendar,
    TimeProvider timeProvider) : IInvoiceService
{
    private const int RowVersionLength = 8;

    public async Task<PagedResponse<InvoiceSummaryResponse>> GetInvoicesListAsync(InvoiceSearchQuery query, Guid businessId, CancellationToken cancellationToken)
    {
        if (query.IssuedFrom is { } issuedFrom && query.IssuedTo is { } issuedTo && issuedFrom > issuedTo)
        {
            throw InvoiceException.Invalid("issuedTo", "Issued to must be on or after issued from.");
        }

        var invoices = dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.BusinessId == businessId);

        if (query.Status is { } status)
        {
            invoices = invoices.Where(i => i.Status == status);
        }

        if (query.OverdueOnly)
        {
            invoices = invoices.Where(i => i.Status == InvoiceStatus.Overdue);
        }

        if (query.CustomerId is { } customerId)
        {
            invoices = invoices.Where(i => i.CustomerId == customerId);
        }

        if (query.IssuedFrom is { } from)
        {
            invoices = invoices.Where(i => i.IssueDate >= from);
        }

        if (query.IssuedTo is { } to)
        {
            invoices = invoices.Where(i => i.IssueDate <= to);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            invoices = invoices.Where(i => i.InvoiceNumber != null && i.InvoiceNumber.Contains(term));
        }

        var totalCount = await invoices.CountAsync(cancellationToken);

        var items = await invoices
            .OrderByDescending(i => i.CreatedAt)
            .ThenByDescending(i => i.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(i => new InvoiceSummaryResponse
            {
                InvoiceId = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                CustomerId = i.CustomerId,
                CustomerName = i.BilledCustomer != null ? i.BilledCustomer.Name : null,
                Status = i.Status,
                IssueDate = i.IssueDate,
                DueDate = i.DueDate,
                Total = i.Total,
                AmountPaid = i.AmountPaid,
                Balance = i.Balance,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<InvoiceSummaryResponse>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<InvoiceDetailsResponse> GetInvoiceAsync(Guid invoiceId, Guid businessId, CancellationToken cancellationToken)
    {
        var invoice = await WithDetails()
            .AsNoTracking()
            .SingleOrDefaultAsync(i => i.Id == invoiceId && i.BusinessId == businessId, cancellationToken)
            ?? throw InvoiceException.NotFound();

        return ToDetailsResponse(invoice);
    }

    public async Task<InvoiceDetailsResponse> CreateInvoiceAsync(InvoiceDetailsRequest request, Guid businessId, Guid userId, CancellationToken cancellationToken)
    {
        ValidateDetails(request);
        await EnsureCustomerCanBeInvoicedAsync(request.CustomerId!.Value, cancellationToken);

        var invoice = Invoice.CreateDraft(businessId, request.CustomerId!.Value, request.PaymentTermsDays, ToLineDetails(request), userId, Now());
        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDetailsResponse(invoice);
    }

    public async Task<InvoiceDetailsResponse> UpdateInvoiceAsync(Guid invoiceId, UpdateInvoiceRequest request, Guid businessId, Guid userId, CancellationToken cancellationToken)
    {
        var invoice = await WithDetails()
            .SingleOrDefaultAsync(i => i.Id == invoiceId && i.BusinessId == businessId, cancellationToken)
            ?? throw InvoiceException.NotFound();

        ValidateDetails(request);
        ExpectVersion(invoice, request.Version);
        await EnsureCustomerCanBeInvoicedAsync(request.CustomerId!.Value, cancellationToken);
        invoice.UpdateDraft(request.CustomerId!.Value, request.PaymentTermsDays, ToLineDetails(request), userId, Now());
        await SaveAsync(cancellationToken);

        return ToDetailsResponse(invoice);
    }

    public async Task DeleteInvoiceAsync(Guid invoiceId, Guid businessId, CancellationToken cancellationToken)
    {
        var invoice = await dbContext.Invoices
            .SingleOrDefaultAsync(i => i.Id == invoiceId && i.BusinessId == businessId, cancellationToken)
            ?? throw InvoiceException.NotFound();

        invoice.EnsureCanDelete();
        dbContext.Invoices.Remove(invoice);
        await SaveAsync(cancellationToken);
    }

    public async Task<InvoiceDetailsResponse> SendInvoiceAsync(Guid invoiceId, Guid businessId, Guid userId, CancellationToken cancellationToken)
    {
        var customerId = await dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.Id == invoiceId && i.BusinessId == businessId)
            .Select(i => (Guid?)i.CustomerId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw InvoiceException.NotFound();

        // Look the customer up before the transaction, so the number sequence is never locked during an HTTP call
        var customer = await customerDirectory.GetAsync(customerId, cancellationToken);
        if (customer is null || customer.IsArchived)
        {
            throw InvoiceException.CustomerNotInvoiceable();
        }

        var billedCustomer = ToBilledCustomer(customer);
        var lookedUpCustomerId = customer.CustomerId;

        // An explicit transaction under EnableRetryOnFailure has to run inside the execution strategy,
        // and a retry starts from a clean change tracker so it never reuses half-applied state
        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async token =>
        {
            dbContext.ChangeTracker.Clear();
            await using var transaction = await dbContext.Database.BeginTransactionAsync(token);

            var invoice = await WithDetails()
                .SingleOrDefaultAsync(i => i.Id == invoiceId && i.BusinessId == businessId, token)
                ?? throw InvoiceException.NotFound();

            // The draft's customer was changed between the lookup and now
            if (invoice.CustomerId != lookedUpCustomerId)
            {
                throw InvoiceException.StaleVersion();
            }

            var issueDate = businessCalendar.Today();

            // If Send refuses (not a draft, no lines) nothing is saved, so the number is never used up
            var invoiceNumber = await invoiceNumberAllocator.NextAsync(businessId, issueDate.Year, token);
            invoice.Send(invoiceNumber, issueDate, billedCustomer, userId, Now());

            await SaveAsync(token);
            await transaction.CommitAsync(token);

            return ToDetailsResponse(invoice);
        }, cancellationToken);
    }

    /// <summary>Business owns customers: it answers 404 for unknown ids and for other businesses' customers.</summary>
    private async Task EnsureCustomerCanBeInvoicedAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await customerDirectory.GetAsync(customerId, cancellationToken);
        if (customer is null)
        {
            throw InvoiceException.Invalid("customerId", "The customer was not found.");
        }

        if (customer.IsArchived)
        {
            throw InvoiceException.Invalid("customerId", "Archived customers cannot be invoiced.");
        }
    }

    private static BilledCustomer ToBilledCustomer(CustomerInfo customer)
    {
        var address = customer.BillingAddress;
        return BilledCustomer.Create(
            customer.Name,
            customer.Email,
            customer.VatNumber,
            address?.Line1,
            address?.Line2,
            address?.Suburb,
            address?.City,
            address?.PostalCode);
    }

    private IQueryable<Invoice> WithDetails()
    {
        return dbContext.Invoices
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .Include(i => i.Events)
            .AsSplitQuery();
    }

    private static void ValidateDetails(InvoiceDetailsRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.CustomerId is null || request.CustomerId == Guid.Empty)
        {
            errors["customerId"] = ["Choose a customer."];
        }

        for (var index = 0; index < request.Lines.Count; index++)
        {
            var line = request.Lines[index];

            if (decimal.Round(line.Quantity, 3) != line.Quantity)
            {
                errors[$"lines[{index}].quantity"] = ["Quantity has at most 3 decimals."];
            }

            if (decimal.Round(line.UnitPrice, 2) != line.UnitPrice)
            {
                errors[$"lines[{index}].unitPrice"] = ["Unit price has at most 2 decimals."];
            }

            if (decimal.Round(line.VatRate, 4) != line.VatRate)
            {
                errors[$"lines[{index}].vatRate"] = ["VAT rate has at most 4 decimals, for example 0.15."];
            }
        }

        if (errors.Count > 0)
        {
            throw InvoiceException.Invalid(errors);
        }
    }

    private static IEnumerable<InvoiceLineDetails> ToLineDetails(InvoiceDetailsRequest request) => request.Lines.Select(l => new InvoiceLineDetails(l.Description, l.Quantity, l.UnitPrice, l.VatRate));

    private void ExpectVersion(Invoice invoice, string version)
    {
        var expected = new byte[RowVersionLength];
        if (!Convert.TryFromBase64String(version, expected, out var written) || written != RowVersionLength)
        {
            throw InvoiceException.Invalid("version", "Version is not valid. Send the version from the last read.");
        }

        dbContext.Entry(invoice).Property(i => i.RowVersion).OriginalValue = expected;
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw InvoiceException.StaleVersion();
        }
    }

    private DateTime Now()
    {
        return timeProvider.GetUtcNow().UtcDateTime;
    }

    private static InvoiceDetailsResponse ToDetailsResponse(Invoice invoice)
    {
        return new InvoiceDetailsResponse
        {
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            CustomerId = invoice.CustomerId,
            BilledCustomer = invoice.BilledCustomer is { } billed
                ? new BilledCustomerResponse
                {
                    Name = billed.Name,
                    Email = billed.Email,
                    VatNumber = billed.VatNumber,
                    AddressLine1 = billed.AddressLine1,
                    AddressLine2 = billed.AddressLine2,
                    Suburb = billed.Suburb,
                    City = billed.City,
                    PostalCode = billed.PostalCode
                }
                : null,
            Status = invoice.Status,
            PaymentTermsDays = invoice.PaymentTermsDays,
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            Subtotal = invoice.Subtotal,
            VatTotal = invoice.VatTotal,
            Total = invoice.Total,
            AmountPaid = invoice.AmountPaid,
            Balance = invoice.Balance,
            Lines = invoice.Lines
                .OrderBy(l => l.LineNumber)
                .Select(l => new InvoiceLineResponse
                {
                    InvoiceLineId = l.Id,
                    LineNumber = l.LineNumber,
                    Description = l.Description,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    VatRate = l.VatRate,
                    LineTotal = l.LineTotal,
                    VatAmount = l.VatAmount
                })
                .ToList(),
            Payments = invoice.Payments
                .OrderBy(p => p.PaidOn)
                .ThenBy(p => p.RecordedAt)
                .Select(p => new PaymentResponse
                {
                    PaymentId = p.Id,
                    Amount = p.Amount,
                    PaidOn = p.PaidOn,
                    Method = p.Method,
                    Reference = p.Reference,
                    RecordedAt = p.RecordedAt
                })
                .ToList(),
            Timeline = invoice.Events
                .OrderBy(e => e.OccurredAt)
                .Select(e => new InvoiceEventResponse
                {
                    EventType = e.EventType,
                    OccurredAt = e.OccurredAt,
                    UserId = e.UserId,
                    Detail = e.Detail
                })
                .ToList(),
            CreatedAt = invoice.CreatedAt,
            ModifiedAt = invoice.ModifiedAt,
            Version = Convert.ToBase64String(invoice.RowVersion)
        };
    }
}
