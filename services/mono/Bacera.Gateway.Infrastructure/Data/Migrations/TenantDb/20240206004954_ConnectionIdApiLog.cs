using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class ConnectionIdApiLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConnectionId",
                schema: "core",
                table: "_ApiLog",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "_ApiLog_ConnectionId_index",
                schema: "core",
                table: "_ApiLog",
                column: "ConnectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "_ApiLog_ConnectionId_index",
                schema: "core",
                table: "_ApiLog");

            migrationBuilder.DropColumn(
                name: "ConnectionId",
                schema: "core",
                table: "_ApiLog");
        }
    }
}