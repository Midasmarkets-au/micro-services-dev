using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class PayoutRecordTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_PayoutRecord",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OperatorPartyId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentMethodId = table.Column<long>(type: "bigint", nullable: false),
                    BatchUid = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BankCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BranchName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccountName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BankNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Info = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_PayoutRecord_pk", x => x.Id);
                    table.ForeignKey(
                        name: "FK__PayoutRecord__Party_OperatorPartyId",
                        column: x => x.OperatorPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_PayoutRecord__PaymentMethod_Id_fk",
                        column: x => x.PaymentMethodId,
                        principalSchema: "acct",
                        principalTable: "_PaymentMethod",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX__PayoutRecord_AccountName",
                schema: "acct",
                table: "_PayoutRecord",
                column: "AccountName");

            migrationBuilder.CreateIndex(
                name: "IX__PayoutRecord_BankCode",
                schema: "acct",
                table: "_PayoutRecord",
                column: "BankCode");

            migrationBuilder.CreateIndex(
                name: "IX__PayoutRecord_BankName",
                schema: "acct",
                table: "_PayoutRecord",
                column: "BankName");

            migrationBuilder.CreateIndex(
                name: "IX__PayoutRecord_BankNumber",
                schema: "acct",
                table: "_PayoutRecord",
                column: "BankNumber");

            migrationBuilder.CreateIndex(
                name: "IX__PayoutRecord_BatchUid",
                schema: "acct",
                table: "_PayoutRecord",
                column: "BatchUid");

            migrationBuilder.CreateIndex(
                name: "IX__PayoutRecord_BranchName",
                schema: "acct",
                table: "_PayoutRecord",
                column: "BranchName");

            migrationBuilder.CreateIndex(
                name: "IX__PayoutRecord_Currency",
                schema: "acct",
                table: "_PayoutRecord",
                column: "Currency");

            migrationBuilder.CreateIndex(
                name: "IX__PayoutRecord_OperatorPartyId",
                schema: "acct",
                table: "_PayoutRecord",
                column: "OperatorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX__PayoutRecord_PaymentMethodId",
                schema: "acct",
                table: "_PayoutRecord",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX__PayoutRecord_Status",
                schema: "acct",
                table: "_PayoutRecord",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_PayoutRecord",
                schema: "acct");
        }
    }
}
