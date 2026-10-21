using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceTracker.Identity.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                business_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                role = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                first_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                last_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                invited_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                is_active = table.Column<bool>(type: "bit", nullable: false),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                modified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                last_login_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                user_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                normalized_user_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                normalized_email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                email_confirmed = table.Column<bool>(type: "bit", nullable: false),
                password_hash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                security_stamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                concurrency_stamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                phone_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                phone_number_confirmed = table.Column<bool>(type: "bit", nullable: false),
                two_factor_enabled = table.Column<bool>(type: "bit", nullable: false),
                lockout_end = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                lockout_enabled = table.Column<bool>(type: "bit", nullable: false),
                access_failed_count = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.user_id);
            });

        migrationBuilder.CreateTable(
            name: "user_claims",
            columns: table => new
            {
                user_claim_id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                claim_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                claim_value = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_claims", x => x.user_claim_id);
                table.ForeignKey(
                    name: "fk_user_claims_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "user_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_logins",
            columns: table => new
            {
                login_provider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                provider_key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                provider_display_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_logins", x => new { x.login_provider, x.provider_key });
                table.ForeignKey(
                    name: "fk_user_logins_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "user_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_tokens",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                login_provider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                value = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                table.ForeignKey(
                    name: "fk_user_tokens_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "user_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_user_claims_user_id",
            table: "user_claims",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_logins_user_id",
            table: "user_logins",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_users_business_id",
            table: "users",
            column: "business_id");

        migrationBuilder.CreateIndex(
            name: "ix_users_normalized_email",
            table: "users",
            column: "normalized_email");

        migrationBuilder.CreateIndex(
            name: "uq_users_normalized_user_name",
            table: "users",
            column: "normalized_user_name",
            unique: true,
            filter: "[normalized_user_name] IS NOT NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "user_claims");

        migrationBuilder.DropTable(
            name: "user_logins");

        migrationBuilder.DropTable(
            name: "user_tokens");

        migrationBuilder.DropTable(
            name: "users");
    }
}
