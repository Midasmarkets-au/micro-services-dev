using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AccountReportGroupParent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ParentId",
                schema: "trd",
                table: "_AccountReportGroup",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX__AccountReportGroup_ParentId",
                schema: "trd",
                table: "_AccountReportGroup",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "_AccountReportGroup__Parent_Id_fk",
                schema: "trd",
                table: "_AccountReportGroup",
                column: "ParentId",
                principalSchema: "trd",
                principalTable: "_AccountReportGroup",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_AccountReportGroup__Parent_Id_fk",
                schema: "trd",
                table: "_AccountReportGroup");

            migrationBuilder.DropIndex(
                name: "IX__AccountReportGroup_ParentId",
                schema: "trd",
                table: "_AccountReportGroup");

            migrationBuilder.DropColumn(
                name: "ParentId",
                schema: "trd",
                table: "_AccountReportGroup");
        }
    }
}
