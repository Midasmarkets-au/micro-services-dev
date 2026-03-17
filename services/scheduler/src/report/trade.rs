use anyhow::Result;
use chrono::{DateTime, Utc};
use serde::{Deserialize, Serialize};
use sqlx::PgPool;

use super::csv::to_csv_bytes;
use super::request::DateRangeCriteria;
use crate::report::ReportRequestType;
use crate::AppContext;

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct TradeRecord {
    pub ticket: Option<i64>,
    pub account_number: Option<i64>,
    pub login: Option<i64>,
    pub symbol: Option<String>,
    pub cmd: Option<i32>,
    pub volume: Option<f64>,
    pub open_time: Option<DateTime<Utc>>,
    pub open_price: Option<f64>,
    pub close_time: Option<DateTime<Utc>>,
    pub close_price: Option<f64>,
    pub profit: Option<f64>,
    pub commission: Option<f64>,
    pub swap: Option<f64>,
    pub comment: Option<String>,
}

pub async fn generate_trade_csv(
    _ctx: &AppContext,
    tenant_pool: &PgPool,
    _tenant_id: i64,
    criteria: &DateRangeCriteria,
    _report_type: ReportRequestType,
) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(1));
    let to = criteria.to.unwrap_or_else(Utc::now);

    // Get MT5 connection string from the first available MT5 trade service
    let mt5_conn = crate::db::tenant::get_mt5_connection_string(tenant_pool, 1).await?;

    if let Some(conn_str) = mt5_conn {
        let mt5_pool = crate::db::mysql_pool(&conn_str).await?;
        let logins = get_active_logins(tenant_pool).await?;
        let deals = crate::db::mt5::get_deals(&mt5_pool, &logins, from, to).await?;

        let records: Vec<TradeRecord> = deals
            .into_iter()
            .map(|d| TradeRecord {
                ticket: Some(d.deal),
                account_number: None,
                login: Some(d.login),
                symbol: d.symbol,
                cmd: d.action,
                volume: d.volume,
                open_time: None,
                open_price: None,
                close_time: d.time,
                close_price: d.price,
                profit: d.profit,
                commission: d.commission,
                swap: d.swap,
                comment: d.comment,
            })
            .collect();

        return to_csv_bytes(&records);
    }

    // Fallback: query from TenantDb TradeTransactions
    generate_trade_csv_from_tenant_db(tenant_pool, criteria).await
}

async fn get_active_logins(pool: &PgPool) -> Result<Vec<i64>> {
    let rows: Vec<(Option<i64>,)> = sqlx::query_as(
        r#"SELECT "AccountNumber" FROM trd."_Account"
           WHERE "AccountNumber" IS NOT NULL AND "Status" = 1"#,
    )
    .fetch_all(pool)
    .await?;
    Ok(rows.into_iter().filter_map(|r| r.0).collect())
}

async fn generate_trade_csv_from_tenant_db(
    pool: &PgPool,
    criteria: &DateRangeCriteria,
) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(1));
    let to = criteria.to.unwrap_or_else(Utc::now);

    let records = sqlx::query_as::<_, TradeRecord>(
        r#"SELECT
            t."Ticket" as ticket,
            a."AccountNumber" as account_number,
            a."AccountNumber" as login,
            t."Symbol" as symbol,
            t."Cmd" as cmd,
            t."Volume" as volume,
            t."OpenAt" as open_time,
            t."OpenPrice" as open_price,
            t."CloseAt" as close_time,
            t."ClosePrice" as close_price,
            t."Profit" as profit,
            t."Commission" as commission,
            t."Swaps" as swap,
            t."Comment" as comment
           FROM trd."_TradeTransaction" t
           LEFT JOIN trd."_TradeAccount" ta ON ta."Id" = t."TradeAccountId"
           LEFT JOIN trd."_Account" a ON a."Id" = ta."AccountId"
           WHERE t."CloseAt" >= $1 AND t."CloseAt" < $2
           ORDER BY t."CloseAt""#,
    )
    .bind(from)
    .bind(to)
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct DemoAccountRecord {
    pub account_number: Option<i64>,
    pub login: Option<i64>,
    pub party_id: Option<i64>,
    pub group: Option<String>,
    pub created_on: Option<DateTime<Utc>>,
}

pub async fn generate_demo_account_csv(pool: &PgPool, query_json: &str) -> Result<Vec<u8>> {
    #[derive(Debug, Deserialize)]
    struct DemoAccountCriteria {
        #[serde(rename = "Date")]
        date: Option<DateTime<Utc>>,
    }

    let criteria: DemoAccountCriteria = serde_json::from_str(query_json)
        .unwrap_or(DemoAccountCriteria { date: None });
    let date = criteria.date.unwrap_or_else(Utc::now);

    let records = sqlx::query_as::<_, DemoAccountRecord>(
        r#"SELECT
            a."AccountNumber" as account_number,
            a."AccountNumber" as login,
            a."PartyId" as party_id,
            a."Group" as group,
            a."CreatedOn" as created_on
           FROM trd."_Account" a
           WHERE a."Type" = 2
           AND a."CreatedOn"::date = $1::date
           ORDER BY a."CreatedOn""#,
    )
    .bind(date)
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}
