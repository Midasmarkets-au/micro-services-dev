using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AccountPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Permission",
                schema: "trd",
                table: "_Account",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValueSql: "'11111'::character varying");

            migrationBuilder.CreateIndex(
                name: "_Account_permission_index",
                schema: "trd",
                table: "_Account",
                column: "Permission");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "_Account_permission_index",
                schema: "trd",
                table: "_Account");

            migrationBuilder.DropColumn(
                name: "Permission",
                schema: "trd",
                table: "_Account");
        }
    }
}
