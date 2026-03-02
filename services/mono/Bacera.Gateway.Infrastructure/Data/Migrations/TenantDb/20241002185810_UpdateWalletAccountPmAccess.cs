using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class UpdateWalletAccountPmAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropTable(
            //     name: "_PaymentMethodAccess",
            //     schema: "acct");

            migrationBuilder.CreateTable(
                name: "_AccountPaymentMethodAccess",
                schema: "acct",
                columns: table => new
                {
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentMethodId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    OperatedPartyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("account_payment_method_access_pkey", x => new { x.AccountId, x.PaymentMethodId });
                    table.ForeignKey(
                        name: "acct_account_payment_method_access_account_id_foreign",
                        column: x => x.AccountId,
                        principalSchema: "trd",
                        principalTable: "_Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "acct_account_payment_method_access_operated_party_id_foreign",
                        column: x => x.OperatedPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "acct_account_payment_method_access_payment_method_id_foreign",
                        column: x => x.PaymentMethodId,
                        principalSchema: "acct",
                        principalTable: "_PaymentMethod",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_WalletPaymentMethodAccess",
                schema: "acct",
                columns: table => new
                {
                    WalletId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentMethodId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    OperatedPartyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("wallet_payment_method_access_pkey", x => new { x.WalletId, x.PaymentMethodId });
                    table.ForeignKey(
                        name: "acct_wallet_payment_method_access_operated_party_id_foreign",
                        column: x => x.OperatedPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "acct_wallet_payment_method_access_payment_method_id_foreign",
                        column: x => x.PaymentMethodId,
                        principalSchema: "acct",
                        principalTable: "_PaymentMethod",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "acct_wallet_payment_method_access_wallet_id_foreign",
                        column: x => x.WalletId,
                        principalSchema: "acct",
                        principalTable: "_Wallet",
                        principalColumn: "Id");
                });

            // migrationBuilder.CreateIndex(
            //     name: "IX__Comment_PartyId",
            //     schema: "core",
            //     table: "_Comment",
            //     column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_account_payment_method_access_status",
                table: "_AccountPaymentMethodAccess",
                schema: "acct",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AccountPaymentMethodAccesses_OperatedPartyId",
                table: "_AccountPaymentMethodAccess",
                schema: "acct",
                column: "OperatedPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountPaymentMethodAccesses_PaymentMethodId",
                table: "_AccountPaymentMethodAccess",
                schema: "acct",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_payment_method_access_status",
                table: "_WalletPaymentMethodAccess",
                schema: "acct",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WalletPaymentMethodAccesses_OperatedPartyId",
                table: "_WalletPaymentMethodAccess",
                schema: "acct",
                column: "OperatedPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletPaymentMethodAccesses_PaymentMethodId",
                table: "_WalletPaymentMethodAccess",
                schema: "acct",
                column: "PaymentMethodId");

            // migrationBuilder.AddForeignKey(
            //     name: "_Comment__Party_Id_fk",
            //     schema: "core",
            //     table: "_Comment",
            //     column: "PartyId",
            //     principalSchema: "core",
            //     principalTable: "_Party",
            //     principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_Comment__Party_Id_fk",
                schema: "core",
                table: "_Comment");

            migrationBuilder.DropTable(
                name: "_AccountPaymentMethodAccess");

            migrationBuilder.DropTable(
                name: "_WalletPaymentMethodAccess");

            migrationBuilder.DropIndex(
                name: "IX__Comment_PartyId",
                schema: "core",
                table: "_Comment");

            migrationBuilder.CreateTable(
                name: "_PaymentMethodAccess",
                schema: "acct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartyId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentMethodId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Model = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false, defaultValueSql: "'account'::character varying"),
                    RowId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_PaymentMethodAccess_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "_PaymentMethodAccess__PartyId_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_PaymentMethodAccess__PaymentMethod_Id_fk",
                        column: x => x.PaymentMethodId,
                        principalSchema: "acct",
                        principalTable: "_PaymentMethod",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "_PaymentMethodAccess_Model_RowId_index",
                schema: "acct",
                table: "_PaymentMethodAccess",
                columns: new[] { "Model", "RowId" });

            migrationBuilder.CreateIndex(
                name: "_PaymentMethodAccess_PartyId_index",
                schema: "acct",
                table: "_PaymentMethodAccess",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "_PaymentMethodAccess_PartyId_Model_RowId_index",
                schema: "acct",
                table: "_PaymentMethodAccess",
                columns: new[] { "PartyId", "Model", "RowId" });

            migrationBuilder.CreateIndex(
                name: "IX__PaymentMethodAccess_PaymentMethodId",
                schema: "acct",
                table: "_PaymentMethodAccess",
                column: "PaymentMethodId");
        }
    }
}
