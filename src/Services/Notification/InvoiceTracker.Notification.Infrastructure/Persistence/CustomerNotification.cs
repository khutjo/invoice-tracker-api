namespace InvoiceTracker.Notification.Infrastructure.Persistence;

/// <summary>
/// One simulated message to a customer. Named CustomerNotification because
/// "Notification" clashes with the InvoiceTracker.Notification namespace.
/// </summary>
public sealed class CustomerNotification
{
    public Guid Id { get; private set; }
    public Guid BusinessId { get; private set; }
    public Guid InvoiceId { get; private set; }
    public NotificationType NotificationType { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public string Recipient { get; private set; } = string.Empty;

    /// <summary>The rendered message, shown in the UI.</summary>
    public string Body { get; private set; } = string.Empty;

    public NotificationStatus Status { get; private set; } = NotificationStatus.Pending;
    public int Attempts { get; private set; }
    public DateTime? NextAttemptAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SentAt { get; private set; }
}
