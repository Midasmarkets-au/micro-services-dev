use anyhow::Result;
use chrono::{DateTime, Utc};
use serde::{Deserialize, Serialize};
use sqlx::PgPool;

use super::csv::to_csv_bytes;
use super::request::DateRangeCriteria;
use crate::report::ReportRequestType;

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct SalesRebateRecord {
    pub id: i64,
    pub sales_account_id: Option<i64>,
    pub sales_code: Option<String>,
    pub account_id: i64,
    pub account_number: Option<i64>,
    pub amount: f64,
    pub currency: Option<String>,
    pub stated_on: Option<DateTime<Utc>>,
    pub created_on: Option<DateTime<Utc>>,
}

pub async fn generate_sales_rebate_csv(pool: &PgPool, criteria: &DateRangeCriteria) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(30));
    let to = criteria.to.unwrap_or_else(Utc::now);

    let records = sqlx::query_as::<_, SalesRebateRecord>(
        r#"SELECT
            sr."Id" as id,
            sr."SalesAccountId" as sales_account_id,
            sa."Code" as sales_code,
            sr."AccountId" as account_id,
            a."AccountNumber" as account_number,
            sr."Amount" as amount,
            sr."Currency" as currency,
            sr."StatedOn" as stated_on,
            sr."CreatedOn" as created_on
           FROM reb."_SalesRebate" sr
           LEFT JOIN trd."_Account" sa ON sa."Id" = sr."SalesAccountId"
           LEFT JOIN trd."_Account" a ON a."Id" = sr."AccountId"
           WHERE sr."CreatedOn" >= $1 AND sr."CreatedOn" < $2
           ORDER BY sr."CreatedOn""#,
    )
    .bind(from)
    .bind(to)
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct SalesRebateSumRecord {
    pub account_id: i64,
    pub account_number: Option<i64>,
    pub sales_account_id: Option<i64>,
    pub sales_code: Option<String>,
    pub total_amount: Option<f64>,
    pub trade_count: i64,
}

pub async fn generate_sales_rebate_sum_csv(pool: &PgPool, criteria: &DateRangeCriteria) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(30));
    let to = criteria.to.unwrap_or_else(Utc::now);

    let records = sqlx::query_as::<_, SalesRebateSumRecord>(
        r#"SELECT
            sr."AccountId" as account_id,
            a."AccountNumber" as account_number,
            sr."SalesAccountId" as sales_account_id,
            sa."Code" as sales_code,
            SUM(sr."Amount") as total_amount,
            COUNT(*) as trade_count
           FROM reb."_SalesRebate" sr
           LEFT JOIN trd."_Account" sa ON sa."Id" = sr."SalesAccountId"
           LEFT JOIN trd."_Account" a ON a."Id" = sr."AccountId"
           WHERE sr."CreatedOn" >= $1 AND sr."CreatedOn" < $2
           GROUP BY sr."AccountId", a."AccountNumber", sr."SalesAccountId", sa."Code"
           ORDER BY total_amount DESC"#,
    )
    .bind(from)
    .bind(to)
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct AccountSearchRecord {
    pub account_id: i64,
    pub account_number: Option<i64>,
    pub party_id: Option<i64>,
    pub group: Option<String>,
    pub status: Option<i16>,
    pub created_on: Option<DateTime<Utc>>,
}

pub async fn generate_account_search_csv(pool: &PgPool, criteria: &DateRangeCriteria) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(30));
    let to = criteria.to.unwrap_or_else(Utc::now);

    let records = sqlx::query_as::<_, AccountSearchRecord>(
        r#"SELECT
            a."Id" as account_id,
            a."AccountNumber" as account_number,
            a."PartyId" as party_id,
            a."Group" as group,
            a."Status" as status,
            a."CreatedOn" as created_on
           FROM trd."_Account" a
           WHERE a."CreatedOn" >= $1 AND a."CreatedOn" < $2
           ORDER BY a."CreatedOn""#,
    )
    .bind(from)
    .bind(to)
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct SalesReportRecord {
    pub account_id: i64,
    pub account_number: Option<i64>,
    pub sales_code: Option<String>,
    pub trade_count: i64,
    pub total_volume: Option<f64>,
    pub total_profit: Option<f64>,
    pub total_commission: Option<f64>,
}

pub async fn generate_sales_report_csv(
    pool: &PgPool,
    criteria: &DateRangeCriteria,
    _report_type: ReportRequestType,
) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(7));
    let to = criteria.to.unwrap_or_else(Utc::now);

    let records = sqlx::query_as::<_, SalesReportRecord>(
        r#"SELECT
            ta."AccountId" as account_id,
            a."AccountNumber" as account_number,
            sa."Code" as sales_code,
            COUNT(t."Id") as trade_count,
            SUM(t."Volume") as total_volume,
            SUM(t."Profit") as total_profit,
            SUM(t."Commission") as total_commission
           FROM trd."_TradeTransaction" t
           JOIN trd."_TradeAccount" ta ON ta."Id" = t."TradeAccountId"
           JOIN trd."_Account" a ON a."Id" = ta."AccountId"
           LEFT JOIN trd."_Account" sa ON sa."Id" = a."SalesAccountId"
           WHERE t."CloseAt" >= $1 AND t."CloseAt" < $2
           GROUP BY ta."AccountId", a."AccountNumber", sa."Code"
           ORDER BY total_volume DESC NULLS LAST"#,
    )
    .bind(from)
    .bind(to)
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct IbReportRecord {
    pub agent_account_id: i64,
    pub agent_account_number: Option<i64>,
    pub client_account_id: Option<i64>,
    pub client_account_number: Option<i64>,
    pub total_rebate: Option<f64>,
    pub trade_count: i64,
}

pub async fn generate_ib_report_csv(
    pool: &PgPool,
    criteria: &DateRangeCriteria,
    _report_type: ReportRequestType,
) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(30));
    let to = criteria.to.unwrap_or_else(Utc::now);

    let records = sqlx::query_as::<_, IbReportRecord>(
        r#"SELECT
            r."AccountId" as agent_account_id,
            a."AccountNumber" as agent_account_number,
            r."SourceAccountId" as client_account_id,
            sa."AccountNumber" as client_account_number,
            SUM(r."Amount") as total_rebate,
            COUNT(r."Id") as trade_count
           FROM reb."_TradeRebate" r
           LEFT JOIN trd."_Account" a ON a."Id" = r."AccountId"
           LEFT JOIN trd."_Account" sa ON sa."Id" = r."SourceAccountId"
           WHERE r."CreatedOn" >= $1 AND r."CreatedOn" < $2
           GROUP BY r."AccountId", a."AccountNumber", r."SourceAccountId", sa."AccountNumber"
           ORDER BY total_rebate DESC NULLS LAST"#,
    )
    .bind(from)
    .bind(to)
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}
