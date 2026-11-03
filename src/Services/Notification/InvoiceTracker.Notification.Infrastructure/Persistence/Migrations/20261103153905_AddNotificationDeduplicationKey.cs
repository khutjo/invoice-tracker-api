using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceTracker.Notification.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationDeduplicationKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "uq_notifications_invoice_id_notification_type",
                table: "notifications");

            migrationBuilder.AddColumn<string>(
                name: "deduplication_key",
                table: "notifications",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "uq_notifications_invoice_id_notification_type_deduplication_key",
                table: "notifications",
                columns: new[] { "invoice_id", "notification_type", "deduplication_key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "uq_notifications_invoice_id_notification_type_deduplication_key",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "deduplication_key",
                table: "notifications");

            migrationBuilder.CreateIndex(
                name: "uq_notifications_invoice_id_notification_type",
                table: "notifications",
                columns: new[] { "invoice_id", "notification_type" },
                unique: true);
        }
    }
}
