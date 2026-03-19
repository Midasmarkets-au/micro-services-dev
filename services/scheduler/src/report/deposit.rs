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
           FROM acct."_Deposit" d
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
           FROM acct."_Withdrawal" w
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
           FROM acct."_Withdrawal" w
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
    pub wallet_id: i64,
    pub transaction_id: i64,
    pub party_id: Option<i64>,
    pub fund_type: Option<i32>,
    pub matter_type: Option<i32>,
    pub state: Option<i32>,
    pub amount: i64,
    pub prev_balance: i64,
    pub created_on: Option<DateTime<Utc>>,
    pub released_on: Option<DateTime<Utc>>,
}

pub async fn generate_wallet_transaction_csv(
    pool: &PgPool,
    criteria: &DateRangeCriteria,
    use_released_time: bool,
) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(1));
    let to = criteria.to.unwrap_or_else(Utc::now);

    // is_from_api=1 (use_released_time=true): filter by wt.UpdatedOn
    // is_from_api=0 (use_released_time=false): filter by m.StatedOn (job entry)
    let sql = if use_released_time {
        r#"SELECT
            wt."WalletId" as wallet_id,
            wt."Id" as transaction_id,
            w."PartyId" as party_id,
            w."FundType" as fund_type,
            m."Type" as matter_type,
            m."StateId" as state,
            wt."Amount" as amount,
            wt."PrevBalance" as prev_balance,
            m."PostedOn" as created_on,
            m."StatedOn" as released_on
           FROM acct."_WalletTransaction" wt
           LEFT JOIN core."_Matter" m ON m."Id" = wt."MatterId"
           LEFT JOIN acct."_Wallet" w ON w."Id" = wt."WalletId"
           WHERE wt."UpdatedOn" >= $1 AND wt."UpdatedOn" <= $2
           ORDER BY wt."Id""#
    } else {
        r#"SELECT
            wt."WalletId" as wallet_id,
            wt."Id" as transaction_id,
            w."PartyId" as party_id,
            w."FundType" as fund_type,
            m."Type" as matter_type,
            m."StateId" as state,
            wt."Amount" as amount,
            wt."PrevBalance" as prev_balance,
            m."PostedOn" as created_on,
            m."StatedOn" as released_on
           FROM acct."_WalletTransaction" wt
           LEFT JOIN core."_Matter" m ON m."Id" = wt."MatterId"
           LEFT JOIN acct."_Wallet" w ON w."Id" = wt."WalletId"
           WHERE m."StatedOn" >= $1 AND m."StatedOn" < $2
           ORDER BY wt."Id""#
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
           FROM acct."_WalletDailySnapshot" s
           LEFT JOIN acct."_Wallet" w ON w."Id" = s."WalletId"
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
            (SELECT SUM(d."Amount") FROM acct."_Deposit" d
             JOIN trd."_Account" a ON a."Id" = d."AccountId"
             WHERE a."PartyId" = w."PartyId"
             AND d."ApprovedOn" >= $1 AND d."ApprovedOn" < $2) as total_deposit,
            (SELECT SUM(wd."Amount") FROM acct."_Withdrawal" wd
             JOIN trd."_Account" a ON a."Id" = wd."AccountId"
             WHERE a."PartyId" = w."PartyId"
             AND wd."ApprovedOn" >= $1 AND wd."ApprovedOn" < $2) as total_withdrawal
           FROM acct."_Wallet" w
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

#[cfg(test)]
mod tests {
    use super::*;
    use crate::config::Config;
    use crate::storage::s3::S3Storage;

    #[tokio::test]
    async fn test_wallet_transaction_csv_and_s3_upload() {
        dotenvy::dotenv().ok();
        let config = Config::from_env().expect("config");

        let db_url = format!(
            "postgresql://{}:{}@{}:{}/portal_tenant_bvi",
            config.db_user, config.db_password, config.db_host, config.db_port
        );
        let pool = sqlx::PgPool::connect(&db_url).await.expect("db connect");
        let s3 = S3Storage::new(&config).await.expect("s3 init");

        let criteria = DateRangeCriteria {
            from: Some("2026-01-01T00:00:00Z".parse().unwrap()),
            to: Some("2026-03-19T23:59:59Z".parse().unwrap()),
        };

        let csv_bytes = generate_wallet_transaction_csv(&pool, &criteria, false)
            .await
            .expect("generate csv");

        println!("CSV rows bytes: {}", csv_bytes.len());
        println!("CSV preview:\n{}", String::from_utf8_lossy(&csv_bytes[..csv_bytes.len().min(500)]));

        let key = "test/wallet_transaction_test.csv".to_string();
        s3.upload_csv(&key, csv_bytes).await.expect("s3 upload");
        println!("Uploaded to S3 key: {}", key);
    }
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
           FROM acct."_Transaction" t
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
