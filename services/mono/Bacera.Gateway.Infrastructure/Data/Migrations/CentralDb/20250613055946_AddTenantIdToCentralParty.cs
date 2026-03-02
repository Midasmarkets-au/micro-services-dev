using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.CentralDb
{
    /// <inheritdoc />
    public partial class AddTenantIdToCentralParty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TenantId",
                schema: "core",
                table: "_CentralParty",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          migrationBuilder.DropColumn(
              name: "TenantId",
              schema: "core",
              table: "_CentralParty");
        }
    }
}
