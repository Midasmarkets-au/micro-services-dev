use anyhow::Result;
use chrono::Utc;
use serde::{Deserialize, Serialize};
use sqlx::PgPool;

use super::csv::to_csv_bytes;
use super::request::DateRangeCriteria;

/// MT5 server is GMT+2 — applied to convert StatedOn (UTC) to MT5 display time,
/// and to adjust from/to (which arrive as MT5 time stored as "UTC") to real UTC.
const HOURS_GAP: i64 = 2;

/// Mirrors RebateRecord.cs — Header: Ticket,Symbol,AccountNumber,AccountUid,ClientName,ClientCode,
/// Currency,SourceCurrency,Volume,Amount,RateValue,PipsValue,CommissionValue,Rate,Pips,Commission,
/// ClosedOn,CreatedOn,ReleasedOn
#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct RebateRecord {
    #[serde(rename = "Ticket")]
    pub ticket: i64,
    #[serde(rename = "Symbol")]
    pub symbol: Option<String>,
    #[serde(rename = "AccountNumber")]
    pub account_number: i64,
    #[serde(rename = "AccountUid")]
    pub account_uid: i64,
    #[serde(rename = "ClientName")]
    pub client_name: Option<String>,
    #[serde(rename = "ClientCode")]
    pub client_code: Option<String>,
    #[serde(rename = "Currency")]
    pub currency: Option<String>,
    #[serde(rename = "SourceCurrency")]
    pub source_currency: Option<String>,
    #[serde(rename = "Volume")]
    pub volume: Option<f64>, // TradeRebate.Volume / 100
    #[serde(rename = "Amount")]
    pub amount: f64, // Rebate.Amount / 1,000,000
    #[serde(rename = "RateValue")]
    pub rate_value: f64, // Information JSON baseRebate.rate / 100
    #[serde(rename = "PipsValue")]
    pub pips_value: f64, // Information JSON baseRebate.pip / 100
    #[serde(rename = "CommissionValue")]
    pub commission_value: f64, // Information JSON baseRebate.commission / 100
    #[serde(rename = "Rate")]
    pub rate: f64,
    #[serde(rename = "Pips")]
    pub pips: f64,
    #[serde(rename = "Commission")]
    pub commission: f64,
    #[serde(rename = "ClosedOn")]
    pub closed_on: Option<String>,   // TradeRebate.ClosedOn (MT5 time, formatted)
    #[serde(rename = "CreatedOn")]
    pub created_on: Option<String>,  // max(WalletTransaction.CreatedOn) + HOURS_GAP, formatted
    #[serde(rename = "ReleasedOn")]
    pub released_on: Option<String>, // Matter.StatedOn + HOURS_GAP, formatted
}

/// Mirrors ProcessRebateRequestAsync logic:
///
/// is_from_api=false (job mode, IsFromApi=0): always use ClosedTime logic —
///   filter by TradeRebate.ClosedOn (MT5 time, from/to direct) AND Matter.StatedOn (UTC, from-2h / to-2h+5min)
///
/// is_from_api=true (API mode, IsFromApi=1, UseClosingTime assumed false): filter by Matter.StatedOn only.
pub async fn generate_rebate_csv(
    pool: &PgPool,
    criteria: &DateRangeCriteria,
    is_from_api: bool,
) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(1));
    let to = criteria.to.unwrap_or_else(Utc::now);

    // from/to arrive as MT5 time (GMT+2) stored as "UTC".
    // Subtract HOURS_GAP to get actual UTC for StatedOn filter.
    let from_utc = from - chrono::Duration::hours(HOURS_GAP);
    let to_utc = to - chrono::Duration::hours(HOURS_GAP);
    let to_utc_plus_5m = to_utc + chrono::Duration::minutes(5);

    // Common SELECT clause (identical for both modes)
    let select_sql = r#"
        tr."Ticket" as ticket,
        tr."Symbol" as symbol,
        tr."AccountNumber" as account_number,
        a."Uid" as account_uid,
        p."NativeName" as client_name,
        a."Code" as client_code,
        c."Code" as currency,
        COALESCE(sc."Code", c."Code") as source_currency,
        tr."Volume"::float8 / 100.0 as volume,
        r."Amount"::float8 / 1000000.0 as amount,
        COALESCE((r."Information"::jsonb -> 'baseRebate' ->> 'rate')::float8, 0.0) / 100.0 as rate_value,
        COALESCE((r."Information"::jsonb -> 'baseRebate' ->> 'pip')::float8, 0.0) / 100.0 as pips_value,
        COALESCE((r."Information"::jsonb -> 'baseRebate' ->> 'commission')::float8, 0.0) / 100.0 as commission_value,
        COALESCE((r."Information"::jsonb -> 'baseRebate' ->> 'rate')::float8, 0.0) / 100.0 as rate,
        COALESCE((r."Information"::jsonb -> 'baseRebate' ->> 'pip')::float8, 0.0) / 100.0 as pips,
        COALESCE((r."Information"::jsonb -> 'baseRebate' ->> 'commission')::float8, 0.0) / 100.0 as commission,
        TO_CHAR(tr."ClosedOn", 'YYYY-MM-DD HH24:MI:SS')                                        as closed_on,
        TO_CHAR(wt.max_created_on + INTERVAL '2 hours', 'YYYY-MM-DD HH24:MI:SS')               as created_on,
        TO_CHAR(m."StatedOn"   + INTERVAL '2 hours', 'YYYY-MM-DD HH24:MI:SS')                  as released_on
       FROM trd."_Rebate" r
       LEFT JOIN trd."_TradeRebate" tr ON tr."Id" = r."TradeRebateId"
       JOIN trd."_Account" a ON a."Id" = r."AccountId"
       LEFT JOIN core."_Party" p ON p."Id" = a."PartyId"
       LEFT JOIN trd."_Account" ta ON ta."Id" = tr."AccountId"
       LEFT JOIN acct."_Currency" c ON c."Id" = r."CurrencyId"
       LEFT JOIN acct."_Currency" sc ON sc."Id" = ta."CurrencyId"
       JOIN core."_Matter" m ON m."Id" = r."Id"
       LEFT JOIN (
           SELECT "MatterId", MAX("CreatedOn") as max_created_on
           FROM acct."_WalletTransaction"
           GROUP BY "MatterId"
       ) wt ON wt."MatterId" = r."Id""#;

    let records = if !is_from_api {
        // Job mode: TradeRebate.Status IN (Completed=2, SkippedWithOpenCloseTimeLessThanOneMinute=4)
        // Filter by ClosedOn (MT5 time) AND StatedOn (UTC after -2h, +5min buffer)
        let sql = format!(
            r#"SELECT {} WHERE tr."Status" IN (2, 4)
               AND tr."ClosedOn" >= $1 AND tr."ClosedOn" <= $2
               AND m."StatedOn" >= $3 AND m."StatedOn" < $4
               ORDER BY r."Id" DESC"#,
            select_sql
        );
        sqlx::query_as::<_, RebateRecord>(&sql)
            .bind(from)
            .bind(to)
            .bind(from_utc)
            .bind(to_utc_plus_5m)
            .fetch_all(pool)
            .await?
    } else {
        // API mode (UseClosingTime=false): filter by StatedOn only, no +5min buffer
        let sql = format!(
            r#"SELECT {} WHERE tr."Status" IN (2, 4)
               AND m."StatedOn" >= $1 AND m."StatedOn" <= $2
               ORDER BY r."Id" DESC"#,
            select_sql
        );
        sqlx::query_as::<_, RebateRecord>(&sql)
            .bind(from_utc)
            .bind(to_utc)
            .fetch_all(pool)
            .await?
    };

    to_csv_bytes(&records)
}
