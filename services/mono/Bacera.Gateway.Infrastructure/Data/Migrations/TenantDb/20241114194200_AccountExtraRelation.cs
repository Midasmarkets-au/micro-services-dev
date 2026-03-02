using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AccountExtraRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_AccountExtraRelation",
                schema: "trd",
                columns: table => new
                {
                    ParentAccountId = table.Column<long>(type: "bigint", nullable: false),
                    ChildAccountId = table.Column<long>(type: "bigint", nullable: false),
                    OperatorPartyId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("account_extra_relations_pkey", x => new { x.ParentAccountId, x.ChildAccountId });
                    table.ForeignKey(
                        name: "_AccountExtraRelation__Child_Account_Id_fk",
                        column: x => x.ChildAccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_AccountExtraRelation__Operator_Party_Id_fk",
                        column: x => x.OperatorPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_AccountExtraRelation__Parent_Account_Id_fk",
                        column: x => x.ParentAccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX__AccountExtraRelation_ChildAccountId",
                schema: "trd",
                table: "_AccountExtraRelation",
                column: "ChildAccountId");

            migrationBuilder.CreateIndex(
                name: "IX__AccountExtraRelation_OperatorPartyId",
                schema: "trd",
                table: "_AccountExtraRelation",
                column: "OperatorPartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_AccountExtraRelation",
                schema: "trd");
        }
    }
}