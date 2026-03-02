using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddIsDeletedToPaymentMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "acct",
                table: "_PaymentMethod",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX__PaymentMethod_IsDeleted",
                schema: "acct",
                table: "_PaymentMethod",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX__PaymentMethod_IsDeleted",
                schema: "acct",
                table: "_PaymentMethod");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "acct",
                table: "_PaymentMethod");
        }
    }
}
