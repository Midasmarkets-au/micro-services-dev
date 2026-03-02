using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class UpdateAccountStatTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Adjust",
                schema: "trd",
                table: "_AccountStat",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Credit",
                schema: "trd",
                table: "_AccountStat",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Equity",
                schema: "trd",
                table: "_AccountStat",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PreviousEquity",
                schema: "trd",
                table: "_AccountStat",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SalesRebateAmount",
                schema: "trd",
                table: "_AccountStat",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SalesRebateCount",
                schema: "trd",
                table: "_AccountStat",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adjust",
                schema: "trd",
                table: "_AccountStat");

            migrationBuilder.DropColumn(
                name: "Credit",
                schema: "trd",
                table: "_AccountStat");

            migrationBuilder.DropColumn(
                name: "Equity",
                schema: "trd",
                table: "_AccountStat");

            migrationBuilder.DropColumn(
                name: "PreviousEquity",
                schema: "trd",
                table: "_AccountStat");

            migrationBuilder.DropColumn(
                name: "SalesRebateAmount",
                schema: "trd",
                table: "_AccountStat");

            migrationBuilder.DropColumn(
                name: "SalesRebateCount",
                schema: "trd",
                table: "_AccountStat");
        }
    }
}
