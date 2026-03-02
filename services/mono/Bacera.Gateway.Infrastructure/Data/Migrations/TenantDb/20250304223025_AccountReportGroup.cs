using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AccountReportGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_AccountReportGroup",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Group = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MetaData = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    OperatorPartyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_AccountReportGroup_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_AccountReportGroup__Operator_Party_Id_fk",
                        column: x => x.OperatorPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_AccountReportGroupLogin",
                schema: "trd",
                columns: table => new
                {
                    AccountReportGroupId = table.Column<long>(type: "bigint", nullable: false),
                    Login = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("account_report_group_logins_pkey", x => new { x.AccountReportGroupId, x.Login });
                    table.ForeignKey(
                        name: "_AccountReportGroupLogin__AccountReportGroup_Id_fk",
                        column: x => x.AccountReportGroupId,
                        principalSchema: "trd",
                        principalTable: "_AccountReportGroup",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX__AccountReportGroup_Category",
                schema: "trd",
                table: "_AccountReportGroup",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX__AccountReportGroup_Group",
                schema: "trd",
                table: "_AccountReportGroup",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "IX__AccountReportGroup_OperatorPartyId",
                schema: "trd",
                table: "_AccountReportGroup",
                column: "OperatorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX__AccountReportGroupLogin_AccountReportGroupId",
                schema: "trd",
                table: "_AccountReportGroupLogin",
                column: "AccountReportGroupId");

            migrationBuilder.CreateIndex(
                name: "IX__AccountReportGroupLogin_Login",
                schema: "trd",
                table: "_AccountReportGroupLogin",
                column: "Login");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_AccountReportGroupLogin",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_AccountReportGroup",
                schema: "trd");
        }
    }
}
