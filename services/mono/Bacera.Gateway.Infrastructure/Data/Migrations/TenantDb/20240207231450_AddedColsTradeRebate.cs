using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddedColsTradeRebate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ClosePrice",
                schema: "trd",
                table: "_TradeRebate",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Commission",
                schema: "trd",
                table: "_TradeRebate",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OpenPrice",
                schema: "trd",
                table: "_TradeRebate",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Profit",
                schema: "trd",
                table: "_TradeRebate",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Swaps",
                schema: "trd",
                table: "_TradeRebate",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "_TradeRebate_Commission_index",
                schema: "trd",
                table: "_TradeRebate",
                column: "Commission");

            migrationBuilder.CreateIndex(
                name: "_TradeRebate_Pl_index",
                schema: "trd",
                table: "_TradeRebate",
                column: "Profit");

            migrationBuilder.CreateIndex(
                name: "_TradeRebate_Swaps_index",
                schema: "trd",
                table: "_TradeRebate",
                column: "Swaps");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "_TradeRebate_Commission_index",
                schema: "trd",
                table: "_TradeRebate");

            migrationBuilder.DropIndex(
                name: "_TradeRebate_Pl_index",
                schema: "trd",
                table: "_TradeRebate");

            migrationBuilder.DropIndex(
                name: "_TradeRebate_Swaps_index",
                schema: "trd",
                table: "_TradeRebate");

            migrationBuilder.DropColumn(
                name: "ClosePrice",
                schema: "trd",
                table: "_TradeRebate");

            migrationBuilder.DropColumn(
                name: "Commission",
                schema: "trd",
                table: "_TradeRebate");

            migrationBuilder.DropColumn(
                name: "OpenPrice",
                schema: "trd",
                table: "_TradeRebate");

            migrationBuilder.DropColumn(
                name: "Profit",
                schema: "trd",
                table: "_TradeRebate");

            migrationBuilder.DropColumn(
                name: "Swaps",
                schema: "trd",
                table: "_TradeRebate");
        }
    }
}
