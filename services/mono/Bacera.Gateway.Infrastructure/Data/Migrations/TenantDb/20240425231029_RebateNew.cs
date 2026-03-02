using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class RebateNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_RebateNew",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    FundType = table.Column<int>(type: "integer", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    TradeRebateId = table.Column<long>(type: "bigint", nullable: false),
                    Information = table.Column<string>(type: "text", nullable: false),
                    PostedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StateId = table.Column<int>(type: "integer", nullable: false),
                    StatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("rebate_new_pkey", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX__Rebate_New_AccountId_index",
                schema: "public",
                table: "_RebateNew",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX__Rebate_New_CurrencyId_index",
                schema: "public",
                table: "_RebateNew",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX__Rebate_New_FundType",
                schema: "public",
                table: "_RebateNew",
                column: "FundType");

            migrationBuilder.CreateIndex(
                name: "IX__Rebate_New_TradeRebateId_index",
                schema: "public",
                table: "_RebateNew",
                column: "TradeRebateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_RebateNew",
                schema: "public");
        }
    }
}
