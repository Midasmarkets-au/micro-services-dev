use anyhow::Result;
use sqlx::PgPool;

use crate::models::meta_trade::NewTradeRebate;

const TABLE: &str = r#"trd."_TradeRebateK8s""#;

/// Check if a ticket+service already exists across all partitions.
pub async fn exists(pool: &PgPool, ticket: i64, service_id: i32) -> Result<bool> {
    let row: (bool,) = sqlx::query_as(&format!(
        r#"SELECT EXISTS(
            SELECT 1 FROM {TABLE}
            WHERE "Ticket" = $1 AND "TradeServiceId" = $2
        )"#,
    ))
    .bind(ticket)
    .bind(service_id)
    .fetch_one(pool)
    .await?;
    Ok(row.0)
}

/// Insert a new TradeRebate record. Returns Some(id) on insert, None if already exists.
/// The caller must call exists() first for cross-partition dedup.
pub async fn insert(pool: &PgPool, rebate: &NewTradeRebate) -> Result<Option<i64>> {
    let row: Option<(i64,)> = sqlx::query_as(&format!(
        r#"INSERT INTO {TABLE} (
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
