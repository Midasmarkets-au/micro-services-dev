using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddSalesRebateSchemaAutoRelease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "_SalesRebate_TradeRebateId_fkey",
                schema: "trd",
                table: "_SalesRebate");

            migrationBuilder.AddColumn<bool>(
                name: "AutoRelease",
                schema: "trd",
                table: "_SalesRebateSchema",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "Ticket",
                schema: "trd",
                table: "_SalesRebate",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // Tables below are managed by the Rust scheduler and may already exist.
            // Use raw SQL with IF NOT EXISTS so this migration is idempotent.
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS trd.sales_rebate_item_k8s (
                    id BIGINT NOT NULL,
                    created_on TIMESTAMPTZ NOT NULL,
                    sales_rebate_id BIGINT NOT NULL,
                    sales_account_id BIGINT NOT NULL,
                    trade_rebate_id BIGINT NOT NULL,
                    ticket BIGINT NOT NULL,
                    trade_account_id BIGINT NOT NULL,
                    trade_account_number BIGINT NOT NULL,
                    trade_account_type SMALLINT NOT NULL,
                    trade_account_fund_type INTEGER NOT NULL,
                    trade_account_currency_id INTEGER NOT NULL,
                    symbol TEXT NOT NULL,
                    volume INTEGER NOT NULL,
                    rebate_base BIGINT NOT NULL,
                    rebate_type TEXT NOT NULL,
                    amount BIGINT NOT NULL,
                    closed_on TIMESTAMPTZ NOT NULL,
                    PRIMARY KEY (id, created_on)
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS trd.sales_rebate_k8s (
                    id BIGINT NOT NULL,
                    created_on TIMESTAMPTZ NOT NULL,
                    sales_account_id BIGINT NOT NULL,
                    period_start TIMESTAMPTZ NOT NULL,
                    period_end TIMESTAMPTZ NOT NULL,
                    schedule_type SMALLINT NOT NULL,
                    total_amount BIGINT NOT NULL DEFAULT 0,
                    trade_count INTEGER NOT NULL DEFAULT 0,
                    status SMALLINT NOT NULL DEFAULT 0,
                    wallet_transaction_id BIGINT,
                    PRIMARY KEY (id, created_on)
                );
                ALTER TABLE trd.sales_rebate_k8s
                    ADD COLUMN IF NOT EXISTS status SMALLINT NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS wallet_transaction_id BIGINT;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // K8s tables are managed by the Rust scheduler; only remove the columns added above.
            migrationBuilder.Sql(@"
                ALTER TABLE trd.sales_rebate_k8s
                    DROP COLUMN IF EXISTS status,
                    DROP COLUMN IF EXISTS wallet_transaction_id;
            ");

            migrationBuilder.DropColumn(
                name: "AutoRelease",
                schema: "trd",
                table: "_SalesRebateSchema");

            migrationBuilder.DropColumn(
                name: "Ticket",
                schema: "trd",
                table: "_SalesRebate");

            migrationBuilder.AddForeignKey(
                name: "_SalesRebate_TradeRebateId_fkey",
                schema: "trd",
                table: "_SalesRebate",
                column: "TradeRebateId",
                principalSchema: "trd",
                principalTable: "_TradeRebate",
                principalColumn: "Id");
        }
    }
}
