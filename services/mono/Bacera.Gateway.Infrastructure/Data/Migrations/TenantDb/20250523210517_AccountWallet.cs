using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AccountWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "WalletId",
                schema: "trd",
                table: "_Account",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX__Account_WalletId",
                schema: "trd",
                table: "_Account",
                column: "WalletId");

            migrationBuilder.AddForeignKey(
                name: "trd_accounts_wallet_id_foreign",
                schema: "trd",
                table: "_Account",
                column: "WalletId",
                principalSchema: "acct",
                principalTable: "_Wallet",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "trd_accounts_wallet_id_foreign",
                schema: "trd",
                table: "_Account");

            migrationBuilder.DropIndex(
                name: "IX__Account_WalletId",
                schema: "trd",
                table: "_Account");

            migrationBuilder.DropColumn(
                name: "WalletId",
                schema: "trd",
                table: "_Account");
        }
    }
}
