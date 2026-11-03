namespace InvoiceTracker.Notification.Infrastructure.Persistence;

/// <summary>
/// One simulated message to a customer. Named CustomerNotification because
/// "Notification" clashes with the InvoiceTracker.Notification namespace.
/// </summary>
public sealed class CustomerNotification
{
    /// <summary>After this many failed deliveries the notification is marked Failed and no longer retried.</summary>
    public const int MaxAttempts = 5;

    private static readonly TimeSpan _firstRetryDelay = TimeSpan.FromSeconds(30);

    private CustomerNotification()
    {
    }

    public Guid Id { get; private set; }
    public Guid BusinessId { get; private set; }
    public Guid InvoiceId { get; private set; }

    /// <summary>Frozen from the event so the log and the portal push can show it without calling Invoicing.</summary>
    public string InvoiceNumber { get; private set; } = string.Empty;

    public string CustomerName { get; private set; } = string.Empty;
    public NotificationType NotificationType { get; private set; }

    /// <summary>
    /// Tells apart notifications of the same type on one invoice: the payment id for PaymentReceived,
    /// so each partial payment gets its own message. Empty for one-per-invoice types such as reminders.
    /// </summary>
    public string DeduplicationKey { get; private set; } = string.Empty;

    public NotificationChannel Channel { get; private set; }
    public string Recipient { get; private set; } = string.Empty;

    /// <summary>The rendered message, shown in the UI.</summary>
    public string Body { get; private set; } = string.Empty;

    public NotificationStatus Status { get; private set; } = NotificationStatus.Pending;
    public int Attempts { get; private set; }
    public DateTime? NextAttemptAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SentAt { get; private set; }

    /// <summary>A new Pending notification, due for its first delivery attempt straight away.</summary>
    public static CustomerNotification CreatePending(
        Guid businessId,
        Guid invoiceId,
        string invoiceNumber,
        string customerName,
        NotificationType notificationType,
        NotificationChannel channel,
        string recipient,
        string body,
        DateTime createdAtUtc,
        string? deduplicationKey = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(customerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        return new CustomerNotification
        {
            Id = Guid.CreateVersion7(),
            BusinessId = businessId,
            InvoiceId = invoiceId,
            InvoiceNumber = invoiceNumber,
            CustomerName = customerName,
            NotificationType = notificationType,
            DeduplicationKey = deduplicationKey ?? string.Empty,
            Channel = channel,
            Recipient = recipient,
            Body = body,
            Status = NotificationStatus.Pending,
            Attempts = 0,
            NextAttemptAt = createdAtUtc,
            CreatedAt = createdAtUtc
        };
    }

    public void MarkSent(DateTime sentAtUtc)
    {
        EnsurePending();

        Attempts++;
        Status = NotificationStatus.Sent;
        SentAt = sentAtUtc;
        NextAttemptAt = null;
    }

    /// <summary>
    /// Records a failed delivery. Retries back off exponentially (30 s, 1 min, 2 min, 4 min);
    /// the fifth failure marks it Failed for good.
    /// </summary>
    public void RecordFailedAttempt(DateTime failedAtUtc)
    {
        EnsurePending();

        Attempts++;
        if (Attempts >= MaxAttempts)
        {
            Status = NotificationStatus.Failed;
            NextAttemptAt = null;
            return;
        }

        NextAttemptAt = failedAtUtc + (_firstRetryDelay * Math.Pow(2, Attempts - 1));
    }

    private void EnsurePending()
    {
        if (Status != NotificationStatus.Pending)
        {
            throw new InvalidOperationException($"Notification {Id} is {Status}; only Pending notifications can be delivered.");
        }
    }
}
