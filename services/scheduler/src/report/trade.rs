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
    pub open_time: Option<String>,
    pub open_price: Option<f64>,
    pub close_time: Option<String>,
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

    // Get MT5 service IDs from tenant DB and use the first one
    let service_ids = crate::db::tenant::get_mt5_service_ids_from_central(tenant_pool).await?;
    let mt5_conn = match service_ids.first() {
        Some(&sid) => crate::db::tenant::get_mt5_connection_string_from_central(tenant_pool, sid).await?,
        None => None,
    };

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
                close_time: d.time.map(|t| t.format("%Y-%m-%d %H:%M:%S").to_string()),
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
            TO_CHAR(t."OpenAt", 'YYYY-MM-DD HH24:MI:SS') as open_time,
            t."OpenPrice" as open_price,
            TO_CHAR(t."CloseAt", 'YYYY-MM-DD HH24:MI:SS') as close_time,
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

/// Mirrors DemoAccountRecord.cs
/// Header: AccountNumber,ExpireOn,Leverage,Balance,Type,Currency,NativeName,FirstName,
///         LastName,CountryCode,PhoneNumber,Email,Language,CreatedOn
#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct DemoAccountRecord {
    #[serde(rename = "AccountNumber")]
    pub account_number: Option<i64>,
    #[serde(rename = "ExpireOn")]
    pub expire_on: Option<String>,
    #[serde(rename = "Leverage")]
    pub leverage: Option<i32>,
    #[serde(rename = "Balance")]
    pub balance: Option<f64>,       // demo."Balance" / 100
    #[serde(rename = "Type")]
    pub r#type: Option<String>,     // CASE on demo."Type"
    #[serde(rename = "Currency")]
    pub currency: Option<String>,   // CASE on demo."CurrencyId"
    #[serde(rename = "NativeName")]
    pub native_name: Option<String>,
    #[serde(rename = "FirstName")]
    pub first_name: Option<String>,
    #[serde(rename = "LastName")]
    pub last_name: Option<String>,
    #[serde(rename = "CountryCode")]
    pub country_code: Option<String>,
    #[serde(rename = "PhoneNumber")]
    pub phone_number: Option<String>,
    #[serde(rename = "Email")]
    pub email: Option<String>,
    #[serde(rename = "Language")]
    pub language: Option<String>,
    #[serde(rename = "CreatedOn")]
    pub created_on: Option<String>, // formatted "yyyy-MM-dd HH:mm:ss"
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
    // C# uses `demo."CreatedOn" > date - 7 days`
    let from_date = date - chrono::Duration::days(7);

    let records = sqlx::query_as::<_, DemoAccountRecord>(
        r#"SELECT
            demo."AccountNumber"                                                        as account_number,
            TO_CHAR(demo."ExpireOn", 'YYYY-MM-DD HH24:MI:SS')                          as expire_on,
            demo."Leverage"                                                             as leverage,
            demo."Balance"::float8 / 100.0                                             as balance,
            CASE demo."Type"
                WHEN 4  THEN 'STD'
                WHEN 6  THEN 'ALPHA'
                WHEN 11 THEN 'Advantage'
                WHEN 13 THEN 'Sea STD'
                ELSE demo."Type"::text
            END                                                                         as type,
            CASE demo."CurrencyId"
                WHEN 36  THEN 'AUD'
                WHEN 840 THEN 'USD'
                ELSE demo."CurrencyId"::text
            END                                                                         as currency,
            CASE WHEN demo."PartyId" > 1 THEN p."NativeName"  ELSE demo."Name"        END as native_name,
            CASE WHEN demo."PartyId" > 1 THEN p."FirstName"   ELSE ''                 END as first_name,
            CASE WHEN demo."PartyId" > 1 THEN p."LastName"    ELSE ''                 END as last_name,
            CASE WHEN demo."PartyId" > 1 THEN p."CountryCode" ELSE demo."CountryCode" END as country_code,
            CASE WHEN demo."PartyId" > 1 THEN p."PhoneNumber" ELSE demo."PhoneNumber" END as phone_number,
            demo."Email"                                                                as email,
            CASE WHEN demo."PartyId" > 1 THEN p."Language"    ELSE ''                 END as language,
            TO_CHAR(demo."CreatedOn", 'YYYY-MM-DD HH24:MI:SS')                         as created_on
           FROM trd."_TradeDemoAccount" demo
           LEFT JOIN core."_Party" p ON p."Id" = demo."PartyId"
           WHERE demo."CreatedOn" > $1
             AND (
               demo."PartyId" = 1
               OR (demo."PartyId" > 1
                   AND NOT EXISTS (
                       SELECT 1 FROM trd."_Account" acc WHERE acc."PartyId" = demo."PartyId"
                   ))
             )
           ORDER BY demo."CreatedOn""#,
    )
    .bind(from_date)
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}
