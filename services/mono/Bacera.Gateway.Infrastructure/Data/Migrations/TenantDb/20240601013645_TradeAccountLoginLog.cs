using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class TradeAccountLoginLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_TradeAccountLoginLog",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountNumber = table.Column<long>(type: "bigint", nullable: false),
                    Ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "''::character varying"),
                    LoginTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("TradeAccountLoginLog_pkey", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "_TradeAccountLoginLog_AccountNumber_index",
                schema: "trd",
                table: "_TradeAccountLoginLog",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "_TradeAccountLoginLog_Ip_index",
                schema: "trd",
                table: "_TradeAccountLoginLog",
                column: "Ip");

            migrationBuilder.CreateIndex(
                name: "_TradeAccountLoginLog_LoginTime_index",
                schema: "trd",
                table: "_TradeAccountLoginLog",
                column: "LoginTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_TradeAccountLoginLog",
                schema: "trd");
        }
    }
}
