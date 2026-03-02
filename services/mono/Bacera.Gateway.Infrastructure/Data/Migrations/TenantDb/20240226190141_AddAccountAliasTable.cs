using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddAccountAliasTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_AccountAlias",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    Alias = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_AccountAlias_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_AccountAlias__Account_Id_fk",
                        column: x => x.AccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_AccountAlias__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: " IX__AccountAlias_PartyId_AccountId_UX",
                schema: "trd",
                table: "_AccountAlias",
                columns: new[] { "PartyId", "AccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__AccountAlias_AccountId",
                schema: "trd",
                table: "_AccountAlias",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX__AccountAlias_PartyId",
                schema: "trd",
                table: "_AccountAlias",
                column: "PartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_AccountAlias",
                schema: "trd");
        }
    }
}
