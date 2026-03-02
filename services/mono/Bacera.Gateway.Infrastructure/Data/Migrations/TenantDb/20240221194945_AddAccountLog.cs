using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddAccountLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_AccountLog",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    OperatorPartyId = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "1"),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Before = table.Column<string>(type: "text", nullable: false),
                    After = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_AccountLog_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_AccountLog__Account_Id_fk",
                        column: x => x.AccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_AccountLog__Operator_Party_Id_fk",
                        column: x => x.OperatorPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX__AccountLog_AccountId",
                schema: "trd",
                table: "_AccountLog",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX__AccountLog_Action",
                schema: "trd",
                table: "_AccountLog",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX__AccountLog_CreatedOn",
                schema: "trd",
                table: "_AccountLog",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX__AccountLog_OperatorPartyId",
                schema: "trd",
                table: "_AccountLog",
                column: "OperatorPartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_AccountLog",
                schema: "trd");
        }
    }
}
