using InvoiceTracker.Notification.Api.Models.Request;
using InvoiceTracker.Notification.Api.Models.Response;

namespace InvoiceTracker.Notification.Api.Interfaces;

public interface INotificationLogService
{
    Task<PagedResponse<NotificationLogItemResponse>> SearchAsync(
        NotificationSearchQuery query,
        Guid businessId,
        CancellationToken cancellationToken);
}
