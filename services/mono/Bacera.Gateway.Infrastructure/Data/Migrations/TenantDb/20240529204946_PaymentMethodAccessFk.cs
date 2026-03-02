using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class PaymentMethodAccessFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX__PaymentMethodAccess_PaymentMethodId",
                schema: "acct",
                table: "_PaymentMethodAccess",
                column: "PaymentMethodId");

            migrationBuilder.AddForeignKey(
                name: "_PaymentMethodAccess__PaymentMethod_Id_fk",
                schema: "acct",
                table: "_PaymentMethodAccess",
                column: "PaymentMethodId",
                principalSchema: "acct",
                principalTable: "_PaymentMethod",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_PaymentMethodAccess__PaymentMethod_Id_fk",
                schema: "acct",
                table: "_PaymentMethodAccess");

            migrationBuilder.DropIndex(
                name: "IX__PaymentMethodAccess_PaymentMethodId",
                schema: "acct",
                table: "_PaymentMethodAccess");
        }
    }
}