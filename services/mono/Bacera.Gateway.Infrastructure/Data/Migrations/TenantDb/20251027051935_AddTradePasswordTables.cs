using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddTradePasswordTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_TradeAccountPassword",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    AccountNumber = table.Column<long>(type: "bigint", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    InitialMainPassword = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InitialInvestorPassword = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InitialPhonePassword = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MainPasswordChangedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    InvestorPasswordChangedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastMainPasswordChangedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastInvestorPasswordChangedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_TradeAccountPassword_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradeAccountPassword_Account",
                        column: x => x.AccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TradeAccountPassword_TradeService",
                        column: x => x.ServiceId,
                        principalSchema: "trd",
                        principalTable: "_TradeService",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_TradeAccountPasswordHistory",
                schema: "trd",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    AccountNumber = table.Column<long>(type: "bigint", nullable: false),
                    PasswordType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OperationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OperatorPartyId = table.Column<long>(type: "bigint", nullable: true),
                    OperatorRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Success = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ChangedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TradeAccountPasswordId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_TradeAccountPasswordHistory_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradeAccountPasswordHistory_Account",
                        column: x => x.AccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TradeAccountPasswordHistory_OperatorParty",
                        column: x => x.OperatorPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK__TradeAccountPasswordHistory__TradeAccountPassword_TradeAcc~",
                        column: x => x.TradeAccountPasswordId,
                        principalSchema: "trd",
                        principalTable: "_TradeAccountPassword",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TradeAccountPassword_AccountId",
                schema: "trd",
                table: "_TradeAccountPassword",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeAccountPassword_AccountNumber",
                schema: "trd",
                table: "_TradeAccountPassword",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TradeAccountPassword_ServiceId",
                schema: "trd",
                table: "_TradeAccountPassword",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX__TradeAccountPasswordHistory_TradeAccountPasswordId",
                schema: "trd",
                table: "_TradeAccountPasswordHistory",
                column: "TradeAccountPasswordId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeAccountPasswordHistory_AccountId",
                schema: "trd",
                table: "_TradeAccountPasswordHistory",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeAccountPasswordHistory_AccountId_PasswordType",
                schema: "trd",
                table: "_TradeAccountPasswordHistory",
                columns: new[] { "AccountId", "PasswordType" });

            migrationBuilder.CreateIndex(
                name: "IX_TradeAccountPasswordHistory_ChangedOn",
                schema: "trd",
                table: "_TradeAccountPasswordHistory",
                column: "ChangedOn",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_TradeAccountPasswordHistory_OperatorPartyId",
                schema: "trd",
                table: "_TradeAccountPasswordHistory",
                column: "OperatorPartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_TradeAccountPasswordHistory",
                schema: "trd");

            migrationBuilder.DropTable(
                name: "_TradeAccountPassword",
                schema: "trd");
        }
    }
}
