using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddDailyEquitySnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "rpt");

            migrationBuilder.CreateTable(
                name: "_DailyEquitySnapshot",
                schema: "rpt",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReportDate = table.Column<DateTime>(type: "date", nullable: false),
                    ReportVersion = table.Column<int>(type: "integer", nullable: false),
                    Office = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    NewUser = table.Column<long>(type: "bigint", nullable: false),
                    NewAcc = table.Column<long>(type: "bigint", nullable: false),
                    PreviousEquity = table.Column<decimal>(type: "numeric", nullable: false),
                    CurrentEquity = table.Column<decimal>(type: "numeric", nullable: false),
                    MarginIn = table.Column<decimal>(type: "numeric", nullable: false),
                    MarginOut = table.Column<decimal>(type: "numeric", nullable: false),
                    Transfer = table.Column<decimal>(type: "numeric", nullable: false),
                    Credit = table.Column<decimal>(type: "numeric", nullable: false),
                    Adjust = table.Column<decimal>(type: "numeric", nullable: false),
                    Rebate = table.Column<decimal>(type: "numeric", nullable: false),
                    NetInOut = table.Column<decimal>(type: "numeric", nullable: false),
                    Lots = table.Column<decimal>(type: "numeric", nullable: false),
                    PL = table.Column<decimal>(type: "numeric", nullable: false),
                    EstimatesNetPL = table.Column<decimal>(type: "numeric", nullable: false),
                    AdditionalInfo = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_DailyEquitySnapshot_pk", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyEquitySnapshot_Date_Version_Office_Currency",
                schema: "rpt",
                table: "_DailyEquitySnapshot",
                columns: new[] { "ReportDate", "ReportVersion", "Office", "Currency" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_DailyEquitySnapshot",
                schema: "rpt");
        }
    }
}
