using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddReferralStatusAndPaymentIdentifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "Status",
                schema: "core",
                table: "_ReferralCode",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);


            migrationBuilder.CreateIndex(
                name: "core_referral_codes_status_index",
                schema: "core",
                table: "_ReferralCode",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "core_referral_codes_status_index",
                schema: "core",
                table: "_ReferralCode");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "core",
                table: "_ReferralCode");
        }
    }
}