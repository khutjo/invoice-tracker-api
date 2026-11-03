using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceTracker.Invoicing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    invoice_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    business_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    customer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    invoice_number = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    payment_terms_days = table.Column<int>(type: "int", nullable: false),
                    billed_customer_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    billed_customer_email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    billed_customer_vat_number = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    billed_customer_address_line1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    billed_customer_address_line2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    billed_customer_suburb = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    billed_customer_city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    billed_customer_postal_code = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    issue_date = table.Column<DateOnly>(type: "date", nullable: true),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    vat_total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    amount_paid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoices", x => x.invoice_id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    outbox_message_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    processed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    attempts = table.Column<int>(type: "int", nullable: false),
                    error = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.outbox_message_id);
                });

            migrationBuilder.CreateTable(
                name: "invoice_documents",
                columns: table => new
                {
                    invoice_document_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    blob_path = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    content_hash = table.Column<string>(type: "nchar(64)", fixedLength: true, maxLength: 64, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    generated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoice_documents", x => x.invoice_document_id);
                    table.ForeignKey(
                        name: "fk_invoice_documents_invoices",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "invoice_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "invoice_events",
                columns: table => new
                {
                    invoice_event_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    event_type = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    occurred_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    detail = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoice_events", x => x.invoice_event_id);
                    table.ForeignKey(
                        name: "fk_invoice_events_invoices",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "invoice_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoice_lines",
                columns: table => new
                {
                    invoice_line_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    line_number = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    vat_rate = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    line_total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoice_lines", x => x.invoice_line_id);
                    table.ForeignKey(
                        name: "fk_invoice_lines_invoices",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "invoice_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    payment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    paid_on = table.Column<DateOnly>(type: "date", nullable: false),
                    method = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    recorded_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.payment_id);
                    table.ForeignKey(
                        name: "fk_payments_invoices",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "invoice_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "uq_invoice_documents_invoice_id",
                table: "invoice_documents",
                column: "invoice_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invoice_events_invoice_id_occurred_at",
                table: "invoice_events",
                columns: new[] { "invoice_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_invoice_lines_invoice_id",
                table: "invoice_lines",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_business_id_customer_id",
                table: "invoices",
                columns: new[] { "business_id", "customer_id" });

            migrationBuilder.CreateIndex(
                name: "ix_invoices_business_id_status_due_date",
                table: "invoices",
                columns: new[] { "business_id", "status", "due_date" });

            migrationBuilder.CreateIndex(
                name: "uq_invoices_business_id_invoice_number",
                table: "invoices",
                columns: new[] { "business_id", "invoice_number" },
                unique: true,
                filter: "[invoice_number] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_unprocessed",
                table: "outbox_messages",
                column: "occurred_at",
                filter: "[processed_at] IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_payments_invoice_id",
                table: "payments",
                column: "invoice_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invoice_documents");

            migrationBuilder.DropTable(
                name: "invoice_events");

            migrationBuilder.DropTable(
                name: "invoice_lines");

            migrationBuilder.DropTable(
                name: "invoice_number_sequences");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "invoices");
        }
    }
}
