using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class ChangeEventPartyAndPointRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX__EventShopPoint_EventPartyId",
                schema: "event",
                table: "_EventShopPoint",
                newName: "IX__EventShopPoint_EventPartyId1");

            migrationBuilder.CreateIndex(
                name: "IX__EventShopPoint_EventPartyId",
                schema: "event",
                table: "_EventShopPoint",
                column: "EventPartyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__Event_AccessRoles",
                schema: "event",
                table: "_Event",
                column: "AccessRoles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX__EventShopPoint_EventPartyId",
                schema: "event",
                table: "_EventShopPoint");

            migrationBuilder.DropIndex(
                name: "IX__Event_AccessRoles",
                schema: "event",
                table: "_Event");

            migrationBuilder.RenameIndex(
                name: "IX__EventShopPoint_EventPartyId1",
                schema: "event",
                table: "_EventShopPoint",
                newName: "IX__EventShopPoint_EventPartyId");
        }
    }
}
