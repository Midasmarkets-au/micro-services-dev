using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class RemoveSiteTypeUniqueInConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "_Configuration_Type_SiteId_uindex",
                schema: "core",
                table: "_Configuration");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "_Configuration_Type_SiteId_uindex",
                schema: "core",
                table: "_Configuration",
                columns: new[] { "Type", "SiteId" },
                unique: true);
        }
    }
}
