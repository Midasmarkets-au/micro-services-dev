using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddExtraInfoToAccountPaymentMethodAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtraInfo",
                schema: "acct",
                table: "_WalletPaymentMethodAccess",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");

            migrationBuilder.AddColumn<string>(
                name: "ExtraInfo",
                schema: "acct",
                table: "_AccountPaymentMethodAccess",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraInfo",
                schema: "acct",
                table: "_WalletPaymentMethodAccess");

            migrationBuilder.DropColumn(
                name: "ExtraInfo",
                schema: "acct",
                table: "_AccountPaymentMethodAccess");
        }
    }
}
