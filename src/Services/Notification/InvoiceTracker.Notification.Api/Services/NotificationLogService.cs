using InvoiceTracker.Notification.Api.Interfaces;
using InvoiceTracker.Notification.Api.Models.Request;
using InvoiceTracker.Notification.Api.Models.Response;
using InvoiceTracker.Notification.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Notification.Api.Services;

public sealed class NotificationLogService(NotificationDbContext dbContext) : INotificationLogService
{
    public async Task<PagedResponse<NotificationLogItemResponse>> SearchAsync(
        NotificationSearchQuery query,
        Guid businessId,
        CancellationToken cancellationToken)
    {
        // Tenant first: the business comes from the token, so other businesses' rows are never visible
        var notifications = dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.BusinessId == businessId);

        if (query.Type is { } type)
        {
            notifications = notifications.Where(n => n.NotificationType == type);
        }

        if (query.Status is { } status)
        {
            notifications = notifications.Where(n => n.Status == status);
        }

        if (query.InvoiceId is { } invoiceId)
        {
            notifications = notifications.Where(n => n.InvoiceId == invoiceId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            notifications = notifications.Where(n =>
                n.InvoiceNumber.Contains(search) || n.CustomerName.Contains(search) || n.Recipient.Contains(search));
        }

        var totalCount = await notifications.CountAsync(cancellationToken);

        // Served by ix_notifications_business_id_created_at
        var items = await notifications
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(n => new NotificationLogItemResponse
            {
                NotificationId = n.Id,
                InvoiceId = n.InvoiceId,
                InvoiceNumber = n.InvoiceNumber,
                CustomerName = n.CustomerName,
                NotificationType = n.NotificationType,
                Channel = n.Channel,
                Recipient = n.Recipient,
                Status = n.Status,
                Attempts = n.Attempts,
                Body = n.Body,
                CreatedAt = DateTime.SpecifyKind(n.CreatedAt, DateTimeKind.Utc),
                SentAt = n.SentAt.HasValue ? DateTime.SpecifyKind(n.SentAt.Value, DateTimeKind.Utc) : null
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<NotificationLogItemResponse>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }
}
