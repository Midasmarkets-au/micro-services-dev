use anyhow::Result;
use chrono::{DateTime, Utc};
use serde::{Deserialize, Serialize};
use sqlx::PgPool;

use super::csv::to_csv_bytes;
use super::request::DateRangeCriteria;
use crate::report::ReportRequestType;

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct DepositRecord {
    pub id: i64,
    pub account_id: i64,
    pub account_number: Option<i64>,
    pub party_id: Option<i64>,
    pub amount: f64,
    pub currency: Option<String>,
    pub status: Option<i32>,
    pub created_on: Option<DateTime<Utc>>,
    pub approved_on: Option<DateTime<Utc>>,
    pub payment_method: Option<String>,
    pub reference: Option<String>,
}

pub async fn generate_deposit_csv(pool: &PgPool, criteria: &DateRangeCriteria) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(30));
    let to = criteria.to.unwrap_or_else(Utc::now);

    let records = sqlx::query_as::<_, DepositRecord>(
        r#"SELECT
            d."Id" as id,
            d."AccountId" as account_id,
            a."AccountNumber" as account_number,
            a."PartyId" as party_id,
            d."Amount" as amount,
            d."Currency" as currency,
            d."Status" as status,
            d."CreatedOn" as created_on,
            d."ApprovedOn" as approved_on,
            d."PaymentMethod" as payment_method,
            d."Reference" as reference
           FROM fin."_Deposit" d
           LEFT JOIN trd."_Account" a ON a."Id" = d."AccountId"
           WHERE d."CreatedOn" >= $1 AND d."CreatedOn" < $2
           ORDER BY d."CreatedOn""#,
    )
    .bind(from)
    .bind(to)
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct WithdrawalRecord {
    pub id: i64,
    pub account_id: i64,
    pub account_number: Option<i64>,
    pub party_id: Option<i64>,
    pub amount: f64,
    pub currency: Option<String>,
    pub status: Option<i32>,
    pub created_on: Option<DateTime<Utc>>,
    pub approved_on: Option<DateTime<Utc>>,
    pub payment_method: Option<String>,
}

pub async fn generate_withdrawal_csv(
    pool: &PgPool,
    criteria: &DateRangeCriteria,
    report_type: ReportRequestType,
) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(30));
    let to = criteria.to.unwrap_or_else(Utc::now);

    let is_pending = matches!(report_type, ReportRequestType::WithdrawPendingForTenant);

    let sql = if is_pending {
        r#"SELECT
            w."Id" as id,
            w."AccountId" as account_id,
            a."AccountNumber" as account_number,
            a."PartyId" as party_id,
            w."Amount" as amount,
            w."Currency" as currency,
            w."Status" as status,
            w."CreatedOn" as created_on,
            w."ApprovedOn" as approved_on,
            w."PaymentMethod" as payment_method
           FROM fin."_Withdrawal" w
           LEFT JOIN trd."_Account" a ON a."Id" = w."AccountId"
           WHERE w."CreatedOn" >= $1 AND w."CreatedOn" < $2
           AND w."Status" IN (0, 1)
           ORDER BY w."CreatedOn""#
    } else {
        r#"SELECT
            w."Id" as id,
            w."AccountId" as account_id,
            a."AccountNumber" as account_number,
            a."PartyId" as party_id,
            w."Amount" as amount,
            w."Currency" as currency,
            w."Status" as status,
            w."CreatedOn" as created_on,
            w."ApprovedOn" as approved_on,
            w."PaymentMethod" as payment_method
           FROM fin."_Withdrawal" w
           LEFT JOIN trd."_Account" a ON a."Id" = w."AccountId"
           WHERE w."CreatedOn" >= $1 AND w."CreatedOn" < $2
           ORDER BY w."CreatedOn""#
    };

    let records = sqlx::query_as::<_, WithdrawalRecord>(sql)
        .bind(from)
        .bind(to)
        .fetch_all(pool)
        .await?;

    to_csv_bytes(&records)
}

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct WalletTransactionRecord {
    pub id: i64,
    pub wallet_id: i64,
    pub party_id: Option<i64>,
    pub amount: f64,
    pub balance_after: Option<f64>,
    pub transaction_type: Option<i32>,
    pub stated_on: Option<DateTime<Utc>>,
    pub released_on: Option<DateTime<Utc>>,
    pub created_on: Option<DateTime<Utc>>,
    pub reference: Option<String>,
    pub comment: Option<String>,
}

pub async fn generate_wallet_transaction_csv(
    pool: &PgPool,
    criteria: &DateRangeCriteria,
    use_released_time: bool,
) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(1));
    let to = criteria.to.unwrap_or_else(Utc::now);

    let sql = if use_released_time {
        r#"SELECT
            wt."Id" as id,
            wt."WalletId" as wallet_id,
            w."PartyId" as party_id,
            wt."Amount" as amount,
            wt."BalanceAfter" as balance_after,
            wt."Type" as transaction_type,
            wt."StatedOn" as stated_on,
            wt."ReleasedOn" as released_on,
            wt."CreatedOn" as created_on,
            wt."Reference" as reference,
            wt."Comment" as comment
           FROM wlt."_WalletTransaction" wt
           LEFT JOIN wlt."_Wallet" w ON w."Id" = wt."WalletId"
           WHERE wt."StatedOn" >= $1 AND wt."StatedOn" < $2
           ORDER BY wt."StatedOn""#
    } else {
        r#"SELECT
            wt."Id" as id,
            wt."WalletId" as wallet_id,
            w."PartyId" as party_id,
            wt."Amount" as amount,
            wt."BalanceAfter" as balance_after,
            wt."Type" as transaction_type,
            wt."StatedOn" as stated_on,
            wt."ReleasedOn" as released_on,
            wt."CreatedOn" as created_on,
            wt."Reference" as reference,
            wt."Comment" as comment
           FROM wlt."_WalletTransaction" wt
           LEFT JOIN wlt."_Wallet" w ON w."Id" = wt."WalletId"
           WHERE wt."CreatedOn" >= $1 AND wt."CreatedOn" < $2
           ORDER BY wt."CreatedOn""#
    };

    let records = sqlx::query_as::<_, WalletTransactionRecord>(sql)
        .bind(from)
        .bind(to)
        .fetch_all(pool)
        .await?;

    to_csv_bytes(&records)
}

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct WalletSnapshotRecord {
    pub id: i64,
    pub wallet_id: i64,
    pub party_id: Option<i64>,
    pub balance: f64,
    pub snapshot_date: Option<DateTime<Utc>>,
}

#[derive(Debug, Deserialize)]
struct WalletSnapshotCriteria {
    #[serde(rename = "snapshotDate")]
    snapshot_date: Option<DateTime<Utc>>,
}

pub async fn generate_wallet_snapshot_csv(pool: &PgPool, query_json: &str) -> Result<Vec<u8>> {
    let criteria: WalletSnapshotCriteria = serde_json::from_str(query_json).unwrap_or(WalletSnapshotCriteria {
        snapshot_date: None,
    });
    let date = criteria.snapshot_date.unwrap_or_else(Utc::now);

    let records = sqlx::query_as::<_, WalletSnapshotRecord>(
        r#"SELECT
            s."Id" as id,
            s."WalletId" as wallet_id,
            w."PartyId" as party_id,
            s."Balance" as balance,
            s."SnapshotDate" as snapshot_date
           FROM wlt."_WalletDailySnapshot" s
           LEFT JOIN wlt."_Wallet" w ON w."Id" = s."WalletId"
           WHERE s."SnapshotDate"::date = $1::date
           ORDER BY s."WalletId""#,
    )
    .bind(date)
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct WalletOverviewRecord {
    pub wallet_id: i64,
    pub party_id: Option<i64>,
    pub balance: f64,
    pub currency: Option<String>,
    pub total_deposit: Option<f64>,
    pub total_withdrawal: Option<f64>,
}

pub async fn generate_wallet_overview_csv(pool: &PgPool, criteria: &DateRangeCriteria) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(30));
    let to = criteria.to.unwrap_or_else(Utc::now);

    let records = sqlx::query_as::<_, WalletOverviewRecord>(
        r#"SELECT
            w."Id" as wallet_id,
            w."PartyId" as party_id,
            w."Balance" as balance,
            w."Currency" as currency,
            (SELECT SUM(d."Amount") FROM fin."_Deposit" d
             JOIN trd."_Account" a ON a."Id" = d."AccountId"
             WHERE a."PartyId" = w."PartyId"
             AND d."ApprovedOn" >= $1 AND d."ApprovedOn" < $2) as total_deposit,
            (SELECT SUM(wd."Amount") FROM fin."_Withdrawal" wd
             JOIN trd."_Account" a ON a."Id" = wd."AccountId"
             WHERE a."PartyId" = w."PartyId"
             AND wd."ApprovedOn" >= $1 AND wd."ApprovedOn" < $2) as total_withdrawal
           FROM wlt."_Wallet" w
           ORDER BY w."Id""#,
    )
    .bind(from)
    .bind(to)
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct TransactionRecord {
    pub id: i64,
    pub account_id: i64,
    pub account_number: Option<i64>,
    pub amount: f64,
    pub transaction_type: Option<i32>,
    pub created_on: Option<DateTime<Utc>>,
    pub reference: Option<String>,
}

pub async fn generate_transaction_csv(pool: &PgPool, criteria: &DateRangeCriteria) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(30));
    let to = criteria.to.unwrap_or_else(Utc::now);

    let records = sqlx::query_as::<_, TransactionRecord>(
        r#"SELECT
            t."Id" as id,
            t."AccountId" as account_id,
            a."AccountNumber" as account_number,
            t."Amount" as amount,
            t."Type" as transaction_type,
            t."CreatedOn" as created_on,
            t."Reference" as reference
           FROM fin."_Transaction" t
           LEFT JOIN trd."_Account" a ON a."Id" = t."AccountId"
           WHERE t."CreatedOn" >= $1 AND t."CreatedOn" < $2
           ORDER BY t."CreatedOn""#,
    )
    .bind(from)
    .bind(to)
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}
