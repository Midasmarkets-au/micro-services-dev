#![allow(dead_code)]

use anyhow::Result;
use chrono::{DateTime, Utc};
use serde::{Deserialize, Serialize};
use sqlx::PgPool;

/// Mirrors sto._ReportRequest
#[derive(Debug, Clone, sqlx::FromRow, Serialize, Deserialize)]
pub struct ReportRequest {
    pub id: i64,
    #[sqlx(rename = "Type")]
    pub r#type: i32,
    #[sqlx(rename = "PartyId")]
    pub party_id: i64,
    #[sqlx(rename = "CreatedOn")]
    pub created_on: DateTime<Utc>,
    #[sqlx(rename = "GeneratedOn")]
    pub generated_on: Option<DateTime<Utc>>,
    #[sqlx(rename = "ExpireOn")]
    pub expire_on: Option<DateTime<Utc>>,
    #[sqlx(rename = "Name")]
    pub name: String,
    #[sqlx(rename = "FileName")]
    pub file_name: String,
    #[sqlx(rename = "Query")]
    pub query: String,
    #[sqlx(rename = "IsFromApi")]
    pub is_from_api: i32,
}

/// Mirrors sto._ReportRequest for insert
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct NewReportRequest {
    pub r#type: i32,
    pub party_id: i64,
    pub name: String,
    pub query: String,
    pub is_from_api: i32,
}

pub async fn get_report_request(pool: &PgPool, id: i64) -> Result<Option<ReportRequest>> {
    let row = sqlx::query_as::<_, ReportRequest>(
        r#"SELECT "Id" as id, "Type", "PartyId", "CreatedOn", "GeneratedOn", "ExpireOn",
                  "Name", "FileName", "Query", "IsFromApi"
           FROM sto."_ReportRequest"
           WHERE "Id" = $1"#,
    )
    .bind(id)
    .fetch_optional(pool)
    .await?;
    Ok(row)
}

pub async fn mark_report_generated(
    pool: &PgPool,
    id: i64,
    file_name: &str,
) -> Result<()> {
    sqlx::query(
        r#"UPDATE sto."_ReportRequest"
           SET "GeneratedOn" = NOW(), "FileName" = $1
           WHERE "Id" = $2"#,
    )
    .bind(file_name)
    .bind(id)
    .execute(pool)
    .await?;
    Ok(())
}

pub async fn insert_report_request(pool: &PgPool, req: &NewReportRequest) -> Result<i64> {
    let row: (i64,) = sqlx::query_as(
        r#"INSERT INTO sto."_ReportRequest" ("Type", "PartyId", "Name", "Query", "IsFromApi", "FileName")
           VALUES ($1, $2, $3, $4, $5, '')
           RETURNING "Id""#,
    )
    .bind(req.r#type)
    .bind(req.party_id)
    .bind(&req.name)
    .bind(&req.query)
    .bind(req.is_from_api)
    .fetch_one(pool)
    .await?;
    Ok(row.0)
}

/// Mirrors trd._Account (minimal fields needed for reports)
#[derive(Debug, Clone, sqlx::FromRow, Serialize, Deserialize)]
pub struct Account {
    #[sqlx(rename = "Id")]
    pub id: i64,
    #[sqlx(rename = "Uid")]
    pub uid: String,
    #[sqlx(rename = "PartyId")]
    pub party_id: i64,
    #[sqlx(rename = "AccountNumber")]
    pub account_number: Option<i64>,
    #[sqlx(rename = "Group")]
    pub group: Option<String>,
    #[sqlx(rename = "CurrencyId")]
    pub currency_id: Option<i32>,
    #[sqlx(rename = "Type")]
    pub r#type: Option<i16>,
    #[sqlx(rename = "Status")]
    pub status: Option<i16>,
}

pub async fn get_accounts_by_party(pool: &PgPool, party_id: i64) -> Result<Vec<Account>> {
    let rows = sqlx::query_as::<_, Account>(
        r#"SELECT "Id", "Uid", "PartyId", "AccountNumber", "Group", "CurrencyId", "Type", "Status"
           FROM trd."_Account"
           WHERE "PartyId" = $1"#,
    )
    .bind(party_id)
    .fetch_all(pool)
    .await?;
    Ok(rows)
}

/// Mirrors fin._Deposit (minimal fields)
#[derive(Debug, Clone, sqlx::FromRow, Serialize, Deserialize)]
pub struct Deposit {
    #[sqlx(rename = "Id")]
    pub id: i64,
    #[sqlx(rename = "AccountId")]
    pub account_id: i64,
    #[sqlx(rename = "Amount")]
    pub amount: f64,
    #[sqlx(rename = "Currency")]
    pub currency: Option<String>,
    #[sqlx(rename = "Status")]
    pub status: Option<i32>,
    #[sqlx(rename = "CreatedOn")]
    pub created_on: DateTime<Utc>,
    #[sqlx(rename = "ApprovedOn")]
    pub approved_on: Option<DateTime<Utc>>,
}

/// Mirrors fin._Withdrawal (minimal fields)
#[derive(Debug, Clone, sqlx::FromRow, Serialize, Deserialize)]
pub struct Withdrawal {
    #[sqlx(rename = "Id")]
    pub id: i64,
    #[sqlx(rename = "AccountId")]
    pub account_id: i64,
    #[sqlx(rename = "Amount")]
    pub amount: f64,
    #[sqlx(rename = "Currency")]
    pub currency: Option<String>,
    #[sqlx(rename = "Status")]
    pub status: Option<i32>,
    #[sqlx(rename = "CreatedOn")]
    pub created_on: DateTime<Utc>,
    #[sqlx(rename = "ApprovedOn")]
    pub approved_on: Option<DateTime<Utc>>,
}

/// Mirrors reb._Rebate (minimal fields)
#[derive(Debug, Clone, sqlx::FromRow, Serialize, Deserialize)]
pub struct Rebate {
    #[sqlx(rename = "Id")]
    pub id: i64,
    #[sqlx(rename = "AccountId")]
    pub account_id: i64,
    #[sqlx(rename = "Amount")]
    pub amount: f64,
    #[sqlx(rename = "StatedOn")]
    pub stated_on: Option<DateTime<Utc>>,
    #[sqlx(rename = "ReleasedOn")]
    pub released_on: Option<DateTime<Utc>>,
    #[sqlx(rename = "CreatedOn")]
    pub created_on: DateTime<Utc>,
}

/// Mirrors wlt._Wallet (minimal fields)
#[derive(Debug, Clone, sqlx::FromRow, Serialize, Deserialize)]
pub struct Wallet {
    #[sqlx(rename = "Id")]
    pub id: i64,
    #[sqlx(rename = "PartyId")]
    pub party_id: i64,
    #[sqlx(rename = "Balance")]
    pub balance: f64,
    #[sqlx(rename = "Currency")]
    pub currency: Option<String>,
}

pub async fn get_wallets_with_balance(pool: &PgPool) -> Result<Vec<i64>> {
    let rows: Vec<(i64,)> = sqlx::query_as(
        r#"SELECT "Id" FROM wlt."_Wallet" WHERE "Balance" != 0"#,
    )
    .fetch_all(pool)
    .await?;
    Ok(rows.into_iter().map(|r| r.0).collect())
}

/// Mirrors sto._AccountReport
#[derive(Debug, Clone, sqlx::FromRow, Serialize, Deserialize)]
pub struct AccountReport {
    #[sqlx(rename = "Id")]
    pub id: i64,
    #[sqlx(rename = "AccountId")]
    pub account_id: i64,
    #[sqlx(rename = "PartyId")]
    pub party_id: i64,
    #[sqlx(rename = "Date")]
    pub date: DateTime<Utc>,
    #[sqlx(rename = "Status")]
    pub status: Option<i32>,
    #[sqlx(rename = "FileName")]
    pub file_name: Option<String>,
    #[sqlx(rename = "Email")]
    pub email: Option<String>,
}

pub async fn get_pending_account_reports(
    pool: &PgPool,
    date: DateTime<Utc>,
) -> Result<Vec<AccountReport>> {
    let rows = sqlx::query_as::<_, AccountReport>(
        r#"SELECT "Id", "AccountId", "PartyId", "Date", "Status", "FileName", "Email"
           FROM sto."_AccountReport"
           WHERE "Date"::date = $1::date AND ("Status" IS NULL OR "Status" = 0)"#,
    )
    .bind(date)
    .fetch_all(pool)
    .await?;
    Ok(rows)
}

pub async fn count_pending_account_reports(pool: &PgPool, date: DateTime<Utc>) -> Result<i64> {
    let row: (i64,) = sqlx::query_as(
        r#"SELECT COUNT(*) FROM sto."_AccountReport"
           WHERE "Date"::date = $1::date AND ("Status" IS NULL OR "Status" = 0)"#,
    )
    .bind(date)
    .fetch_one(pool)
    .await?;
    Ok(row.0)
}

/// Get all active tenant IDs from CentralDb (via the Hangfire/apalis DB which has access).
/// In practice we read from the central DB. Here we expose a helper that takes a pool
/// already connected to the central DB.
pub async fn get_all_tenant_ids(central_pool: &PgPool) -> Result<Vec<i64>> {
    let rows: Vec<(i64,)> = sqlx::query_as(
        r#"SELECT "Id" FROM pub."_Tenant" WHERE "Status" = 1"#,
    )
    .fetch_all(central_pool)
    .await?;
    Ok(rows.into_iter().map(|r| r.0).collect())
}

/// Get MT4 trade service connection string for a given service ID.
pub async fn get_mt4_connection_string(pool: &PgPool, service_id: i64) -> Result<Option<String>> {
    let row: Option<(String,)> = sqlx::query_as(
        r#"SELECT "Options" FROM trd."_TradeService"
           WHERE "Id" = $1 AND "Platform" = 1"#, // Platform 1 = MetaTrader4
    )
    .bind(service_id)
    .fetch_optional(pool)
    .await?;

    if let Some((options_json,)) = row {
        let v: serde_json::Value = serde_json::from_str(&options_json)?;
        if let Some(conn_str) = v["Database"]["ConnectionString"].as_str() {
            return Ok(Some(conn_str.to_string()));
        }
    }
    Ok(None)
}

/// Get MT5 trade service connection string for a given service ID.
pub async fn get_mt5_connection_string(pool: &PgPool, service_id: i64) -> Result<Option<String>> {
    let row: Option<(String,)> = sqlx::query_as(
        r#"SELECT "Options" FROM trd."_TradeService"
           WHERE "Id" = $1 AND "Platform" = 2"#, // Platform 2 = MetaTrader5
    )
    .bind(service_id)
    .fetch_optional(pool)
    .await?;

    if let Some((options_json,)) = row {
        let v: serde_json::Value = serde_json::from_str(&options_json)?;
        if let Some(conn_str) = v["Database"]["ConnectionString"].as_str() {
            return Ok(Some(conn_str.to_string()));
        }
    }
    Ok(None)
}
