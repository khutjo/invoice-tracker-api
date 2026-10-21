using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceTracker.Notification.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "notifications",
            columns: table => new
            {
                notification_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                business_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                invoice_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                notification_type = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                channel = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                recipient = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                attempts = table.Column<int>(type: "int", nullable: false),
                next_attempt_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                sent_at = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_notifications", x => x.notification_id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_notifications_business_id_created_at",
            table: "notifications",
            columns: new[] { "business_id", "created_at" });

        migrationBuilder.CreateIndex(
            name: "ix_notifications_status_next_attempt_at",
            table: "notifications",
            columns: new[] { "status", "next_attempt_at" });

        migrationBuilder.CreateIndex(
            name: "uq_notifications_invoice_id_notification_type",
            table: "notifications",
            columns: new[] { "invoice_id", "notification_type" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "notifications");
    }
}
