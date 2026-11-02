using InvoiceTracker.BuildingBlocks.ServiceDefaults.Authentication;
using InvoiceTracker.Notification.Api.Interfaces;
using InvoiceTracker.Notification.Api.Models.Request;
using InvoiceTracker.Notification.Api.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceTracker.Notification.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/notifications")]
public sealed class NotificationsController(INotificationLogService notificationLogService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<NotificationLogItemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchNotificationsAsync([FromQuery] NotificationSearchQuery query, CancellationToken cancellationToken)
    {
        var response = await notificationLogService.SearchAsync(query, User.GetBusinessId(), cancellationToken);
        return Ok(response);
    }
}
