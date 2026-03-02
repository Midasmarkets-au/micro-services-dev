using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class CommentEventShopOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                schema: "event",
                table: "_EventShopOrder",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                schema: "event",
                table: "_EventShopOrder");
        }
    }
}
