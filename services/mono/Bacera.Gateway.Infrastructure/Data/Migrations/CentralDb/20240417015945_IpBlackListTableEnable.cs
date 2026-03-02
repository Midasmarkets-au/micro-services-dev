using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.CentralDb
{
    /// <inheritdoc />
    public partial class IpBlackListTableEnable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                schema: "core",
                table: "_IpBlackList",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Enabled",
                schema: "core",
                table: "_IpBlackList");
        }
    }
}
