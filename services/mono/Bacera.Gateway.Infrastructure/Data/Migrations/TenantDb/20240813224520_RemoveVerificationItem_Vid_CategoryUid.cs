using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class RemoveVerificationItem_Vid_CategoryUid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "_VerificationItem_Vid_Category_pk",
                schema: "core",
                table: "_VerificationItem");

            migrationBuilder.CreateIndex(
                name: "IX__VerificationItem_VerificationId",
                schema: "core",
                table: "_VerificationItem",
                column: "VerificationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX__VerificationItem_VerificationId",
                schema: "core",
                table: "_VerificationItem");

            migrationBuilder.CreateIndex(
                name: "_VerificationItem_Vid_Category_pk",
                schema: "core",
                table: "_VerificationItem",
                columns: new[] { "VerificationId", "Category" },
                unique: true);
        }
    }
}
