namespace InvoiceTracker.Notification.Infrastructure.Persistence;

public enum NotificationType
{
    InvoiceSent,
    PaymentReceived,
    OverdueReminder1,
    OverdueReminder2,
    OverdueFinal,
    InvoiceCancelled
}

public enum NotificationChannel
{
    Email,
    Sms
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Failed
}
