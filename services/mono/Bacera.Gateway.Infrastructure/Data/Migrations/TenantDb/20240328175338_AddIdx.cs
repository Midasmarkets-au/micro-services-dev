using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddIdx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: " IX__EventShopClientPoint_ChildAccountId_ParentAccountId_Unique",
                schema: "event",
                table: "_EventShopClientPoint",
                columns: new[] { "ChildAccountId", "ParentAccountId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: " IX__EventShopClientPoint_ChildAccountId_ParentAccountId_Unique",
                schema: "event",
                table: "_EventShopClientPoint");
        }
    }
}
