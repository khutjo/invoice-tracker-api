using InvoiceTracker.BuildingBlocks.ServiceDefaults.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace InvoiceTracker.Notification.Api.Hubs;

/// <summary>
/// Live feed for the portal. Server-to-client only: each connection joins its business's group,
/// and the dispatcher pushes <c>notificationSent</c> to that group. The REST log stays the source of truth.
/// </summary>
[Authorize]
public sealed class NotificationsHub(ILogger<NotificationsHub> logger) : Hub
{
    public const string Path = "/hubs/notifications";
    public const string NotificationSentMethod = "notificationSent";

    public static string GroupFor(Guid businessId)
    {
        return $"business:{businessId}";
    }

    public override async Task OnConnectedAsync()
    {
        // The business comes from the validated token only, so a user can never join another tenant's group
        var businessId = Context.User!.GetBusinessId();
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupFor(businessId));

        logger.LogInformation("Portal connection {ConnectionId} joined notifications for business {BusinessId}",
            Context.ConnectionId, businessId);

        await base.OnConnectedAsync();
    }
}
