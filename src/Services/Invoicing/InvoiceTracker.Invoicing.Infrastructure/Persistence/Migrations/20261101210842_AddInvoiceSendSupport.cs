using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceTracker.Invoicing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceSendSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "payment_terms_days",
                table: "invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "line_number",
                table: "invoice_lines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "invoice_number_sequences",
                columns: table => new
                {
                    business_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    year = table.Column<int>(type: "int", nullable: false),
                    last_number = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoice_number_sequences", x => new { x.business_id, x.year });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invoice_number_sequences");

            migrationBuilder.DropColumn(
                name: "payment_terms_days",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "line_number",
                table: "invoice_lines");
        }
    }
}
