using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddSalesRebate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_SalesRebate",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TradeRebateId = table.Column<long>(type: "bigint", nullable: false),
                    SalesAccountId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    TradeAccountType = table.Column<short>(type: "smallint", nullable: false),
                    TradeAccountFundType = table.Column<int>(type: "integer", nullable: false),
                    TradeAccountCurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false,
                        defaultValueSql: "now()"),
                    Note = table.Column<string>(type: "text", nullable: true),
                    TradeAccountId = table.Column<long>(type: "bigint", nullable: false),
                    TradeAccountNumber = table.Column<long>(type: "bigint", nullable: false),
                    RebateType = table.Column<string>(type: "character varying", nullable: false,
                        defaultValueSql: "'unknow'::character varying"),
                    RebateBase = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_SalesRebate_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_SalesRebate_SalesAccountId_fkey",
                        column: x => x.SalesAccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_SalesRebate_TradeRebateId_fkey",
                        column: x => x.TradeRebateId,
                        principalSchema: "trd",
                        principalTable: "_TradeRebate",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_SalesRebateSchema",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalesAccountId = table.Column<long>(type: "bigint", nullable: false),
                    DirectAgent = table.Column<int>(type: "integer", nullable: false),
                    DirectClient = table.Column<int>(type: "integer", nullable: false),
                    DirectSales = table.Column<int>(type: "integer", nullable: false),
                    OtherSales = table.Column<int>(type: "integer", nullable: false),
                    ExcludeAccount = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'[]'::json"),
                    ExcludeSymbol = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'[]'::json"),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false,
                        defaultValueSql: "now()"),
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
                name: "_SalesRebate_SalesAccountId_index",
                schema: "trd",
                table: "_SalesRebate",
                column: "SalesAccountId");

            migrationBuilder.CreateIndex(
                name: "IX__SalesRebate_TradeRebateId",
                schema: "trd",
                table: "_SalesRebate",
                column: "TradeRebateId");

            migrationBuilder.CreateIndex(
                name: "_SalesRebateSchema_SalesAccountId_uindex",
                schema: "trd",
                table: "_SalesRebateSchema",
                column: "SalesAccountId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_SalesRebate",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_SalesRebateSchema",
                schema: "trd");
        }
    }
}