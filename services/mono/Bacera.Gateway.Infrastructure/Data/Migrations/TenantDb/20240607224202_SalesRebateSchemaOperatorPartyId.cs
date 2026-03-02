using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class SalesRebateSchemaOperatorPartyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "trd",
                table: "_SalesRebateSchema",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "OperatorPartyId",
                schema: "trd",
                table: "_SalesRebateSchema",
                type: "bigint",
                nullable: false,
                defaultValueSql: "1");

            migrationBuilder.CreateIndex(
                name: "IX__SalesRebateSchema_OperatorPartyId",
                schema: "trd",
                table: "_SalesRebateSchema",
                column: "OperatorPartyId");

            migrationBuilder.AddForeignKey(
                name: "_SalesRebateSchema_OperatorPartyId_fkey",
                schema: "trd",
                table: "_SalesRebateSchema",
                column: "OperatorPartyId",
                principalSchema: "core",
                principalTable: "_Party",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_SalesRebateSchema_OperatorPartyId_fkey",
                schema: "trd",
                table: "_SalesRebateSchema");

            migrationBuilder.DropIndex(
                name: "IX__SalesRebateSchema_OperatorPartyId",
                schema: "trd",
                table: "_SalesRebateSchema");

            migrationBuilder.DropColumn(
                name: "OperatorPartyId",
                schema: "trd",
                table: "_SalesRebateSchema");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "trd",
                table: "_SalesRebateSchema",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
