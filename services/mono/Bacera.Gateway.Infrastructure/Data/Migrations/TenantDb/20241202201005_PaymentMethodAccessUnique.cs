using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class PaymentMethodAccessUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_wallet_payment_method_access_wallet_id_payment_method_id_unique",
                schema: "acct",
                table: "_WalletPaymentMethodAccess",
                columns: new[] { "WalletId", "PaymentMethodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_account_payment_method_access_accountId_PaymentMethodId",
                schema: "acct",
                table: "_AccountPaymentMethodAccess",
                columns: new[] { "AccountId", "PaymentMethodId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_wallet_payment_method_access_wallet_id_payment_method_id_unique",
                schema: "acct",
                table: "_WalletPaymentMethodAccess");

            migrationBuilder.DropIndex(
                name: "IX_account_payment_method_access_accountId_PaymentMethodId",
                schema: "acct",
                table: "_AccountPaymentMethodAccess");
        }
    }
}
