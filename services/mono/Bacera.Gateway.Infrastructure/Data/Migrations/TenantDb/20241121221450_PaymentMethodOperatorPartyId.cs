using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class PaymentMethodOperatorPartyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OperatorPartyId",
                schema: "acct",
                table: "_PaymentMethod",
                type: "bigint",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.CreateIndex(
                name: "IX__PaymentMethod_OperatorPartyId",
                schema: "acct",
                table: "_PaymentMethod",
                column: "OperatorPartyId");

            migrationBuilder.AddForeignKey(
                name: "_PaymentMethod__OperatorParty_Id_fk",
                schema: "acct",
                table: "_PaymentMethod",
                column: "OperatorPartyId",
                principalSchema: "core",
                principalTable: "_Party",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_PaymentMethod__OperatorParty_Id_fk",
                schema: "acct",
                table: "_PaymentMethod");

            migrationBuilder.DropIndex(
                name: "IX__PaymentMethod_OperatorPartyId",
                schema: "acct",
                table: "_PaymentMethod");

            migrationBuilder.DropColumn(
                name: "OperatorPartyId",
                schema: "acct",
                table: "_PaymentMethod");
        }
    }
}
