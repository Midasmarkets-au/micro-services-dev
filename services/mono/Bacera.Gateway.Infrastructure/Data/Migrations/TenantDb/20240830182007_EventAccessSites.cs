using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class EventAccessSites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessSites",
                schema: "event",
                table: "_Event",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.CreateIndex(
                name: "IX__Event_AccessSites",
                schema: "event",
                table: "_Event",
                column: "AccessSites");

            migrationBuilder.CreateIndex(
                name: "acct_crypto_transactions_created_on_index",
                schema: "acct",
                table: "_CryptoTransaction",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "acct_crypto_transactions_transaction_hash_index",
                schema: "acct",
                table: "_CryptoTransaction",
                column: "TransactionHash");

            migrationBuilder.AddForeignKey(
                name: "_AuthCode__Party_Id_fk",
                schema: "core",
                table: "_AuthCode",
                column: "PartyId",
                principalSchema: "core",
                principalTable: "_Party",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_AuthCode__Party_Id_fk",
                schema: "core",
                table: "_AuthCode");

            migrationBuilder.DropIndex(
                name: "IX__Event_AccessSites",
                schema: "event",
                table: "_Event");

            migrationBuilder.DropIndex(
                name: "acct_crypto_transactions_created_on_index",
                schema: "acct",
                table: "_CryptoTransaction");

            migrationBuilder.DropIndex(
                name: "acct_crypto_transactions_transaction_hash_index",
                schema: "acct",
                table: "_CryptoTransaction");

            migrationBuilder.DropColumn(
                name: "AccessSites",
                schema: "event",
                table: "_Event");
        }
    }
}
