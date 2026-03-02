using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AccountIdEventShopTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AccountId",
                schema: "event",
                table: "_EventShopPointTransaction",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX__EventShopPointTransaction_AccountId",
                schema: "event",
                table: "_EventShopPointTransaction",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "_EventShopPointTransaction__AccountId_fk",
                schema: "event",
                table: "_EventShopPointTransaction",
                column: "AccountId",
                principalSchema: "trd",
                principalTable: "_Account",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_EventShopPointTransaction__AccountId_fk",
                schema: "event",
                table: "_EventShopPointTransaction");

            migrationBuilder.DropIndex(
                name: "IX__EventShopPointTransaction_AccountId",
                schema: "event",
                table: "_EventShopPointTransaction");

            migrationBuilder.DropColumn(
                name: "AccountId",
                schema: "event",
                table: "_EventShopPointTransaction");
        }
    }
}
