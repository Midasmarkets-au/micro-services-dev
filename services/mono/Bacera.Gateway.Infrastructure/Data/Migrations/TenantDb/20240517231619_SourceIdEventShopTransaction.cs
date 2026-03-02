using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class SourceIdEventShopTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SourceId",
                schema: "event",
                table: "_EventShopPointTransaction",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX__EventShopPointTransaction_SourceId",
                schema: "event",
                table: "_EventShopPointTransaction",
                column: "SourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX__EventShopPointTransaction_SourceId",
                schema: "event",
                table: "_EventShopPointTransaction");

            migrationBuilder.DropColumn(
                name: "SourceId",
                schema: "event",
                table: "_EventShopPointTransaction");
        }
    }
}
