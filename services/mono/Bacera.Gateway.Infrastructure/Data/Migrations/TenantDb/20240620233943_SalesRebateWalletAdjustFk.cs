using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class SalesRebateWalletAdjustFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX__SalesRebate_WalletAdjustId",
                schema: "trd",
                table: "_SalesRebate",
                column: "WalletAdjustId");

            migrationBuilder.AddForeignKey(
                name: "_SalesRebate_WalletAdjustId_fkey",
                schema: "trd",
                table: "_SalesRebate",
                column: "WalletAdjustId",
                principalSchema: "acct",
                principalTable: "_WalletAdjust",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_SalesRebate_WalletAdjustId_fkey",
                schema: "trd",
                table: "_SalesRebate");

            migrationBuilder.DropIndex(
                name: "IX__SalesRebate_WalletAdjustId",
                schema: "trd",
                table: "_SalesRebate");
        }
    }
}