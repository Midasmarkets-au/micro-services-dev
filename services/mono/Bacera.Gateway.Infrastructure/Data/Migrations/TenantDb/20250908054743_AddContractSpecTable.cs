using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddContractSpecTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contract_specs",
                schema: "cms",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    site = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    category = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    symbol = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    contract_size = table.Column<int>(type: "integer", nullable: true),
                    contract_unit = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    trading_start_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    trading_end_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    trading_start_weekday = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    trading_end_weekday = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    break_start_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    break_end_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    more_break_start_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    more_break_end_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    margin_requirements = table.Column<string>(type: "json", nullable: true),
                    commission = table.Column<int>(type: "integer", nullable: true),
                    rollover_time = table.Column<TimeSpan>(type: "interval", nullable: true),
                    comment = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    operator_info = table.Column<string>(type: "json", nullable: true),
                    description_langs = table.Column<string>(type: "json", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contract_specs", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contract_specs",
                schema: "cms");
        }
    }
}
