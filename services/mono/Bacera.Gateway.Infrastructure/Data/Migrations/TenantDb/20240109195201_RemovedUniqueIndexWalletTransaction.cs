using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class RemovedUniqueIndexWalletTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "_WalletTransaction_WalletId_MatterId_uindex",
                schema: "acct",
                table: "_WalletTransaction");

            migrationBuilder.AlterColumn<string>(
                name: "Group",
                schema: "trd",
                table: "_TradeAccountStatus",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldDefaultValueSql: "''::character varying");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Group",
                schema: "trd",
                table: "_TradeAccountStatus",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValueSql: "''::character varying",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "_WalletTransaction_WalletId_MatterId_uindex",
                schema: "acct",
                table: "_WalletTransaction",
                columns: new[] { "WalletId", "MatterId" },
                unique: true);
        }
    }
}
