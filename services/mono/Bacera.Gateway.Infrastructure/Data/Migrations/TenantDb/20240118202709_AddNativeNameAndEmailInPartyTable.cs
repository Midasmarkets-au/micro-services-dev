using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddNativeNameAndEmailInPartyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "core",
                table: "_Party",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NativeName",
                schema: "core",
                table: "_Party",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Uid",
                schema: "core",
                table: "_Party",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "core_parties_email_index",
                schema: "core",
                table: "_Party",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "core_parties_native_name_index",
                schema: "core",
                table: "_Party",
                column: "NativeName");

            migrationBuilder.CreateIndex(
                name: "core_parties_uid_index",
                schema: "core",
                table: "_Party",
                column: "Uid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "core_parties_email_index",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropIndex(
                name: "core_parties_native_name_index",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "NativeName",
                schema: "core",
                table: "_Party");

            migrationBuilder.DropColumn(
                name: "Uid",
                schema: "core",
                table: "_Party");
        }
    }
}