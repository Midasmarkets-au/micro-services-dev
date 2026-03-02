using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddCaseLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_CaseLanguage",
                schema: "sto",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CaseId = table.Column<long>(type: "bigint", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_CaseLanguage_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_Case__Language_Id_fk",
                        column: x => x.CaseId,
                        principalSchema: "sto",
                        principalTable: "_Case",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX__CaseLanguage_CaseId",
                schema: "sto",
                table: "_CaseLanguage",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX__CaseLanguage_Language",
                schema: "sto",
                table: "_CaseLanguage",
                column: "Language");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_CaseLanguage",
                schema: "sto");
        }
    }
}
