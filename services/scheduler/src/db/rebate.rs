use anyhow::{anyhow, Result};
use chrono::Utc;
use rust_decimal::prelude::ToPrimitive;
use rust_decimal::Decimal;
use sqlx::PgPool;

use crate::db::rebate_calc::REBATE_TABLE;

// ── State constants ───────────────────────────────────────────────────────────

pub const STATE_REBATE_ON_HOLD: i32 = 510;
pub const STATE_REBATE_RELEASED: i32 = 520;
pub const STATE_REBATE_COMPLETED: i32 = 550;
pub const STATE_REBATE_CANCELED: i32 = 505;
pub const OPERATOR_PARTY_ID: i64 = 1;

// ── Structs ───────────────────────────────────────────────────────────────────

#[derive(Debug, sqlx::FromRow)]
struct RebateRow {
    #[sqlx(rename = "AccountId")]
    account_id: i64,
    #[sqlx(rename = "Amount")]
    amount: Decimal,
    #[sqlx(rename = "StateId")]
    state_id: i32,
}

#[derive(Debug, sqlx::FromRow)]
struct AccountRow {
    #[sqlx(rename = "Id")]
    id: i64,
    #[sqlx(rename = "PartyId")]
    party_id: i64,
    #[sqlx(rename = "WalletId")]
    wallet_id: Option<i64>,
    #[sqlx(rename = "Status")]
    status: i16,
    #[sqlx(rename = "IsClosed")]
    is_closed: i32,
    #[sqlx(rename = "CurrencyId")]
    currency_id: i32,
    #[sqlx(rename = "FundType")]
    fund_type: i32,
    has_pause_tag: bool,
}

// ── Pending rebate query ──────────────────────────────────────────────────────

/// Return rebate IDs pending release (StateId IN 510, 520) from trd.rebate_k8s.
/// PG partition pruning handles the date range automatically.
pub async fn get_pending_rebate_ids(pool: &PgPool) -> Result<Vec<i64>> {
    let rows: Vec<(i64,)> = sqlx::query_as(&format!(
        r#"SELECT r."Id"
           FROM {REBATE_TABLE} r
           INNER JOIN core."_MatterK8s" m ON r."Id" = m."Id"
           WHERE m."StateId" IN ($1, $2)
           ORDER BY r."Id""#
    ))
    .bind(STATE_REBATE_ON_HOLD)
    .bind(STATE_REBATE_RELEASED)
    .fetch_all(pool)
    .await?;

    Ok(rows.into_iter().map(|(id,)| id).collect())
}

// ── Inner helpers ─────────────────────────────────────────────────────────────

async fn get_rebate(
    tx: &mut sqlx::Transaction<'_, sqlx::Postgres>,
    rebate_id: i64,
) -> Result<Option<RebateRow>> {
    let row = sqlx::query_as::<_, RebateRow>(&format!(
        r#"SELECT r."AccountId", r."Amount", m."StateId"
           FROM {REBATE_TABLE} r
           INNER JOIN core."_MatterK8s" m ON r."Id" = m."Id"
           WHERE r."Id" = $1"#
    ))
    .bind(rebate_id)
    .fetch_optional(&mut **tx)
    .await?;
    Ok(row)
}

async fn get_account(
    tx: &mut sqlx::Transaction<'_, sqlx::Postgres>,
    account_id: i64,
) -> Result<Option<AccountRow>> {
    let row = sqlx::query_as::<_, AccountRow>(
        r#"SELECT a."Id", a."PartyId", a."WalletId", a."Status", a."IsClosed",
                  a."CurrencyId", a."FundType",
                  EXISTS(
                    SELECT 1 FROM trd."_AccountWithTag" awt
                    INNER JOIN core."_Tag" t ON awt."TagId" = t."Id"
                    WHERE awt."AccountId" = a."Id" AND t."Name" = 'PauseReleaseRebate'
                  ) AS has_pause_tag
           FROM trd."_Account" a
           WHERE a."Id" = $1"#,
    )
    .bind(account_id)
    .fetch_optional(&mut **tx)
    .await?;
    Ok(row)
}

/// Update matter state in _MatterK8s and append activity to core.activity_k8s.
async fn transit_state(
    tx: &mut sqlx::Transaction<'_, sqlx::Postgres>,
    matter_id: i64,
    from_state: i32,
    to_state: i32,
    note: &str,
) -> Result<()> {
    let now = Utc::now();

    sqlx::query(
        r#"INSERT INTO core.activity_k8s
           ("MatterId", "PartyId", "PerformedOn", "OnStateId", "ActionId", "ToStateId", "Data")
           VALUES ($1, $2, $3, $4, 0, $5, $6)"#,
    )
    .bind(matter_id)
    .bind(OPERATOR_PARTY_ID)
    .bind(now)
    .bind(from_state)
    .bind(to_state)
    .bind(note)
    .execute(&mut **tx)
    .await?;

    sqlx::query(
        r#"UPDATE core."_MatterK8s" SET "StateId" = $1, "StatedOn" = $2 WHERE "Id" = $3"#
    )
    .bind(to_state)
    .bind(now)
    .bind(matter_id)
    .execute(&mut **tx)
    .await?;

    Ok(())
}

// ── Main process ──────────────────────────────────────────────────────────────

/// Process a single rebate atomically: validate, update wallet, transit state.
pub async fn process_rebate(pool: &PgPool, rebate_id: i64) -> Result<()> {
    let mut tx = pool.begin().await?;

    // 1. Load rebate from partitioned table (PG routes via CreatedOn partition key)
    let rebate = match get_rebate(&mut tx, rebate_id).await? {
        Some(r) => r,
        None => return Err(anyhow!("Rebate {} not found", rebate_id)),
    };

    // 2. Load account with pause-tag check
    let account = get_account(&mut tx, rebate.account_id).await?;

    // 3. Validate account (must exist, not closed, Status == 0 = Activate)
    let account = match account {
        Some(a) if a.is_closed == 0 && a.status == 0 => a,
        _ => {
            transit_state(&mut tx, rebate_id, rebate.state_id, STATE_REBATE_CANCELED, "Account invalid or closed").await?;
            tx.commit().await?;
            return Ok(());
        }
    };

    // 4. Check pause tag
    if account.has_pause_tag {
        transit_state(&mut tx, rebate_id, rebate.state_id, STATE_REBATE_CANCELED, "Receiver account release paused").await?;
        tx.commit().await?;
        return Ok(());
    }

    // 5. Resolve wallet ID
    let wallet_id = match account.wallet_id {
        Some(wid) => wid,
        None => {
            let row: Option<(i64,)> = sqlx::query_as(
                r#"SELECT "Id" FROM acct."_Wallet"
                   WHERE "PartyId" = $1 AND "CurrencyId" = $2 AND "FundType" = $3
                   LIMIT 1"#,
            )
            .bind(account.party_id)
            .bind(account.currency_id)
            .bind(account.fund_type)
            .fetch_optional(&mut *tx)
            .await?;

            let wid = row
                .map(|r| r.0)
                .ok_or_else(|| anyhow!("No wallet found for account {}", rebate.account_id))?;

            sqlx::query(r#"UPDATE trd."_Account" SET "WalletId" = $1 WHERE "Id" = $2"#)
                .bind(wid)
                .bind(account.id)
                .execute(&mut *tx)
                .await?;

            wid
        }
    };

    // 6. Lock wallet row (FOR UPDATE prevents concurrent double-credit)
    let wallet_row: (i64,) = sqlx::query_as(
        r#"SELECT "Balance" FROM acct."_Wallet" WHERE "Id" = $1 FOR UPDATE"#,
    )
    .bind(wallet_id)
    .fetch_one(&mut *tx)
    .await?;
    let prev_balance = wallet_row.0;

    // 7. Idempotency check — if WalletTransaction already exists, just complete
    let already_released: (bool,) = sqlx::query_as(
        r#"SELECT EXISTS(SELECT 1 FROM acct.wallet_transaction_k8s WHERE "MatterId" = $1)"#,
    )
    .bind(rebate_id)
    .fetch_one(&mut *tx)
    .await?;

    if already_released.0 {
        transit_state(&mut tx, rebate_id, rebate.state_id, STATE_REBATE_COMPLETED, "Release By System").await?;
        tx.commit().await?;
        return Ok(());
    }

    // 8. Amount is NUMERIC(20,8) in cents — round to whole cents before writing to wallet
    let amount_cents = rebate.amount
        .round_dp(0)
        .to_i64()
        .ok_or_else(|| anyhow!("Rebate amount out of i64 range: {}", rebate.amount))?;

    let new_balance = prev_balance + amount_cents;
    let now = Utc::now();

    // 9-10. Update wallet balance and insert transaction record
    sqlx::query(r#"UPDATE acct."_Wallet" SET "Balance" = $1 WHERE "Id" = $2"#)
        .bind(new_balance)
        .bind(wallet_id)
        .execute(&mut *tx)
        .await?;

    sqlx::query(
        r#"INSERT INTO acct.wallet_transaction_k8s
           ("WalletId", "MatterId", "PrevBalance", "Amount", "CreatedOn", "UpdatedOn")
           VALUES ($1, $2, $3, $4, $5, $5)"#,
    )
    .bind(wallet_id)
    .bind(rebate_id)
    .bind(prev_balance)
    .bind(amount_cents)
    .bind(now)
    .execute(&mut *tx)
    .await?;

    // 11-12. Transit state to completed
    transit_state(&mut tx, rebate_id, rebate.state_id, STATE_REBATE_COMPLETED, "Release By System").await?;

    tx.commit().await?;
    Ok(())
}
