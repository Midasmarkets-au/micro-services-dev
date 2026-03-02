using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class ReferPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferPath",
                schema: "trd",
                table: "_TradeRebate",
                type: "text",
                nullable: false,
                defaultValueSql: "''::character varying");

            migrationBuilder.CreateIndex(
                name: "_TradeRebate_ReferPath_index",
                schema: "trd",
                table: "_TradeRebate",
                column: "ReferPath");

            migrationBuilder.CreateIndex(
                name: "_Account_ReferPath_index",
                schema: "trd",
                table: "_Account",
                column: "ReferPath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "_TradeRebate_ReferPath_index",
                schema: "trd",
                table: "_TradeRebate");

            migrationBuilder.DropIndex(
                name: "_Account_ReferPath_index",
                schema: "trd",
                table: "_Account");

            migrationBuilder.DropColumn(
                name: "ReferPath",
                schema: "trd",
                table: "_TradeRebate");
        }
    }
}
