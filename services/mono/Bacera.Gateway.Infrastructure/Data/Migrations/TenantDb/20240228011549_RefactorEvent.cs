using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class RefactorEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Point",
                schema: "event",
                table: "_EventShopOrder");

            migrationBuilder.RenameColumn(
                name: "Point",
                schema: "event",
                table: "_EventShopReward",
                newName: "TotalPoint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalPoint",
                schema: "event",
                table: "_EventShopReward",
                newName: "Point");

            migrationBuilder.AddColumn<long>(
                name: "Point",
                schema: "event",
                table: "_EventShopOrder",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
