using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class RemoveSalesRebateSchemaAlphaRebateDftVal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AlphaRebate",
                schema: "trd",
                table: "_SalesRebateSchema",
                type: "integer",
                nullable: false,
                defaultValueSql: "0",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValueSql: "50");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AlphaRebate",
                schema: "trd",
                table: "_SalesRebateSchema",
                type: "integer",
                nullable: false,
                defaultValueSql: "50",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValueSql: "0");
        }
    }
}
