use anyhow::Result;
use sqlx::PgPool;

use crate::models::meta_trade::NewTradeRebate;

/// Returns the per-year table name, e.g. `trd."_TradeRebate_2026"`.
fn table_name(year: i32) -> String {
    format!(r#"trd."_TradeRebate_{}""#, year)
}

/// Unique index name for a given year.
fn ux_name(year: i32) -> String {
    format!(r#"UX__TradeRebate_{}_Ticket_ServiceId"#, year)
}

/// Account number index name for a given year.
fn ix_account_name(year: i32) -> String {
    format!(r#"IX__TradeRebate_{}_AccountNumber"#, year)
}

/// Ensure the per-year trd._TradeRebate_{year} table exists.
/// Called on tenant pool init (current year) and lazily before insert (any year).
pub async fn ensure_table(pool: &PgPool, year: i32) -> Result<()> {
    let tbl = table_name(year);
    let ux = ux_name(year);
    let ix = ix_account_name(year);

    sqlx::query(&format!(r#"
        CREATE TABLE IF NOT EXISTS {tbl} (
            "Id"             BIGSERIAL        PRIMARY KEY,
            "AccountId"      BIGINT,
            "TradeServiceId" INT              NOT NULL,
            "Ticket"         BIGINT           NOT NULL,
            "AccountNumber"  BIGINT           NOT NULL,
            "CurrencyId"     INT              NOT NULL DEFAULT -1,
            "Volume"         INT              NOT NULL DEFAULT 0,
            "Status"         INT              NOT NULL DEFAULT 0,
            "RuleType"       INT              NOT NULL DEFAULT 199,
            "CreatedOn"      TIMESTAMPTZ      NOT NULL DEFAULT NOW(),
            "UpdatedOn"      TIMESTAMPTZ      NOT NULL DEFAULT NOW(),
            "ClosedOn"       TIMESTAMPTZ      NOT NULL,
            "OpenedOn"       TIMESTAMPTZ      NOT NULL,
            "TimeStamp"      BIGINT           NOT NULL DEFAULT 0,
            "Action"         INT              NOT NULL DEFAULT 0,
            "DealId"         BIGINT           NOT NULL DEFAULT 0,
            "Symbol"         VARCHAR(32)      NOT NULL DEFAULT '',
            "ReferPath"      VARCHAR(512)     NOT NULL DEFAULT '',
            "Commission"     NUMERIC(18,8)    NOT NULL DEFAULT 0,
            "Swaps"          NUMERIC(18,8)    NOT NULL DEFAULT 0,
            "OpenPrice"      NUMERIC(18,8)    NOT NULL DEFAULT 0,
            "ClosePrice"     NUMERIC(18,8)    NOT NULL DEFAULT 0,
            "Profit"         NUMERIC(18,8)    NOT NULL DEFAULT 0,
            "Reason"         INT              NOT NULL DEFAULT 0
        )
    "#))
    .execute(pool)
    .await?;

    sqlx::query(&format!(r#"
        CREATE UNIQUE INDEX IF NOT EXISTS "{ux}"
            ON {tbl} ("Ticket", "TradeServiceId")
    "#))
    .execute(pool)
    .await?;

    sqlx::query(&format!(r#"
        CREATE INDEX IF NOT EXISTS "{ix}"
            ON {tbl} ("AccountNumber")
    "#))
    .execute(pool)
    .await?;

    Ok(())
}

/// Check if a ticket+service already exists in the given year's table.
pub async fn exists(pool: &PgPool, ticket: i64, service_id: i32, year: i32) -> Result<bool> {
    let tbl = table_name(year);
    let row: (bool,) = sqlx::query_as(&format!(
        r#"SELECT EXISTS(
            SELECT 1 FROM {tbl}
            WHERE "Ticket" = $1 AND "TradeServiceId" = $2
        )"#,
    ))
    .bind(ticket)
    .bind(service_id)
    .fetch_one(pool)
    .await?;
    Ok(row.0)
}

/// Insert a new TradeRebate record into the given year's table. Idempotent via ON CONFLICT DO NOTHING.
/// Returns Some(id) on insert, None if already exists.
/// Lazily creates the table if it doesn't exist yet (handles year rollover).
pub async fn insert(pool: &PgPool, rebate: &NewTradeRebate, year: i32) -> Result<Option<i64>> {
    ensure_table(pool, year).await?;

    let tbl = table_name(year);
    let row: Option<(i64,)> = sqlx::query_as(&format!(
        r#"INSERT INTO {tbl} (
            "AccountId", "TradeServiceId", "Ticket", "AccountNumber",
            "CurrencyId", "Volume", "Status", "RuleType",
            "CreatedOn", "UpdatedOn", "ClosedOn", "OpenedOn",
            "TimeStamp", "Action", "DealId", "Symbol", "ReferPath",
            "Commission", "Swaps", "OpenPrice", "ClosePrice", "Profit", "Reason"
        ) VALUES (
            $1, $2, $3, $4, $5, $6, $7, $8,
            NOW(), NOW(), $9, $10, $11, $12, $13, $14, $15,
            $16, $17, $18, $19, $20, $21
        )
        ON CONFLICT ("Ticket", "TradeServiceId") DO NOTHING
        RETURNING "Id""#,
    ))
    .bind(rebate.account_id)
    .bind(rebate.trade_service_id)
    .bind(rebate.ticket)
    .bind(rebate.account_number)
    .bind(rebate.currency_id)
    .bind(rebate.volume)
    .bind(rebate.status)
    .bind(rebate.rule_type)
    .bind(rebate.closed_on)
    .bind(rebate.opened_on)
    .bind(rebate.time_stamp)
    .bind(rebate.action)
    .bind(rebate.deal_id)
    .bind(&rebate.symbol)
    .bind(&rebate.refer_path)
    .bind(rebate.commission)
    .bind(rebate.swaps)
    .bind(rebate.open_price)
    .bind(rebate.close_price)
    .bind(rebate.profit)
    .bind(rebate.reason)
    .fetch_optional(pool)
    .await?;
    Ok(row.map(|(id,)| id))
}
