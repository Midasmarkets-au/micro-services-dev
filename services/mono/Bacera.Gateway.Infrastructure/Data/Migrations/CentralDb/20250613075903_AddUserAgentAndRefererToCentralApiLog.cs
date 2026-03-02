using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.CentralDb
{
    /// <inheritdoc />
    public partial class AddUserAgentAndRefererToCentralApiLog : Migration
    {        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                schema: "core",
                table: "_ApiLog",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Referer",
                schema: "core",
                table: "_ApiLog",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserAgent",
                schema: "core",
                table: "_ApiLog");

            migrationBuilder.DropColumn(
                name: "Referer",
                schema: "core",
                table: "_ApiLog");
        }
    }
}
