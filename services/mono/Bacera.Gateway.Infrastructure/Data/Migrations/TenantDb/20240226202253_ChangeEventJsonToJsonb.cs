using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class ChangeEventJsonToJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Image",
                schema: "event",
                table: "_EventShopItemLanguage",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb",
                oldClrType: typeof(string),
                oldType: "json",
                oldDefaultValueSql: "'{}'::json");

            migrationBuilder.AlterColumn<string>(
                name: "Configuration",
                schema: "event",
                table: "_EventShopItem",
                type: "json",
                nullable: false,
                defaultValueSql: "'{}'::jsonb",
                oldClrType: typeof(string),
                oldType: "json",
                oldDefaultValueSql: "'{}'::json");

            migrationBuilder.AlterColumn<string>(
                name: "AccessRoles",
                schema: "event",
                table: "_EventShopItem",
                type: "json",
                nullable: false,
                defaultValueSql: "'[]'::jsonb",
                oldClrType: typeof(string),
                oldType: "json",
                oldDefaultValueSql: "'[]'::json");

            migrationBuilder.AlterColumn<string>(
                name: "Settings",
                schema: "event",
                table: "_EventParty",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb",
                oldClrType: typeof(string),
                oldType: "json",
                oldDefaultValueSql: "'[]'::json");

            migrationBuilder.AlterColumn<string>(
                name: "Images",
                schema: "event",
                table: "_EventLanguage",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb",
                oldClrType: typeof(string),
                oldType: "json",
                oldDefaultValueSql: "'[]'::json");

            migrationBuilder.AlterColumn<string>(
                name: "AccessRoles",
                schema: "event",
                table: "_Event",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb",
                oldClrType: typeof(string),
                oldType: "json",
                oldDefaultValueSql: "'[]'::json");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Image",
                schema: "event",
                table: "_EventShopItemLanguage",
                type: "json",
                nullable: false,
                defaultValueSql: "'{}'::json",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldDefaultValueSql: "'{}'::jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "Configuration",
                schema: "event",
                table: "_EventShopItem",
                type: "json",
                nullable: false,
                defaultValueSql: "'{}'::json",
                oldClrType: typeof(string),
                oldType: "json",
                oldDefaultValueSql: "'{}'::jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "AccessRoles",
                schema: "event",
                table: "_EventShopItem",
                type: "json",
                nullable: false,
                defaultValueSql: "'[]'::json",
                oldClrType: typeof(string),
                oldType: "json",
                oldDefaultValueSql: "'[]'::jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "Settings",
                schema: "event",
                table: "_EventParty",
                type: "json",
                nullable: false,
                defaultValueSql: "'[]'::json",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldDefaultValueSql: "'[]'::jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "Images",
                schema: "event",
                table: "_EventLanguage",
                type: "json",
                nullable: false,
                defaultValueSql: "'[]'::json",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldDefaultValueSql: "'[]'::jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "AccessRoles",
                schema: "event",
                table: "_Event",
                type: "json",
                nullable: false,
                defaultValueSql: "'[]'::json",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldDefaultValueSql: "'[]'::jsonb");
        }
    }
}
