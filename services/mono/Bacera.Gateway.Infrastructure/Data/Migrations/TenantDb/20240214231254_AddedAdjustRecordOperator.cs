using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddedAdjustRecordOperator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OperatorPartyId",
                schema: "trd",
                table: "_AdjustRecord",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX__AdjustRecord_OperatorPartyId",
                schema: "trd",
                table: "_AdjustRecord",
                column: "OperatorPartyId");

            migrationBuilder.AddForeignKey(
                name: "_AdjustRecord__Operator_Party_Id_fk",
                schema: "trd",
                table: "_AdjustRecord",
                column: "OperatorPartyId",
                principalSchema: "core",
                principalTable: "_Party",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_AdjustRecord__Operator_Party_Id_fk",
                schema: "trd",
                table: "_AdjustRecord");

            migrationBuilder.DropIndex(
                name: "IX__AdjustRecord_OperatorPartyId",
                schema: "trd",
                table: "_AdjustRecord");

            migrationBuilder.DropColumn(
                name: "OperatorPartyId",
                schema: "trd",
                table: "_AdjustRecord");
        }
    }
}