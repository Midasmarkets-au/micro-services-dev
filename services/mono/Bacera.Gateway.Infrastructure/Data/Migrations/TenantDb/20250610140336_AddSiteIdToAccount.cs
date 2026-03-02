using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddSiteIdToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SiteId",
                schema: "trd",
                table: "_Account",
                type: "integer",
                nullable: false,
                defaultValueSql: "'0'::integer");

            migrationBuilder.CreateIndex(
                name: "_Account_SiteId_index",
                schema: "trd",
                table: "_Account",
                column: "SiteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "_Account_SiteId_index",
                schema: "trd",
                table: "_Account");

            migrationBuilder.DropColumn(
                name: "SiteId",
                schema: "trd",
                table: "_Account");
        }
    }
}
