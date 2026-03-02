using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class FixImagesDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix the default value for Images column in _EventLanguage table
            migrationBuilder.AlterColumn<string>(
                name: "Images",
                schema: "event",
                table: "_EventLanguage",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldDefaultValueSql: "'[]'::jsonb");

            // Fix the default value for Images column in _EventShopItemLanguage table
            migrationBuilder.AlterColumn<string>(
                name: "Images",
                schema: "event",
                table: "_EventShopItemLanguage",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldDefaultValueSql: "'[]'::jsonb");

            // Update existing records that have '[]' to '{}'
            migrationBuilder.Sql(@"
                UPDATE event.""_EventLanguage"" 
                SET ""Images"" = '{}'::jsonb 
                WHERE ""Images"" = '[]'::jsonb;
            ");

            migrationBuilder.Sql(@"
                UPDATE event.""_EventShopItemLanguage"" 
                SET ""Images"" = '{}'::jsonb 
                WHERE ""Images"" = '[]'::jsonb;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert the default value back to array format
            migrationBuilder.AlterColumn<string>(
                name: "Images",
                schema: "event",
                table: "_EventLanguage",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldDefaultValueSql: "'{}'::jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "Images",
                schema: "event",
                table: "_EventShopItemLanguage",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldDefaultValueSql: "'{}'::jsonb");
        }
    }
} 