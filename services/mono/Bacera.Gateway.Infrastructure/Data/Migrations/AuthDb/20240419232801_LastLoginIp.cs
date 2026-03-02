using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.AuthDb
{
    /// <inheritdoc />
    public partial class LastLoginIp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastLoginIp",
                schema: "auth",
                table: "_User",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValueSql: "''::character varying");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLoginIp",
                schema: "auth",
                table: "_User");
        }
    }
}
