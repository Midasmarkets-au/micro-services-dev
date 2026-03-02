using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class RefactorTrdRebate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_TradeRebate__TradeAccount_Id_fk",
                schema: "trd",
                table: "_TradeRebate");

            migrationBuilder.DropIndex(
                name: "_TradeRebate_TradeAccountId_index",
                schema: "trd",
                table: "_TradeRebate");

            migrationBuilder.RenameColumn(
                name: "TradeAccountId",
                schema: "trd",
                table: "_TradeRebate",
                newName: "AccountId");

            migrationBuilder.CreateIndex(
                name: "_TradeRebate_AccountId_index",
                schema: "trd",
                table: "_TradeRebate",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "_TradeRebate__Account_Id_fk",
                schema: "trd",
                table: "_TradeRebate",
                column: "AccountId",
                principalSchema: "trd",
                principalTable: "_Account",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}