using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.Notification.Infrastructure.Persistence;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<CustomerNotification> Notifications => Set<CustomerNotification>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(32);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerNotification>(notification =>
        {
            notification.ToTable("notifications");
            notification.HasKey(n => n.Id).HasName("pk_notifications");
            notification.Property(n => n.Id).HasColumnName("notification_id");

            notification.Property(n => n.Recipient).HasMaxLength(256);

            // A reminder stage is created once per invoice, however often the event is redelivered
            notification.HasIndex(n => new { n.InvoiceId, n.NotificationType })
                .IsUnique()
                .HasDatabaseName("uq_notifications_invoice_id_notification_type");

            // The dispatcher claims Pending rows in batches
            notification.HasIndex(n => new { n.Status, n.NextAttemptAt })
                .HasDatabaseName("ix_notifications_status_next_attempt_at");

            notification.HasIndex(n => new { n.BusinessId, n.CreatedAt })
                .HasDatabaseName("ix_notifications_business_id_created_at");
        });
    }
}
