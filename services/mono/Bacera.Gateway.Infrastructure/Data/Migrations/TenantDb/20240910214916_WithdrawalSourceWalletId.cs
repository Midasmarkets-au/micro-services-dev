using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class WithdrawalSourceWalletId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SourceWalletId",
                schema: "acct",
                table: "_Withdrawal",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX__Withdrawal_SourceWalletId",
                schema: "acct",
                table: "_Withdrawal",
                column: "SourceWalletId");

            migrationBuilder.AddForeignKey(
                name: "_Withdrawal__Wallet_Id_fk",
                schema: "acct",
                table: "_Withdrawal",
                column: "SourceWalletId",
                principalSchema: "acct",
                principalTable: "_Wallet",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_Withdrawal__Wallet_Id_fk",
                schema: "acct",
                table: "_Withdrawal");

            migrationBuilder.DropIndex(
                name: "IX__Withdrawal_SourceWalletId",
                schema: "acct",
                table: "_Withdrawal");

            migrationBuilder.DropColumn(
                name: "SourceWalletId",
                schema: "acct",
                table: "_Withdrawal");
        }
    }
}
