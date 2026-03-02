using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class RefactorConfigurationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "core",
                table: "_Configuration",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataFormat",
                schema: "core",
                table: "_Configuration",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "core",
                table: "_Configuration",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Key",
                schema: "core",
                table: "_Configuration",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "RowId",
                schema: "core",
                table: "_Configuration",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX__Configuration_Category",
                schema: "core",
                table: "_Configuration",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX__Configuration_DataFormat",
                schema: "core",
                table: "_Configuration",
                column: "DataFormat");

            migrationBuilder.CreateIndex(
                name: "IX__Configuration_Key",
                schema: "core",
                table: "_Configuration",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX__Configuration_RowId",
                schema: "core",
                table: "_Configuration",
                column: "RowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX__Configuration_Category",
                schema: "core",
                table: "_Configuration");

            migrationBuilder.DropIndex(
                name: "IX__Configuration_DataFormat",
                schema: "core",
                table: "_Configuration");

            migrationBuilder.DropIndex(
                name: "IX__Configuration_Key",
                schema: "core",
                table: "_Configuration");

            migrationBuilder.DropIndex(
                name: "IX__Configuration_RowId",
                schema: "core",
                table: "_Configuration");

            migrationBuilder.DropColumn(
                name: "Category",
                schema: "core",
                table: "_Configuration");

            migrationBuilder.DropColumn(
                name: "DataFormat",
                schema: "core",
                table: "_Configuration");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "core",
                table: "_Configuration");

            migrationBuilder.DropColumn(
                name: "Key",
                schema: "core",
                table: "_Configuration");

            migrationBuilder.DropColumn(
                name: "RowId",
                schema: "core",
                table: "_Configuration");
        }
    }
}
