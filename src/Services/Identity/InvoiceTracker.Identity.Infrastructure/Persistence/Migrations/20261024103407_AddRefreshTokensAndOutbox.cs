using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceTracker.Identity.Infrastructure.Persistence.Migrations;


public partial class AddRefreshTokensAndOutbox : Migration
{

    protected override void Up(MigrationBuilder migrationBuilder)
    {
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
            name: "refresh_tokens",
            columns: table => new
            {
                refresh_token_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                family_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                token_hash = table.Column<string>(type: "nchar(64)", fixedLength: true, maxLength: 64, nullable: false),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                revoked_at = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_refresh_tokens", x => x.refresh_token_id);
                table.ForeignKey(
                    name: "fk_refresh_tokens_users",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "user_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_outbox_messages_unprocessed",
            table: "outbox_messages",
            column: "occurred_at",
            filter: "[processed_at] IS NULL");

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_family_id",
            table: "refresh_tokens",
            column: "family_id");

        migrationBuilder.CreateIndex(
            name: "ix_refresh_tokens_user_id",
            table: "refresh_tokens",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "uq_refresh_tokens_token_hash",
            table: "refresh_tokens",
            column: "token_hash",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "outbox_messages");

        migrationBuilder.DropTable(
            name: "refresh_tokens");
    }
}

