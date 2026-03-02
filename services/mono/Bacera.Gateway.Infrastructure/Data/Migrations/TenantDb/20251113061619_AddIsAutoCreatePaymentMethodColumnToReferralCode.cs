using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddIsAutoCreatePaymentMethodColumnToReferralCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IsAutoCreatePaymentMethod",
                schema: "core",
                table: "_ReferralCode",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "core_referral_codes_is_auto_create_paymentmethod_index",
                schema: "core",
                table: "_ReferralCode",
                column: "IsAutoCreatePaymentMethod");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "core_referral_codes_is_auto_create_paymentmethod_index",
                schema: "core",
                table: "_ReferralCode");

            migrationBuilder.DropColumn(
                name: "IsAutoCreatePaymentMethod",
                schema: "core",
                table: "_ReferralCode");
        }
    }
}
