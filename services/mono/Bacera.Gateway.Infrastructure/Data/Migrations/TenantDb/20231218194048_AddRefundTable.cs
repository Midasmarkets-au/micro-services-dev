using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddRefundTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_Refund",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    FundType = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    Comment = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false,
                        defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Refund_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_Refund__Currency_Id_fk",
                        column: x => x.CurrencyId,
                        principalSchema: "acct",
                        principalTable: "_Currency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Refund__FundType_Id_fk",
                        column: x => x.FundType,
                        principalSchema: "acct",
                        principalTable: "_FundType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Refund__Matter_Id_fk",
                        column: x => x.Id,
                        principalSchema: "core",
                        principalTable: "_Matter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_Refund__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX__Refund_CurrencyId",
                schema: "acct",
                table: "_Refund",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX__Refund_FundType",
                schema: "acct",
                table: "_Refund",
                column: "FundType");

            migrationBuilder.CreateIndex(
                name: "IX__Refund_PartyId",
                schema: "acct",
                table: "_Refund",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX__Refund_TargetId",
                schema: "acct",
                table: "_Refund",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX__Refund_TargetType",
                schema: "acct",
                table: "_Refund",
                column: "TargetType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_Refund",
                schema: "acct");
        }
    }
}