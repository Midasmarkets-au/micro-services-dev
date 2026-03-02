using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class PaymentAccessPidFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "_PaymentMethodAccess__PartyId_fk",
                schema: "acct",
                table: "_PaymentMethodAccess",
                column: "PartyId",
                principalSchema: "core",
                principalTable: "_Party",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_PaymentMethodAccess__PartyId_fk",
                schema: "acct",
                table: "_PaymentMethodAccess");
        }
    }
}
