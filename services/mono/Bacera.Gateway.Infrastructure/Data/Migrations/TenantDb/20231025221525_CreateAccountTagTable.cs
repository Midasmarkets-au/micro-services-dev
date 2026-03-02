using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class CreateAccountTagTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_AccountTag",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false,
                        defaultValueSql: "''::character varying")
                },
                constraints: table => { table.PrimaryKey("_AccountTag_pk", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "_AccountHasTag",
                schema: "trd",
                columns: table => new
                {
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    AccountTagId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_AccountHasTag_pk", x => new { x.AccountId, x.AccountTagId });
                    table.ForeignKey(
                        name: "_AccountHasTag__AccountTag_Id_fk",
                        column: x => x.AccountTagId,
                        principalSchema: "trd",
                        principalTable: "_AccountTag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "_AccountHasTag__Account_Id_fk",
                        column: x => x.AccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_AccountHasTag",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_AccountTag",
                schema: "trd");
        }
    }
}