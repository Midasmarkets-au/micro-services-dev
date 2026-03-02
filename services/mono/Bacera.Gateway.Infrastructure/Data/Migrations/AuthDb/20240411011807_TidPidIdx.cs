using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.AuthDb
{
    /// <inheritdoc />
    public partial class TidPidIdx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "Status",
                schema: "auth",
                table: "_User",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateIndex(
                name: "_User_PartyId_index",
                schema: "auth",
                table: "_User",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "_User_Status_index",
                schema: "auth",
                table: "_User",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "_User_TenantId_index",
                schema: "auth",
                table: "_User",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "_User_TenantId_PartyId_index",
                schema: "auth",
                table: "_User",
                columns: new[] { "TenantId", "PartyId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "_User_PartyId_index",
                schema: "auth",
                table: "_User");

            migrationBuilder.DropIndex(
                name: "_User_Status_index",
                schema: "auth",
                table: "_User");

            migrationBuilder.DropIndex(
                name: "_User_TenantId_index",
                schema: "auth",
                table: "_User");

            migrationBuilder.DropIndex(
                name: "_User_TenantId_PartyId_index",
                schema: "auth",
                table: "_User");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "auth",
                table: "_User");
        }
    }
}
