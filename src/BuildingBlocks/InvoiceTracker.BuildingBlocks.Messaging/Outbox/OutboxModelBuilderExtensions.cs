using InvoiceTracker.BuildingBlocks.Contracts;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTracker.BuildingBlocks.Messaging.Outbox;

public static class OutboxModelBuilderExtensions
{
    public static ModelBuilder AddOutbox(this ModelBuilder builder)
    {
        builder.Entity<OutboxMessage>(message =>
        {
            message.ToTable("outbox_messages");

            message.HasKey(m => m.Id).HasName("pk_outbox_messages");
            message.Property(m => m.Id).HasColumnName("outbox_message_id").ValueGeneratedNever();
            message.Property(m => m.Type).HasColumnName("type").HasMaxLength(200);
            message.Property(m => m.Payload).HasColumnName("payload");
            message.Property(m => m.OccurredAt).HasColumnName("occurred_at");
            message.Property(m => m.ProcessedAt).HasColumnName("processed_at");
            message.Property(m => m.Attempts).HasColumnName("attempts");
            message.Property(m => m.Error).HasColumnName("error").HasMaxLength(2000);
            message.HasIndex(m => m.OccurredAt)
                .HasDatabaseName("ix_outbox_messages_unprocessed")
                .HasFilter("[processed_at] IS NULL");
        });

        return builder;
    }

    public static void AddOutboxMessage(this DbContext dbContext, IntegrationEvent integrationEvent)
    {
        dbContext.Set<OutboxMessage>().Add(OutboxMessage.From(integrationEvent));
    }
}
