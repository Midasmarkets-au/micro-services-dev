using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class UpdateColNameImagesEventShopItemLanguages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                schema: "event",
                table: "_EventShopItemLanguage");

            migrationBuilder.AddColumn<string>(
                name: "Images",
                schema: "event",
                table: "_EventShopItemLanguage",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Images",
                schema: "event",
                table: "_EventShopItemLanguage");

            migrationBuilder.AddColumn<string>(
                name: "Image",
                schema: "event",
                table: "_EventShopItemLanguage",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");
        }
    }
}
