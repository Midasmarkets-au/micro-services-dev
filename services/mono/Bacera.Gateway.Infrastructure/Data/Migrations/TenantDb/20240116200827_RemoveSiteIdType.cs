using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class RemoveSiteIdType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_Configuration__Site_Id_fk",
                schema: "core",
                table: "_Configuration");

            migrationBuilder.DropIndex(
                name: "IX__Configuration_SiteId",
                schema: "core",
                table: "_Configuration");

            migrationBuilder.DropColumn(
                name: "SiteId",
                schema: "core",
                table: "_Configuration");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "core",
                table: "_Configuration");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SiteId",
                schema: "core",
                table: "_Configuration",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "core",
                table: "_Configuration",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX__Configuration_SiteId",
                schema: "core",
                table: "_Configuration",
                column: "SiteId");

            migrationBuilder.AddForeignKey(
                name: "_Configuration__Site_Id_fk",
                schema: "core",
                table: "_Configuration",
                column: "SiteId",
                principalSchema: "core",
                principalTable: "_Site",
                principalColumn: "Id");
        }
    }
}