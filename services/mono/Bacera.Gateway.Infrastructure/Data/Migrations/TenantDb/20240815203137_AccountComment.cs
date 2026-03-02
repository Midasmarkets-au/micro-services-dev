using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AccountComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_AccountComment",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    OperatorPartyId = table.Column<long>(type: "bigint", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_AccountComment_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_AccountComment__Account_Id_fk",
                        column: x => x.AccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "_AccountComment__OperatorParty_Id_fk",
                        column: x => x.OperatorPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX__AccountComment_AccountId",
                schema: "trd",
                table: "_AccountComment",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX__AccountComment_OperatorPartyId",
                schema: "trd",
                table: "_AccountComment",
                column: "OperatorPartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_AccountComment",
                schema: "trd");
        }
    }
}