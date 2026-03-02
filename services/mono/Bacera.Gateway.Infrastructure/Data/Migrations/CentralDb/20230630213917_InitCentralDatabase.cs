using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.CentralDb
{
    /// <inheritdoc />
    public partial class InitCentralDatabase : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.EnsureSchema(
                name: "trd");

            migrationBuilder.CreateTable(
                name: "_Tenant",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DatabaseName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ApiLogEnable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("tenant_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_Domain",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    DomainName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("domain_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "core_domain_tenant_id_foreign",
                        column: x => x.TenantId,
                        principalSchema: "core",
                        principalTable: "_Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "_TenantTradeAccount",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'10000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    TradeAccountId = table.Column<long>(type: "bigint", nullable: false),
                    AccountNumber = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_TenantTradeAccount_PrimeKey", x => x.Id);
                    table.ForeignKey(
                        name: "_TenantTradeAccount_TenantId_Foreign",
                        column: x => x.TenantId,
                        principalSchema: "core",
                        principalTable: "_Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "core",
                table: "_Tenant",
                columns: new[] { "Id", "DatabaseName", "Name" },
                values: new object[] { 1L, "portal_tenant_1", "Demo Tenant" });

            migrationBuilder.InsertData(
                schema: "core",
                table: "_Domain",
                columns: new[] { "Id", "DomainName", "TenantId" },
                values: new object[] { 1L, "demo.localhost", 1L });

            migrationBuilder.CreateIndex(
                name: "IX_domains_tenant_id",
                schema: "core",
                table: "_Domain",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "UX_Domain_DomainName",
                schema: "core",
                table: "_Domain",
                column: "DomainName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "_TenantTradeAccount_Ids_Index",
                schema: "trd",
                table: "_TenantTradeAccount",
                columns: new[] { "TenantId", "ServiceId", "TradeAccountId", "AccountNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_Domain",
                schema: "core");

            migrationBuilder.DropTable(
                name: "_TenantTradeAccount",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_Tenant",
                schema: "core");
        }
    }
}
