using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceTracker.Invoicing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBilledCustomerToInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "billed_customer_address_line1",
                table: "invoices",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "billed_customer_address_line2",
                table: "invoices",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "billed_customer_city",
                table: "invoices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "billed_customer_email",
                table: "invoices",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "billed_customer_name",
                table: "invoices",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "billed_customer_postal_code",
                table: "invoices",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "billed_customer_suburb",
                table: "invoices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "billed_customer_vat_number",
                table: "invoices",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "billed_customer_address_line1",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "billed_customer_address_line2",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "billed_customer_city",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "billed_customer_email",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "billed_customer_name",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "billed_customer_postal_code",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "billed_customer_suburb",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "billed_customer_vat_number",
                table: "invoices");
        }
    }
}
