use anyhow::Result;
use chrono::Utc;
use serde::{Deserialize, Serialize};
use sqlx::PgPool;

use super::csv::to_csv_bytes;
use super::request::DateRangeCriteria;
use crate::report::ReportRequestType;

/// MT5 server is GMT+2
const HOURS_GAP: i64 = 2;

// ──────────────────────────────────────────────────────────────────────────────
// SalesRebate report
// Mirrors ProcessSalesRebateForTenantRequestAsync + SalesRebateRecord.cs
// Header: id,trade_rebate_id,sales_uid,trade_account_number,account_type,
//         currency,amount,status,rebate_type,rebate_base,wallet_id,created_on
// ──────────────────────────────────────────────────────────────────────────────

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct SalesRebateRecord {
    pub id: i64,
    pub trade_rebate_id: Option<i64>,
    pub sales_uid: Option<i64>,
    pub trade_account_number: Option<i64>,
    pub account_type: Option<i16>,
    pub currency: Option<String>,
    pub amount: Option<f64>,   // SalesRebate.Amount / 100
    pub status: Option<String>, // SalesRebateStatusTypes name
    pub rebate_type: Option<String>,
    pub rebate_base: Option<i64>,
    pub wallet_id: Option<i64>, // from WalletAdjust.WalletId
    pub created_on: Option<String>, // CreatedOn + HOURS_GAP, formatted "yyyy-MM-dd HH:mm:ss"
}

pub async fn generate_sales_rebate_csv(pool: &PgPool, criteria: &DateRangeCriteria) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(30));
    let to = criteria.to.unwrap_or_else(Utc::now);

    // from/to are MT5 time stored as "UTC"; subtract HOURS_GAP to get actual UTC
    // (mirrors DateHelper.MinusMT5GMTHours in ProcessSalesRebateForTenantRequestAsync)
    let from_utc = from - chrono::Duration::hours(HOURS_GAP);
    let to_utc = to - chrono::Duration::hours(HOURS_GAP);

    let records = sqlx::query_as::<_, SalesRebateRecord>(
        r#"SELECT
            sr."Id"                     as id,
            sr."TradeRebateId"          as trade_rebate_id,
            sa."Uid"                    as sales_uid,
            sr."TradeAccountNumber"     as trade_account_number,
            sr."TradeAccountType"       as account_type,
            c."Code"                    as currency,
            sr."Amount"::float8 / 100.0 as amount,
            CASE sr."Status"
                WHEN -2 THEN 'NoAccountFound'
                WHEN  0 THEN 'Pending'
                WHEN  1 THEN 'Complete'
                WHEN  2 THEN 'History2'
                WHEN  3 THEN 'History3'
                WHEN  4 THEN 'RebateExist'
                WHEN  5 THEN 'History5'
                WHEN  6 THEN 'Pause'
                ELSE sr."Status"::text
            END                         as status,
            sr."RebateType"             as rebate_type,
            sr."RebateBase"             as rebate_base,
            wa."WalletId"               as wallet_id,
            TO_CHAR(sr."CreatedOn" + INTERVAL '2 hours', 'YYYY-MM-DD HH24:MI:SS') as created_on
           FROM trd."_SalesRebate" sr
           LEFT JOIN trd."_Account"    sa ON sa."Id"    = sr."SalesAccountId"
           LEFT JOIN acct."_Currency"  c  ON c."Id"     = sr."TradeAccountCurrencyId"
           LEFT JOIN acct."_WalletAdjust" wa ON wa."Id" = sr."WalletAdjustId"
           WHERE sr."CreatedOn" >= $1 AND sr."CreatedOn" < $2
           ORDER BY sr."Id" DESC"#,
    )
    .bind(from_utc)
    .bind(to_utc)
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}

// ──────────────────────────────────────────────────────────────────────────────
// SalesRebateSumByAccount report
// Mirrors ProcessSalesRebateSumByAccountForTenantRequestAsync + SalesRebateSumByAccountRecord.cs
// Header: trade_account_number,total_amount,wallet_id,sales_code
// ──────────────────────────────────────────────────────────────────────────────

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct SalesRebateSumRecord {
    pub trade_account_number: Option<i64>,
    pub total_amount: Option<f64>, // SUM(Amount) / 100
    pub wallet_id: Option<i64>,    // 0 — not set by mono's group query
    pub sales_code: Option<String>, // empty — not set by mono's group query
}

pub async fn generate_sales_rebate_sum_csv(pool: &PgPool, criteria: &DateRangeCriteria) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(30));
    let to = criteria.to.unwrap_or_else(Utc::now);

    let from_utc = from - chrono::Duration::hours(HOURS_GAP);
    let to_utc = to - chrono::Duration::hours(HOURS_GAP);

    // Mirrors mono: group by TradeAccountNumber, sum Amount.
    // WalletId and SalesCode are not set in the group query (default 0 / empty).
    let records = sqlx::query_as::<_, SalesRebateSumRecord>(
        r#"SELECT
            sr."TradeAccountNumber"              as trade_account_number,
            SUM(sr."Amount")::float8 / 100.0     as total_amount,
            0::bigint                            as wallet_id,
            NULL::text                           as sales_code
           FROM trd."_SalesRebate" sr
           WHERE sr."CreatedOn" >= $1 AND sr."CreatedOn" < $2
           GROUP BY sr."TradeAccountNumber"
           ORDER BY sr."TradeAccountNumber" DESC"#,
    )
    .bind(from_utc)
    .bind(to_utc)
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}

// ──────────────────────────────────────────────────────────────────────────────
// AccountSearch report
// Mirrors ProcessAccountSearchForTenantRequestAsync + AccountSearchRecord.cs
// Header: Name,Email,Group,Uid,AccountNumber,Server,AgentName,AgentEmail,AgentUid,
//         AgentGroup,SalesName,SalesEmail,SalesUid,SalesCode,CCC,PhoneNumber,ReferPath,WalletId
// ──────────────────────────────────────────────────────────────────────────────

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct AccountSearchRecord {
    #[serde(rename = "Name")]
    pub name: Option<String>,
    #[serde(rename = "Email")]
    pub email: Option<String>,
    #[serde(rename = "Group")]
    pub group: Option<String>,
    #[serde(rename = "Uid")]
    pub uid: Option<i64>,
    #[serde(rename = "AccountNumber")]
    pub account_number: Option<i64>,
    #[serde(rename = "Server")]
    pub server: Option<String>, // ServiceId as text (no pool lookup in Rust)
    #[serde(rename = "AgentName")]
    pub agent_name: Option<String>,
    #[serde(rename = "AgentEmail")]
    pub agent_email: Option<String>,
    #[serde(rename = "AgentUid")]
    pub agent_uid: Option<i64>,
    #[serde(rename = "AgentGroup")]
    pub agent_group: Option<String>, // Account.Group (own group = agent group in mono)
    #[serde(rename = "SalesName")]
    pub sales_name: Option<String>,
    #[serde(rename = "SalesEmail")]
    pub sales_email: Option<String>,
    #[serde(rename = "SalesUid")]
    pub sales_uid: Option<i64>,
    #[serde(rename = "SalesCode")]
    pub sales_code: Option<String>,
    #[serde(rename = "CCC")]
    pub ccc: Option<String>,
    #[serde(rename = "PhoneNumber")]
    pub phone_number: Option<String>,
    #[serde(rename = "ReferPath")]
    pub refer_path: Option<String>,
    #[serde(rename = "WalletId")]
    pub wallet_id: Option<i64>,
}

pub async fn generate_account_search_csv(pool: &PgPool, _criteria: &DateRangeCriteria) -> Result<Vec<u8>> {
    // Mirrors AccountSearchRecordExtensions.ToRecords() with added wallet lookup.
    // Note: Server name cannot be resolved here (requires in-memory service map); ServiceId is output instead.
    let records = sqlx::query_as::<_, AccountSearchRecord>(
        r#"SELECT
            p."NativeName"        as name,
            p."Email"             as email,
            COALESCE(a."Group", '')  as group,
            a."Uid"               as uid,
            a."AccountNumber"     as account_number,
            a."ServiceId"::text   as server,
            ap."NativeName"       as agent_name,
            ap."Email"            as agent_email,
            ag."Uid"              as agent_uid,
            ag."Group"            as agent_group,
            sp."NativeName"       as sales_name,
            sp."Email"            as sales_email,
            sa."Uid"              as sales_uid,
            CASE
                WHEN a."Role" = 3 THEN a."Code"
                WHEN sa."Code" IS NOT NULL THEN sa."Code"
                ELSE NULL
            END                   as sales_code,
            p."CCC"               as ccc,
            p."PhoneNumber"       as phone_number,
            a."ReferPath"         as refer_path,
            w."Id"                as wallet_id
           FROM trd."_Account" a
           LEFT JOIN core."_Party" p   ON p."Id"  = a."PartyId"
           LEFT JOIN trd."_Account" ag ON ag."Id" = a."AgentAccountId"
           LEFT JOIN core."_Party" ap  ON ap."Id" = ag."PartyId"
           LEFT JOIN trd."_Account" sa ON sa."Id" = a."SalesAccountId"
           LEFT JOIN core."_Party" sp  ON sp."Id" = sa."PartyId"
           LEFT JOIN acct."_Wallet" w  ON w."PartyId" = a."PartyId"
               AND w."CurrencyId" = a."CurrencyId"
               AND w."FundType"   = a."FundType"
           WHERE a."Status" = 1
           ORDER BY a."AccountNumber""#,
    )
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}

// ──────────────────────────────────────────────────────────────────────────────
// SalesReport / IB report (internal analytics, no direct mono equivalent needed)
// ──────────────────────────────────────────────────────────────────────────────

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
           FROM trd."_TradeRebate" r
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
