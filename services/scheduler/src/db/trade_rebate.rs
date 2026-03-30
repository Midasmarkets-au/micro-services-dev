use anyhow::Result;
use sqlx::PgPool;

use crate::models::meta_trade::NewTradeRebate;

/// 检查 _TradeRebateNew 中是否已存在该 ticket + service 组合（去重）
pub async fn exists(pool: &PgPool, ticket: i64, service_id: i32) -> Result<bool> {
    let row: (bool,) = sqlx::query_as(
        r#"SELECT EXISTS(
            SELECT 1 FROM trd."_TradeRebateNew"
            WHERE "Ticket" = $1 AND "TradeServiceId" = $2
        )"#,
    )
    .bind(ticket)
    .bind(service_id)
    .fetch_one(pool)
    .await?;
    Ok(row.0)
}

/// 插入新 TradeRebate 记录。ON CONFLICT DO NOTHING（幂等）
/// 返回 Some(id) 成功插入，None 表示已存在
pub async fn insert(pool: &PgPool, rebate: &NewTradeRebate) -> Result<Option<i64>> {
    let row: Option<(i64,)> = sqlx::query_as(
        r#"INSERT INTO trd."_TradeRebateNew" (
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
    )
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
