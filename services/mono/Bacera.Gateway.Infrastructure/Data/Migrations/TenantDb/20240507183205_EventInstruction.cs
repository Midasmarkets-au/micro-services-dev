using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class EventInstruction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX__EventLanguage_Description",
                schema: "event",
                table: "_EventLanguage");

            migrationBuilder.AddColumn<string>(
                name: "Instruction",
                schema: "event",
                table: "_EventLanguage",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Instruction",
                schema: "event",
                table: "_EventLanguage");

            migrationBuilder.CreateIndex(
                name: "IX__EventLanguage_Description",
                schema: "event",
                table: "_EventLanguage",
                column: "Description");
        }
    }
}
