using InvoiceTracker.BuildingBlocks.Contracts.Events.IdentityModels;
using InvoiceTracker.BuildingBlocks.Messaging.Abstractions;
using InvoiceTracker.Business.Domain.Businesses;
using InvoiceTracker.Business.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Business.Api.Consumers;


public sealed class BusinessRegisteredHandler(
    BusinessDbContext dbContext,
    ILogger<BusinessRegisteredHandler> logger) : IIntegrationEventHandler<BusinessRegistered>
{
    public async Task HandleAsync(BusinessRegistered integrationEvent, CancellationToken cancellationToken)
    {
        if (await dbContext.Businesses.AnyAsync(b => b.Id == integrationEvent.BusinessId, cancellationToken))
        {
            logger.LogInformation("Business {BusinessId} already exists; skipping duplicate {EventId}",
                integrationEvent.BusinessId, integrationEvent.EventId);
            return;
        }

        var business = BusinessProfile.Create(
            integrationEvent.BusinessId,
            integrationEvent.BusinessName,
            integrationEvent.OwnerUserId,
            integrationEvent.OccurredAt.UtcDateTime);

        dbContext.Businesses.Add(business);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // Another delivery of the same event got past the AnyAsync check first; the primary key let only one in
            logger.LogInformation("Business {BusinessId} was created by a concurrent delivery; skipping {EventId}",
                integrationEvent.BusinessId, integrationEvent.EventId);
            return;
        }

        logger.LogInformation("Created business profile {BusinessId} ({BusinessName}) for owner {OwnerUserId}",
            business.Id, business.Name, integrationEvent.OwnerUserId);
    }
}
