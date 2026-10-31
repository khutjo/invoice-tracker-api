using InvoiceTracker.Business.Infrastructure.Persistence.ReferenceData;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceTracker.Business.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "banks",
                columns: table => new
                {
                    bank_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    universal_branch_code = table.Column<string>(type: "char(6)", unicode: false, fixedLength: true, maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_banks", x => x.bank_id);
                });

            migrationBuilder.CreateTable(
                name: "postal_codes",
                columns: table => new
                {
                    postal_code_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "char(4)", unicode: false, fixedLength: true, maxLength: 4, nullable: false),
                    suburb = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    town = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    kind = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_postal_codes", x => x.postal_code_id);
                });

            migrationBuilder.CreateIndex(
                name: "uq_banks_name",
                table: "banks",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_postal_codes_code",
                table: "postal_codes",
                column: "code");

            // Seed the reference data from the embedded .tsv files (see ReferenceDataSeed)
            migrationBuilder.Sql(ReferenceDataSeed.BanksSql());
            migrationBuilder.Sql(ReferenceDataSeed.PostalCodesSql());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "banks");

            migrationBuilder.DropTable(
                name: "postal_codes");
        }
    }
}
