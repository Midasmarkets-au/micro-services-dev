using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class AddTradeRebateK8sReadModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // These tables are managed by the Rust scheduler (services/scheduler/src/db/partition.rs
            // ::ensure_parent_tables) and already exist in every deployed environment. Use raw SQL with
            // IF NOT EXISTS so this migration is idempotent and never conflicts with scheduler-owned DDL.
            // No FK constraints are added here — the scheduler already maintains
            // trade_rebate_k8s.account_id / rebate_k8s.account_id FKs at startup, and matter_k8s /
            // rebate_k8s.party_id / rebate_k8s.trade_rebate_id have no DB-level FK by design.
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS core.matter_k8s (
                    id        BIGINT       NOT NULL,
                    pid       BIGINT,
                    type      INT          NOT NULL,
                    state_id  INT          NOT NULL,
                    posted_on TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
                    stated_on TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
                    PRIMARY KEY (id)
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS trd.trade_rebate_k8s (
                    id               BIGINT         NOT NULL,
                    account_id       BIGINT,
                    trade_service_id INT            NOT NULL,
                    ticket           BIGINT         NOT NULL,
                    account_number   BIGINT         NOT NULL,
                    currency_id      INT            NOT NULL DEFAULT -1,
                    volume           INT            NOT NULL DEFAULT 0,
                    status           INT            NOT NULL DEFAULT 0,
                    rule_type        INT            NOT NULL DEFAULT 199,
                    created_on       TIMESTAMPTZ    NOT NULL DEFAULT NOW(),
                    updated_on       TIMESTAMPTZ    NOT NULL DEFAULT NOW(),
                    closed_on        TIMESTAMPTZ    NOT NULL,
                    opened_on        TIMESTAMPTZ    NOT NULL,
                    time_stamp       BIGINT         NOT NULL DEFAULT 0,
                    action           INT            NOT NULL DEFAULT 0,
                    deal_id          BIGINT         NOT NULL DEFAULT 0,
                    symbol           VARCHAR(32)    NOT NULL DEFAULT '',
                    refer_path       VARCHAR(512)   NOT NULL DEFAULT '',
                    commission       NUMERIC(18,8)  NOT NULL DEFAULT 0,
                    swaps            NUMERIC(18,8)  NOT NULL DEFAULT 0,
                    open_price       NUMERIC(18,8)  NOT NULL DEFAULT 0,
                    close_price      NUMERIC(18,8)  NOT NULL DEFAULT 0,
                    profit           NUMERIC(18,8)  NOT NULL DEFAULT 0,
                    reason           INT            NOT NULL DEFAULT 0,
                    PRIMARY KEY (id, closed_on)
                ) PARTITION BY RANGE (closed_on);
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS trd.rebate_k8s (
                    id              BIGINT        NOT NULL,
                    party_id        BIGINT        NOT NULL,
                    account_id      BIGINT        NOT NULL,
                    fund_type       INT           NOT NULL DEFAULT 0,
                    currency_id     INT           NOT NULL DEFAULT -1,
                    amount          NUMERIC(20,8) NOT NULL DEFAULT 0,
                    trade_rebate_id BIGINT,
                    hold_until_on   TIMESTAMPTZ,
                    information     TEXT          NOT NULL DEFAULT '',
                    created_on      TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
                    PRIMARY KEY (id, created_on)
                ) PARTITION BY RANGE (created_on);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // K8s tables are managed by the Rust scheduler; do not drop them here.
        }
    }
}
