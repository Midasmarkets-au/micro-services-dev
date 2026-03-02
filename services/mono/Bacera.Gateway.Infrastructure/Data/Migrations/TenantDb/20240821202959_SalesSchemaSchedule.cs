using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class SalesSchemaSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "Schedule",
                schema: "trd",
                table: "_SalesRebateSchema",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateIndex(
                name: "_SalesRebateSchema_Schedule_index",
                schema: "trd",
                table: "_SalesRebateSchema",
                column: "Schedule");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "_SalesRebateSchema_Schedule_index",
                schema: "trd",
                table: "_SalesRebateSchema");

            migrationBuilder.DropColumn(
                name: "Schedule",
                schema: "trd",
                table: "_SalesRebateSchema");
        }
    }
}