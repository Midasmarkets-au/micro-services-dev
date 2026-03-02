using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class RefactorTrdAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_Deposit__TradeAccount_Id_fk",
                schema: "acct",
                table: "_Deposit");

            migrationBuilder.DropForeignKey(
                name: "_Withdrawal__TradeAccount_Id_fk",
                schema: "acct",
                table: "_Withdrawal");

            migrationBuilder.RenameColumn(
                name: "SourceTradeAccountId",
                schema: "acct",
                table: "_Withdrawal",
                newName: "SourceAccountId");

            migrationBuilder.RenameColumn(
                name: "TargetTradeAccountId",
                schema: "acct",
                table: "_Deposit",
                newName: "TargetAccountId");

            migrationBuilder.RenameIndex(
                name: "IX__Deposit_TargetTradeAccountId",
                schema: "acct",
                table: "_Deposit",
                newName: "IX__Deposit_TargetAccountId");

            migrationBuilder.AddForeignKey(
                name: "_Deposit__Account_Id_fk",
                schema: "acct",
                table: "_Deposit",
                column: "TargetAccountId",
                principalSchema: "trd",
                principalTable: "_Account",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "trd_account_status_id_foreign",
                schema: "trd",
                table: "_TradeAccountStatus",
                column: "Id",
                principalSchema: "trd",
                principalTable: "_Account",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "_Withdrawal__Account_Id_fk",
                schema: "acct",
                table: "_Withdrawal",
                column: "SourceAccountId",
                principalSchema: "trd",
                principalTable: "_Account",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_Deposit__Account_Id_fk",
                schema: "acct",
                table: "_Deposit");

            migrationBuilder.DropForeignKey(
                name: "trd_account_status_id_foreign",
                schema: "trd",
                table: "_TradeAccountStatus");

            migrationBuilder.DropForeignKey(
                name: "_Withdrawal__Account_Id_fk",
                schema: "acct",
                table: "_Withdrawal");

            migrationBuilder.RenameColumn(
                name: "SourceAccountId",
                schema: "acct",
                table: "_Withdrawal",
                newName: "SourceTradeAccountId");

            migrationBuilder.RenameColumn(
                name: "TargetAccountId",
                schema: "acct",
                table: "_Deposit",
                newName: "TargetTradeAccountId");

            migrationBuilder.RenameIndex(
                name: "IX__Deposit_TargetAccountId",
                schema: "acct",
                table: "_Deposit",
                newName: "IX__Deposit_TargetTradeAccountId");

            migrationBuilder.AddForeignKey(
                name: "_Deposit__TradeAccount_Id_fk",
                schema: "acct",
                table: "_Deposit",
                column: "TargetTradeAccountId",
                principalSchema: "trd",
                principalTable: "_TradeAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "_Withdrawal__TradeAccount_Id_fk",
                schema: "acct",
                table: "_Withdrawal",
                column: "SourceTradeAccountId",
                principalSchema: "trd",
                principalTable: "_TradeAccount",
                principalColumn: "Id");
        }
    }
}
