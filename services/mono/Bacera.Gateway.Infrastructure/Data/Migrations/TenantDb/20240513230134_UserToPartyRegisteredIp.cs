using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class UserToPartyRegisteredIp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegisteredIp",
                schema: "core",
                table: "_Party",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValueSql: "''::character varying");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegisteredIp",
                schema: "core",
                table: "_Party");
        }
    }
}
