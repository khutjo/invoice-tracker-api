using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceTracker.Invoicing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOverdueRemindersAndWebhookPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reminder_stage",
                table: "invoices",
                type: "int",
                maxLength: 32,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "uq_payments_external_id",
                table: "payments",
                column: "external_id",
                unique: true,
                filter: "[external_id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "uq_payments_external_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "reminder_stage",
                table: "invoices");
        }
    }
}
