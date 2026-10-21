using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceTracker.Business.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "addresses",
            columns: table => new
            {
                address_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                line1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                line2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                suburb = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                postal_code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                country_code = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: false),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                modified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                modified_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_addresses", x => x.address_id);
            });

        migrationBuilder.CreateTable(
            name: "bank_details",
            columns: table => new
            {
                bank_details_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                bank_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                account_holder = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                account_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                account_type = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                branch_code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                branch_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                modified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                modified_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_bank_details", x => x.bank_details_id);
            });

        migrationBuilder.CreateTable(
            name: "contact_numbers",
            columns: table => new
            {
                contact_number_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                contact_number_type = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                modified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                modified_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_contact_numbers", x => x.contact_number_id);
            });

        migrationBuilder.CreateTable(
            name: "email_addresses",
            columns: table => new
            {
                email_address_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                modified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                modified_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_email_addresses", x => x.email_address_id);
            });

        migrationBuilder.CreateTable(
            name: "businesses",
            columns: table => new
            {
                business_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                registration_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                vat_number = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                support_email_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                contact_number_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                address_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                bank_details_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                default_payment_terms_days = table.Column<int>(type: "int", nullable: false),
                time_zone = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                logo_blob_path = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                is_active = table.Column<bool>(type: "bit", nullable: false),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                modified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                modified_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_businesses", x => x.business_id);
                table.ForeignKey(
                    name: "fk_businesses_addresses",
                    column: x => x.address_id,
                    principalTable: "addresses",
                    principalColumn: "address_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_businesses_bank_details",
                    column: x => x.bank_details_id,
                    principalTable: "bank_details",
                    principalColumn: "bank_details_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_businesses_contact_numbers",
                    column: x => x.contact_number_id,
                    principalTable: "contact_numbers",
                    principalColumn: "contact_number_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_businesses_email_addresses",
                    column: x => x.support_email_id,
                    principalTable: "email_addresses",
                    principalColumn: "email_address_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "customers",
            columns: table => new
            {
                customer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                business_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                email_address_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                contact_number_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                billing_address_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                customer_reference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                vat_number = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                is_archived = table.Column<bool>(type: "bit", nullable: false),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                modified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                modified_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_customers", x => x.customer_id);
                table.ForeignKey(
                    name: "fk_customers_addresses",
                    column: x => x.billing_address_id,
                    principalTable: "addresses",
                    principalColumn: "address_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_customers_businesses",
                    column: x => x.business_id,
                    principalTable: "businesses",
                    principalColumn: "business_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_customers_contact_numbers",
                    column: x => x.contact_number_id,
                    principalTable: "contact_numbers",
                    principalColumn: "contact_number_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_customers_email_addresses",
                    column: x => x.email_address_id,
                    principalTable: "email_addresses",
                    principalColumn: "email_address_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_businesses_address_id",
            table: "businesses",
            column: "address_id",
            unique: true,
            filter: "[address_id] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_businesses_bank_details_id",
            table: "businesses",
            column: "bank_details_id",
            unique: true,
            filter: "[bank_details_id] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_businesses_contact_number_id",
            table: "businesses",
            column: "contact_number_id",
            unique: true,
            filter: "[contact_number_id] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_businesses_support_email_id",
            table: "businesses",
            column: "support_email_id",
            unique: true,
            filter: "[support_email_id] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_customers_billing_address_id",
            table: "customers",
            column: "billing_address_id",
            unique: true,
            filter: "[billing_address_id] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_customers_business_id_name",
            table: "customers",
            columns: new[] { "business_id", "name" });

        migrationBuilder.CreateIndex(
            name: "ix_customers_contact_number_id",
            table: "customers",
            column: "contact_number_id",
            unique: true,
            filter: "[contact_number_id] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_customers_email_address_id",
            table: "customers",
            column: "email_address_id",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "customers");

        migrationBuilder.DropTable(
            name: "businesses");

        migrationBuilder.DropTable(
            name: "addresses");

        migrationBuilder.DropTable(
            name: "bank_details");

        migrationBuilder.DropTable(
            name: "contact_numbers");

        migrationBuilder.DropTable(
            name: "email_addresses");
    }
}
