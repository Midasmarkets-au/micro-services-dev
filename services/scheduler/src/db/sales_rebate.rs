use anyhow::{anyhow, Result};
use chrono::{DateTime, Utc};
use rust_decimal::Decimal;
use sqlx::PgPool;
use tracing::info;

const MATTER_TYPE_SALES_REBATE: i32 = 600;
const STATE_SALES_REBATE_PENDING:  i32 = 0;
const STATE_SALES_REBATE_RELEASED: i32 = 1;
const OPERATOR_PARTY_ID: i64 = 1;

/// Active SalesRebateSchema row.
#[derive(Debug, sqlx::FromRow)]
pub struct SalesRebateSchemaRow {
    #[sqlx(rename = "SalesAccountId")]
    pub sales_account_id: i64,
    /// Payee account. Equals sales_account_id for normal (front-line) schemas;
    /// differs for override schemas where an upline account collects on a downline's volume.
    #[sqlx(rename = "RebateAccountId")]
    pub rebate_account_id: i64,
    #[sqlx(rename = "Rebate")]
    pub rebate: i32,
    #[sqlx(rename = "AlphaRebate")]
    pub alpha_rebate: i32,
    #[sqlx(rename = "ProRebate")]
    pub pro_rebate: i32,
    #[sqlx(rename = "ExcludeSymbol")]
    pub exclude_symbol: String,
    #[sqlx(rename = "ExcludeAccount")]
    pub exclude_account: String,
    #[sqlx(rename = "AutoRelease")]
    pub auto_release: bool,
}

/// Trade record with joined account info for a settlement period.
#[derive(Debug, sqlx::FromRow)]
pub struct PeriodTrade {
    pub trade_rebate_id: i64,
    pub ticket: i64,
    pub trade_account_id: i64,
    pub account_number: i64,
    pub uid: i64,
    pub account_type: i16,
    pub fund_type: i32,
    pub currency_id: i32,
    pub symbol: String,
    pub volume: i32,
    pub closed_on: DateTime<Utc>,
}

/// Data for a new sales_rebate_k8s summary record.
pub struct NewSalesRebateSummary {
    pub sales_account_id: i64,
    pub rebate_account_id: i64,
    pub period_start: DateTime<Utc>,
    pub period_end: DateTime<Utc>,
    pub schedule_type: i16,
    pub total_amount: Decimal,
    pub trade_count: i32,
    pub matter_id: i64,
}

/// Data for a new sales_rebate_item_k8s detail record.
pub struct NewSalesRebateItem {
    pub sales_account_id: i64,
    pub trade_rebate_id: i64,
    pub ticket: i64,
    pub trade_account_id: i64,
    pub trade_account_number: i64,
    pub trade_account_type: i16,
    pub trade_account_fund_type: i32,
    pub trade_account_currency_id: i32,
    pub symbol: String,
    pub volume: i32,
    pub rebate_base: Decimal,
    pub rebate_type: String,
    pub amount: Decimal,
    pub closed_on: DateTime<Utc>,
    pub excluded: bool,
}

/// Idempotency check: returns true if a summary record already exists for
/// (sales_account_id, rebate_account_id, period_start).
pub async fn summary_exists(
    pool: &PgPool,
    sales_account_id: i64,
    rebate_account_id: i64,
    period_start: DateTime<Utc>,
) -> Result<bool> {
    let row: (bool,) = sqlx::query_as(
        r#"SELECT EXISTS (
               SELECT 1 FROM trd.sales_rebate_k8s
               WHERE sales_account_id = $1 AND rebate_account_id = $2 AND period_start = $3
           )"#,
    )
    .bind(sales_account_id)
    .bind(rebate_account_id)
    .bind(period_start)
    .fetch_one(pool)
    .await?;
    Ok(row.0)
}

/// Returns active (Status=0) schemas filtered by schedule_type (0=Daily, 3=Monthly).
pub async fn get_active_schemas_by_schedule(
    pool: &PgPool,
    schedule_type: i16,
) -> Result<Vec<SalesRebateSchemaRow>> {
    let rows = sqlx::query_as::<_, SalesRebateSchemaRow>(
        r#"SELECT "SalesAccountId", "RebateAccountId", "Rebate", "AlphaRebate", "ProRebate",
                  "ExcludeSymbol"::text, "ExcludeAccount"::text,
                  COALESCE("AutoRelease", false) AS "AutoRelease"
           FROM trd."_SalesRebateSchema"
           WHERE "Status" = 0
             AND "Schedule" = $1"#,
    )
    .bind(schedule_type)
    .fetch_all(pool)
    .await?;
    Ok(rows)
}

/// Returns all trades in [period_start, period_end) for a given SalesAccountId,
/// joined with account info. Only includes trades where account_id IS NOT NULL.
pub async fn get_period_trades(
    pool: &PgPool,
    sales_account_id: i64,
    period_start: DateTime<Utc>,
    period_end: DateTime<Utc>,
) -> Result<Vec<PeriodTrade>> {
    let rows = sqlx::query_as::<_, PeriodTrade>(
        r#"SELECT tr.id AS trade_rebate_id,
                  tr.ticket,
                  tr.account_id AS trade_account_id,
                  a."AccountNumber" AS account_number,
                  a."Uid" AS uid,
                  a."Type" AS account_type,
                  a."FundType" AS fund_type,
                  a."CurrencyId" AS currency_id,
                  tr.symbol,
                  tr.volume,
                  tr.closed_on
           FROM trd.trade_rebate_k8s tr
           JOIN trd."_Account" a ON a."Id" = tr.account_id
           WHERE a."SalesAccountId" = $1
             AND tr.closed_on >= $2
             AND tr.closed_on < $3
             AND tr.account_id IS NOT NULL"#,
    )
    .bind(sales_account_id)
    .bind(period_start)
    .bind(period_end)
    .fetch_all(pool)
    .await?;
    Ok(rows)
}

/// Inserts a summary record and all item records in a single transaction.
/// Returns (rebate_id, created_on).
pub async fn insert_batch(
    pool: &PgPool,
    summary: &NewSalesRebateSummary,
    items: &[NewSalesRebateItem],
) -> Result<(i64, DateTime<Utc>)> {
    let mut tx = pool.begin().await?;

    // Create matter_k8s record for audit trail
    sqlx::query(
        r#"INSERT INTO core.matter_k8s (id, type, state_id, posted_on, stated_on)
           VALUES ($1, $2, $3, NOW(), NOW())"#,
    )
    .bind(summary.matter_id)
    .bind(MATTER_TYPE_SALES_REBATE)
    .bind(STATE_SALES_REBATE_PENDING)
    .execute(&mut *tx)
    .await?;

    let row: (i64, DateTime<Utc>) = sqlx::query_as(
        r#"INSERT INTO trd.sales_rebate_k8s
               (sales_account_id, rebate_account_id, period_start, period_end, schedule_type, total_amount, trade_count, matter_id)
           VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
           RETURNING id, created_on"#,
    )
    .bind(summary.sales_account_id)
    .bind(summary.rebate_account_id)
    .bind(summary.period_start)
    .bind(summary.period_end)
    .bind(summary.schedule_type)
    .bind(summary.total_amount)
    .bind(summary.trade_count)
    .bind(summary.matter_id)
    .fetch_one(&mut *tx)
    .await?;

    let (rebate_id, created_on) = row;

    for item in items {
        sqlx::query(
            r#"INSERT INTO trd.sales_rebate_item_k8s
                   (sales_rebate_id, sales_account_id, trade_rebate_id, ticket,
                    trade_account_id, trade_account_number, trade_account_type,
                    trade_account_fund_type, trade_account_currency_id,
                    symbol, volume, rebate_base, rebate_type, amount, closed_on, excluded)
               VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15, $16)"#,
        )
        .bind(rebate_id)
        .bind(item.sales_account_id)
        .bind(item.trade_rebate_id)
        .bind(item.ticket)
        .bind(item.trade_account_id)
        .bind(item.trade_account_number)
        .bind(item.trade_account_type)
        .bind(item.trade_account_fund_type)
        .bind(item.trade_account_currency_id)
        .bind(&item.symbol)
        .bind(item.volume)
        .bind(item.rebate_base)
        .bind(&item.rebate_type)
        .bind(item.amount)
        .bind(item.closed_on)
        .bind(item.excluded)
        .execute(&mut *tx)
        .await?;
    }

    tx.commit().await?;
    Ok((rebate_id, created_on))
}

/// Deletes an existing summary and all its items for a given
/// (sales_account_id, rebate_account_id, period_start).
/// Used by the force-recalculate path to clear stale data before regenerating.
pub async fn delete_period_summary(
    pool: &PgPool,
    sales_account_id: i64,
    rebate_account_id: i64,
    period_start: DateTime<Utc>,
) -> Result<()> {
    sqlx::query(
        r#"DELETE FROM trd.sales_rebate_item_k8s
           WHERE sales_rebate_id IN (
               SELECT id FROM trd.sales_rebate_k8s
               WHERE sales_account_id = $1 AND rebate_account_id = $2 AND period_start = $3
           )"#,
    )
    .bind(sales_account_id)
    .bind(rebate_account_id)
    .bind(period_start)
    .execute(pool)
    .await?;

    sqlx::query(
        r#"DELETE FROM trd.sales_rebate_k8s
           WHERE sales_account_id = $1 AND rebate_account_id = $2 AND period_start = $3"#,
    )
    .bind(sales_account_id)
    .bind(rebate_account_id)
    .bind(period_start)
    .execute(pool)
    .await?;

    Ok(())
}

/// Releases a sales_rebate_k8s summary to the payee (target) account's wallet.
/// `target_account_id` is the schema's RebateAccountId — the upline collector for
/// override schemas, or the sales account itself for normal front-line schemas.
/// Idempotent: if wallet_transaction_k8s already has matter_id=rebate_id, skips wallet ops.
/// Returns the wallet_transaction_id.
///
/// _Wallet.Balance is an old table using bigint×10000 convention.
/// wallet_transaction_k8s uses NUMERIC(20,4) — conversion applied at the boundary.
pub async fn release_summary(
    pool: &PgPool,
    rebate_id: i64,
    created_on: DateTime<Utc>,
    target_account_id: i64,
    total_amount: Decimal,
    matter_id: Option<i64>,
) -> Result<i64> {
    let mut tx = pool.begin().await?;

    // 1. Load payee (target) account → party_id, currency_id, fund_type, wallet_id
    let acct: (i64, i32, i32, Option<i64>) = sqlx::query_as(
        r#"SELECT "PartyId", "CurrencyId", "FundType", "WalletId"
           FROM trd."_Account"
           WHERE "Id" = $1"#,
    )
    .bind(target_account_id)
    .fetch_one(&mut *tx)
    .await
    .map_err(|e| anyhow!("rebate target account {} not found: {:#}", target_account_id, e))?;

    let (party_id, currency_id, fund_type, maybe_wallet_id) = acct;

    // 2. Resolve wallet id
    let wallet_id = if let Some(wid) = maybe_wallet_id {
        wid
    } else {
        let row: Option<(i64,)> = sqlx::query_as(
            r#"SELECT "Id" FROM acct."_Wallet"
               WHERE "PartyId" = $1 AND "CurrencyId" = $2 AND "FundType" = $3
               LIMIT 1"#,
        )
        .bind(party_id)
        .bind(currency_id)
        .bind(fund_type)
        .fetch_optional(&mut *tx)
        .await?;

        let wid = row.map(|r| r.0).ok_or_else(|| {
            anyhow!(
                "no wallet for rebate target account {} (party={} currency={} fund={})",
                target_account_id, party_id, currency_id, fund_type
            )
        })?;

        sqlx::query(r#"UPDATE trd."_Account" SET "WalletId" = $1 WHERE "Id" = $2"#)
            .bind(wid)
            .bind(target_account_id)
            .execute(&mut *tx)
            .await?;

        wid
    };

    // 3. Lock wallet row — _Wallet.Balance is bigint×10000 (old table convention)
    let wallet_balance: (i64,) = sqlx::query_as(
        r#"SELECT "Balance" FROM acct."_Wallet" WHERE "Id" = $1 FOR UPDATE"#,
    )
    .bind(wallet_id)
    .fetch_one(&mut *tx)
    .await
    .map_err(|e| anyhow!("wallet {} not found: {:#}", wallet_id, e))?;

    // Convert old-table bigint×10000 → Decimal for wallet_transaction_k8s
    let prev_balance = Decimal::from(wallet_balance.0) / Decimal::from(10000);

    // 4. Idempotency: check existing wallet_transaction_k8s
    let existing: Option<(i64,)> = sqlx::query_as(
        r#"SELECT id FROM acct.wallet_transaction_k8s WHERE matter_id = $1 LIMIT 1"#,
    )
    .bind(rebate_id)
    .fetch_optional(&mut *tx)
    .await?;

    if let Some((wt_id,)) = existing {
        sqlx::query(
            r#"UPDATE trd.sales_rebate_k8s
               SET status = 1, wallet_transaction_id = $1
               WHERE id = $2 AND created_on = $3"#,
        )
        .bind(wt_id)
        .bind(rebate_id)
        .bind(created_on)
        .execute(&mut *tx)
        .await?;
        tx.commit().await?;
        info!("SalesRebate: release idempotent rebate_id={} wt_id={}", rebate_id, wt_id);
        return Ok(wt_id);
    }

    let now = Utc::now();

    // 5. Update _Wallet.Balance — convert back to bigint×10000
    use rust_decimal::prelude::ToPrimitive;
    let new_balance_raw = ((prev_balance + total_amount) * Decimal::from(10000))
        .to_i64()
        .ok_or_else(|| anyhow!("overflow converting wallet balance to i64"))?;
    sqlx::query(r#"UPDATE acct."_Wallet" SET "Balance" = $1 WHERE "Id" = $2"#)
        .bind(new_balance_raw)
        .bind(wallet_id)
        .execute(&mut *tx)
        .await?;

    // 6. Insert wallet_transaction_k8s — NUMERIC(20,4) columns
    let wt_row: (i64,) = sqlx::query_as(
        r#"INSERT INTO acct.wallet_transaction_k8s
               (wallet_id, matter_id, prev_balance, amount, created_on, updated_on)
           VALUES ($1, $2, $3, $4, $5, $5)
           RETURNING id"#,
    )
    .bind(wallet_id)
    .bind(rebate_id)
    .bind(prev_balance)
    .bind(total_amount)
    .bind(now)
    .fetch_one(&mut *tx)
    .await?;
    let wt_id = wt_row.0;

    // 7. Update sales_rebate_k8s status
    sqlx::query(
        r#"UPDATE trd.sales_rebate_k8s
           SET status = 1, wallet_transaction_id = $1
           WHERE id = $2 AND created_on = $3"#,
    )
    .bind(wt_id)
    .bind(rebate_id)
    .bind(created_on)
    .execute(&mut *tx)
    .await?;

    // 8. Audit: update matter_k8s state + insert activity_k8s
    if let Some(mid) = matter_id {
        let audit_now = Utc::now();
        sqlx::query(
            r#"UPDATE core.matter_k8s SET state_id = $1, stated_on = $2 WHERE id = $3"#,
        )
        .bind(STATE_SALES_REBATE_RELEASED)
        .bind(audit_now)
        .bind(mid)
        .execute(&mut *tx)
        .await?;

        sqlx::query(
            r#"INSERT INTO core.activity_k8s
               (matter_id, party_id, performed_on, on_state_id, action_id, to_state_id, data)
               VALUES ($1, $2, $3, $4, 0, $5, $6)"#,
        )
        .bind(mid)
        .bind(OPERATOR_PARTY_ID)
        .bind(audit_now)
        .bind(STATE_SALES_REBATE_PENDING)
        .bind(STATE_SALES_REBATE_RELEASED)
        .bind(format!("SalesRebate auto-released: rebate_id={} wt_id={}", rebate_id, wt_id))
        .execute(&mut *tx)
        .await?;
    }

    tx.commit().await?;
    Ok(wt_id)
}
