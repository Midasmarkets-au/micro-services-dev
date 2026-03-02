using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class SalesRebateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add missing WalletAdjustId column to existing _SalesRebate table
            migrationBuilder.AddColumn<long>(
                name: "WalletAdjustId",
                schema: "trd",
                table: "_SalesRebate",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // Add missing TradeAccountId foreign key constraint
            migrationBuilder.CreateIndex(
                name: "IX__SalesRebate_TradeAccountId",
                schema: "trd",
                table: "_SalesRebate",
                column: "TradeAccountId");

            migrationBuilder.AddForeignKey(
                name: "_SalesRebate_TradeAccountId_fk",
                schema: "trd",
                table: "_SalesRebate",
                column: "TradeAccountId",
                principalSchema: "trd",
                principalTable: "_Account",
                principalColumn: "Id");

            // Update _SalesRebateSchema table structure
            migrationBuilder.DropTable(
                name: "_SalesRebateSchema",
                schema: "trd");

            migrationBuilder.CreateTable(
                name: "_SalesRebateSchema",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalesAccountId = table.Column<long>(type: "bigint", nullable: false),
                    RebateAccountId = table.Column<long>(type: "bigint", nullable: false),
                    Rebate = table.Column<int>(type: "integer", nullable: false),
                    ExcludeAccount = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'[]'::json"),
                    ExcludeSymbol = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'[]'::json"),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Note = table.Column<string>(type: "text", nullable: true),
                    AlphaRebate = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "50")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_SalesRebateSchema_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_SalesRebateSchema_RebateAccountId_fkey",
                        column: x => x.RebateAccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_SalesRebateSchema_SalesAccountId_fkey",
                        column: x => x.SalesAccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                });



            migrationBuilder.CreateIndex(
                name: "IX__SalesRebateSchema_RebateAccountId",
                schema: "trd",
                table: "_SalesRebateSchema",
                column: "RebateAccountId");

            migrationBuilder.CreateIndex(
                name: "IX__SalesRebateSchema_SalesAccountId",
                schema: "trd",
                table: "_SalesRebateSchema",
                column: "SalesAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the added column and constraints
            migrationBuilder.DropForeignKey(
                name: "_SalesRebate_TradeAccountId_fk",
                schema: "trd",
                table: "_SalesRebate");

            migrationBuilder.DropIndex(
                name: "IX__SalesRebate_TradeAccountId",
                schema: "trd",
                table: "_SalesRebate");

            migrationBuilder.DropColumn(
                name: "WalletAdjustId",
                schema: "trd",
                table: "_SalesRebate");

            migrationBuilder.DropTable(
                name: "_SalesRebateSchema",
                schema: "trd");

            // Recreate original _SalesRebateSchema structure
            migrationBuilder.CreateTable(
                name: "_SalesRebateSchema",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalesAccountId = table.Column<long>(type: "bigint", nullable: false),
                    DirectAgent = table.Column<int>(type: "integer", nullable: false),
                    DirectClient = table.Column<int>(type: "integer", nullable: false),
                    DirectSales = table.Column<int>(type: "integer", nullable: false),
                    OtherSales = table.Column<int>(type: "integer", nullable: false),
                    ExcludeAccount = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'[]'::json"),
                    ExcludeSymbol = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'[]'::json"),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    Note = table.Column<string>(type: "text", nullable: true),
                    PartyId = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_SalesRebateSchema_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_SalesRebateSchema_SalesAccountId_fkey",
                        column: x => x.SalesAccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "_SalesRebateSchema_SalesAccountId_uindex",
                schema: "trd",
                table: "_SalesRebateSchema",
                column: "SalesAccountId",
                unique: true);
        }
    }
}
