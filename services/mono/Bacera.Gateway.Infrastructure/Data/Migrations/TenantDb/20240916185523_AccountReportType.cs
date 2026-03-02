using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AccountReportType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "trd",
                table: "_AccountReport",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX__AccountReport_Type",
                schema: "trd",
                table: "_AccountReport",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX__AccountReport_Type",
                schema: "trd",
                table: "_AccountReport");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "trd",
                table: "_AccountReport");
        }
    }
}
