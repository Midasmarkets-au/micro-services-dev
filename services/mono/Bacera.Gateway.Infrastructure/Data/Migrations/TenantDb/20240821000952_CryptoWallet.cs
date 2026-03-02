using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class CryptoWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_Crypto",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    OperatorPartyId = table.Column<long>(type: "bigint", nullable: false),
                    InUsePaymentId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("crypto_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "acct_crypto_in_use_payment_id_foreign",
                        column: x => x.InUsePaymentId,
                        principalSchema: "acct",
                        principalTable: "_Payment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "acct_crypto_operator_party_id_foreign",
                        column: x => x.OperatorPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_CryptoTransaction",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CryptoId = table.Column<long>(type: "bigint", nullable: false),
                    Confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PaymentId = table.Column<long>(type: "bigint", nullable: true),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    FromAddress = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    TransactionHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("crypto_transactions_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "acct_crypto_transactions_crypto_id_foreign",
                        column: x => x.CryptoId,
                        principalSchema: "acct",
                        principalTable: "_Crypto",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "acct_crypto_transactions_payment_id_foreign",
                        column: x => x.PaymentId,
                        principalSchema: "acct",
                        principalTable: "_Payment",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "acct_crypto_address_unique",
                schema: "acct",
                table: "_Crypto",
                column: "Address",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "acct_crypto_status_index",
                schema: "acct",
                table: "_Crypto",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "acct_crypto_type_index",
                schema: "acct",
                table: "_Crypto",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX__Crypto_InUsePaymentId",
                schema: "acct",
                table: "_Crypto",
                column: "InUsePaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__Crypto_OperatorPartyId",
                schema: "acct",
                table: "_Crypto",
                column: "OperatorPartyId");

            migrationBuilder.CreateIndex(
                name: "acct_crypto_transactions_confirmed_index",
                schema: "acct",
                table: "_CryptoTransaction",
                column: "Confirmed");

            migrationBuilder.CreateIndex(
                name: "acct_crypto_transactions_from_address_index",
                schema: "acct",
                table: "_CryptoTransaction",
                column: "FromAddress");

            migrationBuilder.CreateIndex(
                name: "acct_crypto_transactions_status_index",
                schema: "acct",
                table: "_CryptoTransaction",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX__CryptoTransaction_CryptoId",
                schema: "acct",
                table: "_CryptoTransaction",
                column: "CryptoId");

            migrationBuilder.CreateIndex(
                name: "IX__CryptoTransaction_PaymentId",
                schema: "acct",
                table: "_CryptoTransaction",
                column: "PaymentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_CryptoTransaction",
                schema: "acct");

            migrationBuilder.DropTable(
                name: "_Crypto",
                schema: "acct");
        }
    }
}