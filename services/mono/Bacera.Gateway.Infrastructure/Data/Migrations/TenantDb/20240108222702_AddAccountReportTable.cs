using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddAccountReportTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Context",
                schema: "sto",
                table: "_Medium",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValueSql: "''::character varying",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.CreateTable(
                name: "_AccountReport",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    AccountNumber = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFile = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    TryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Tries = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_AccountReport_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_AccountReport__Account_Id_fk",
                        column: x => x.AccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_AccountReport__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX__ReferralCode_AccountId",
                schema: "core",
                table: "_ReferralCode",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX__AccountReport_AccountId",
                schema: "trd",
                table: "_AccountReport",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX__AccountReport_AccountNumber",
                schema: "trd",
                table: "_AccountReport",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX__AccountReport_Date",
                schema: "trd",
                table: "_AccountReport",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX__AccountReport_PartyId",
                schema: "trd",
                table: "_AccountReport",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX__AccountReport_ServiceId",
                schema: "trd",
                table: "_AccountReport",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX__AccountReport_Status",
                schema: "trd",
                table: "_AccountReport",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX__AccountReport_TenantId",
                schema: "trd",
                table: "_AccountReport",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX__AccountReport_Tries",
                schema: "trd",
                table: "_AccountReport",
                column: "Tries");

            migrationBuilder.CreateIndex(
                name: "IX__AccountReport_TryTime",
                schema: "trd",
                table: "_AccountReport",
                column: "TryTime");

            migrationBuilder.CreateIndex(
                name: "IX_AccountNumber_ServiceId_Date_Unique",
                schema: "trd",
                table: "_AccountReport",
                columns: new[] { "AccountNumber", "ServiceId", "Date" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "core_referral_codes_account_id_foreign",
                schema: "core",
                table: "_ReferralCode",
                column: "AccountId",
                principalSchema: "trd",
                principalTable: "_Account",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "core_referral_codes_account_id_foreign",
                schema: "core",
                table: "_ReferralCode");

            migrationBuilder.DropTable(
                name: "_AccountReport",
                schema: "trd");

            migrationBuilder.DropIndex(
                name: "IX__ReferralCode_AccountId",
                schema: "core",
                table: "_ReferralCode");

            migrationBuilder.AlterColumn<string>(
                name: "Context",
                schema: "sto",
                table: "_Medium",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldDefaultValueSql: "''::character varying");
        }
    }
}
