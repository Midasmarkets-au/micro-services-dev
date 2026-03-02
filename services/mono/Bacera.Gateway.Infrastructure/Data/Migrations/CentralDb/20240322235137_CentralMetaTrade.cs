using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.CentralDb
{
    /// <inheritdoc />
    public partial class CentralMetaTrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_MetaTrade",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    AccountNumber = table.Column<long>(type: "bigint", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    Ticket = table.Column<long>(type: "bigint", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Cmd = table.Column<int>(type: "integer", nullable: false),
                    OpenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CloseAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeStamp = table.Column<long>(type: "bigint", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: true),
                    Digits = table.Column<int>(type: "integer", nullable: false),
                    Volume = table.Column<double>(type: "double precision", nullable: false),
                    OpenPrice = table.Column<double>(type: "double precision", nullable: true),
                    Sl = table.Column<double>(type: "double precision", nullable: false),
                    Tp = table.Column<double>(type: "double precision", nullable: false),
                    ClosePrice = table.Column<double>(type: "double precision", nullable: true),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    Profit = table.Column<double>(type: "double precision", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    CurrentPrice = table.Column<double>(type: "double precision", nullable: false),
                    Commission = table.Column<double>(type: "double precision", nullable: false),
                    Swaps = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("meta_trade_trade_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "trd_meta_trade_trade_tenant_id_foreign",
                        column: x => x.TenantId,
                        principalSchema: "core",
                        principalTable: "_Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_meta_trade_trade_account_number",
                schema: "trd",
                table: "_MetaTrade",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_meta_trade_trade_close_at",
                schema: "trd",
                table: "_MetaTrade",
                column: "CloseAt");

            migrationBuilder.CreateIndex(
                name: "IX_meta_trade_trade_cmd",
                schema: "trd",
                table: "_MetaTrade",
                column: "Cmd");

            migrationBuilder.CreateIndex(
                name: "IX_meta_trade_trade_open_at",
                schema: "trd",
                table: "_MetaTrade",
                column: "OpenAt");

            migrationBuilder.CreateIndex(
                name: "IX_meta_trade_trade_service_id",
                schema: "trd",
                table: "_MetaTrade",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_meta_trade_trade_service_id_ticket",
                schema: "trd",
                table: "_MetaTrade",
                columns: new[] { "ServiceId", "Ticket" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_meta_trade_trade_symbol",
                schema: "trd",
                table: "_MetaTrade",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_meta_trade_trade_tenant_id",
                schema: "trd",
                table: "_MetaTrade",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_meta_trade_trade_ticket",
                schema: "trd",
                table: "_MetaTrade",
                column: "Ticket");

            migrationBuilder.CreateIndex(
                name: "IX_meta_trade_trade_time_stamp",
                schema: "trd",
                table: "_MetaTrade",
                column: "TimeStamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_MetaTrade",
                schema: "trd");
        }
    }
}