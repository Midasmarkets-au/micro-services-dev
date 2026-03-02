using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class ChangeSalesRebateWalletAdjustIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "WalletAdjustId",
                schema: "trd",
                table: "_SalesRebate",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "_Payment__PaymentMethod_Id_fk",
                schema: "acct",
                table: "_Payment",
                column: "PaymentServiceId",
                principalSchema: "acct",
                principalTable: "_PaymentMethod",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_Payment__PaymentMethod_Id_fk",
                schema: "acct",
                table: "_Payment");

            migrationBuilder.AlterColumn<long>(
                name: "WalletAdjustId",
                schema: "trd",
                table: "_SalesRebate",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
