using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddAccountStatTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_AccountStat",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    NewAccountCount = table.Column<long>(type: "bigint", nullable: false),
                    NewAgentCount = table.Column<long>(type: "bigint", nullable: false),
                    DepositAmount = table.Column<long>(type: "bigint", nullable: false),
                    DepositCount = table.Column<long>(type: "bigint", nullable: false),
                    WithdrawAmount = table.Column<long>(type: "bigint", nullable: false),
                    WithdrawCount = table.Column<long>(type: "bigint", nullable: false),
                    TradeVolume = table.Column<long>(type: "bigint", nullable: false),
                    TradeSymbol = table.Column<string>(type: "text", nullable: false),
                    RebateAmount = table.Column<long>(type: "bigint", nullable: false),
                    RebateCount = table.Column<long>(type: "bigint", nullable: false),
                    TradeProfit = table.Column<long>(type: "bigint", nullable: false),
                    TradeCount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_AccountStat_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_AccountStat__Account_Id_fk",
                        column: x => x.AccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_WalletAdjust",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WalletId = table.Column<long>(type: "bigint", nullable: false),
                    SourceType = table.Column<short>(type: "smallint", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    Comment = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false,
                        defaultValueSql: "''::character varying")
                },
                constraints: table =>
                {
                    table.PrimaryKey("wallet_adjust_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "acct_wallet_adjusts_wallet_id_foreign",
                        column: x => x.WalletId,
                        principalSchema: "acct",
                        principalTable: "_Wallet",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX__AccountStat_AccountId",
                schema: "trd",
                table: "_AccountStat",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountStats_AccountId_Date",
                schema: "trd",
                table: "_AccountStat",
                columns: new[] { "AccountId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: " IX_wallet_adjusts_source_type",
                schema: "acct",
                table: "_WalletAdjust",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_adjusts_wallet_id",
                schema: "acct",
                table: "_WalletAdjust",
                column: "WalletId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_AccountStat",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_WalletAdjust",
                schema: "acct");
        }
    }
}