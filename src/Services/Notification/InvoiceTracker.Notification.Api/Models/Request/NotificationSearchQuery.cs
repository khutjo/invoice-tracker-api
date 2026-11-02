using System.ComponentModel.DataAnnotations;
using InvoiceTracker.Notification.Infrastructure.Persistence;

namespace InvoiceTracker.Notification.Api.Models.Request;

public sealed class NotificationSearchQuery
{
    /// <summary>Matches part of the invoice number, customer name or recipient.</summary>
    [StringLength(100)]
    public string? Search
    {
        get; init;
    }

    [Range(1, int.MaxValue)]
    public int Page
    {
        get; init;
    } = 1;

    [Range(1, 100)]
    public int PageSize
    {
        get; init;
    } = 20;

    public NotificationType? Type
    {
        get; init;
    }

    public NotificationStatus? Status
    {
        get; init;
    }

    /// <summary>Only this invoice's notifications (the invoice detail page).</summary>
    public Guid? InvoiceId
    {
        get; init;
    }
}
