using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.CentralDb
{
    /// <inheritdoc />
    public partial class CentralParty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TradeAccountId",
                schema: "trd",
                table: "_TenantTradeAccount",
                newName: "AccountId");

            migrationBuilder.AddColumn<long>(
                name: "Uid",
                schema: "trd",
                table: "_TenantTradeAccount",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.RenameTable(
                name: "_TenantTradeAccount",
                newName: "_CentralAccount",
                schema: "trd");

            migrationBuilder.CreateTable(
                name: "_CentralParty",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false,
                        defaultValueSql: "now()"),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NativeName = table.Column<string>(type: "text", nullable: false),
                    Uid = table.Column<long>(type: "bigint", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                },
                constraints: table => { table.PrimaryKey("_CentralParty_PrimeKey", x => x.Id); });

            migrationBuilder.CreateIndex(
                name: "_CentralAccount_Ids_Index",
                schema: "trd",
                table: "_CentralAccount",
                columns: new[] { "TenantId", "ServiceId", "AccountId", "AccountNumber" });

            migrationBuilder.CreateIndex(
                name: "_CentralAccount_Uid_Index",
                schema: "trd",
                table: "_CentralAccount",
                column: "Uid");

            migrationBuilder.CreateIndex(
                name: "_CentralParty_Code_Index",
                schema: "core",
                table: "_CentralParty",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "_CentralParty_Email_Index",
                schema: "core",
                table: "_CentralParty",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "_CentralParty_NativeName_Index",
                schema: "core",
                table: "_CentralParty",
                column: "NativeName");

            migrationBuilder.CreateIndex(
                name: "_CentralParty_SiteId_Index",
                schema: "core",
                table: "_CentralParty",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "_CentralParty_Uid_Index",
                schema: "core",
                table: "_CentralParty",
                column: "Uid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}