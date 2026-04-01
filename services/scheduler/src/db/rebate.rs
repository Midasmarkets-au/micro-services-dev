use anyhow::{anyhow, Result};
use chrono::Utc;
use serde::Deserialize;
use sqlx::PgPool;

// ── State constants (mirrors mono StateTypes) ────────────────────────────────

pub const STATE_REBATE_ON_HOLD: i32 = 510;
pub const STATE_REBATE_RELEASED: i32 = 520;
pub const STATE_REBATE_COMPLETED: i32 = 550;
pub const STATE_REBATE_CANCELED: i32 = 505;
pub const OPERATOR_PARTY_ID: i64 = 1;

// ── Structs ──────────────────────────────────────────────────────────────────

#[derive(Debug, sqlx::FromRow)]
pub struct RebateRow {
    #[allow(dead_code)]
    #[sqlx(rename = "Id")]
    pub id: i64,
    #[sqlx(rename = "AccountId")]
    pub account_id: i64,
    #[sqlx(rename = "Amount")]
    pub amount: i64,
    #[sqlx(rename = "StateId")]
    pub state_id: i32,
}

#[derive(Debug, sqlx::FromRow)]
pub struct AccountRow {
    #[sqlx(rename = "Id")]
    pub id: i64,
    #[sqlx(rename = "PartyId")]
    pub party_id: i64,
    #[sqlx(rename = "WalletId")]
    pub wallet_id: Option<i64>,
    #[sqlx(rename = "Status")]
    pub status: i16,
    #[sqlx(rename = "IsClosed")]
    pub is_closed: i32,
    #[sqlx(rename = "CurrencyId")]
    pub currency_id: i32,
    #[sqlx(rename = "FundType")]
    pub fund_type: i32,
    pub has_pause_tag: bool,
}

#[derive(Deserialize)]
struct BoolValue {
    #[serde(rename = "Value")]
    value: bool,
}

// ── Queries ──────────────────────────────────────────────────────────────────

/// Check if rebate feature is enabled for this tenant (siteId=0 = global config).
pub async fn is_rebate_enabled(pool: &PgPool) -> Result<bool> {
    let row: Option<(String,)> = sqlx::query_as(
        r#"SELECT "Value" FROM core."_Configuration"
           WHERE "Category" = 'Public' AND "Key" = 'RebateEnabled' AND "RowId" = 0
           LIMIT 1"#,
    )
    .fetch_optional(pool)
    .await?;

    match row {
        None => Ok(false),
        Some((json,)) => {
            let parsed: BoolValue = serde_json::from_str(&json)?;
            Ok(parsed.value)
        }
    }
}

/// Return IDs of rebates pending release (states 510 or 520), ordered by Id.
pub async fn get_pending_rebate_ids(pool: &PgPool) -> Result<Vec<i64>> {
    let rows: Vec<(i64,)> = sqlx::query_as(
        r#"SELECT r."Id"
           FROM trd."_Rebate" r
           INNER JOIN core."_Matter" m ON r."Id" = m."Id"
           WHERE m."StateId" IN ($1, $2)
           ORDER BY r."Id""#,
    )
    .bind(STATE_REBATE_ON_HOLD)
    .bind(STATE_REBATE_RELEASED)
    .fetch_all(pool)
    .await?;
    Ok(rows.into_iter().map(|r| r.0).collect())
}

/// Fetch rebate + matter details for a single rebate ID.
async fn get_rebate(tx: &mut sqlx::Transaction<'_, sqlx::Postgres>, rebate_id: i64) -> Result<Option<RebateRow>> {
    let row = sqlx::query_as::<_, RebateRow>(
        r#"SELECT r."Id", r."AccountId", r."Amount", m."StateId"
           FROM trd."_Rebate" r
           INNER JOIN core."_Matter" m ON r."Id" = m."Id"
           WHERE r."Id" = $1"#,
    )
    .bind(rebate_id)
    .fetch_optional(&mut **tx)
    .await?;
    Ok(row)
}

/// Fetch account with pause-tag check.
async fn get_account(tx: &mut sqlx::Transaction<'_, sqlx::Postgres>, account_id: i64) -> Result<Option<AccountRow>> {
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

/// Transit matter state and record activity (within an open transaction).
async fn transit_state(
    tx: &mut sqlx::Transaction<'_, sqlx::Postgres>,
    matter_id: i64,
    from_state: i32,
    to_state: i32,
    note: &str,
) -> Result<()> {
    let now = Utc::now();
    sqlx::query(
        r#"INSERT INTO core."_Activity"
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

    sqlx::query(r#"UPDATE core."_Matter" SET "StateId" = $1, "StatedOn" = $2 WHERE "Id" = $3"#)
        .bind(to_state)
        .bind(now)
        .bind(matter_id)
        .execute(&mut **tx)
        .await?;

    Ok(())
}

/// Process a single rebate: validate, update wallet, transit state. Atomic.
pub async fn process_rebate(pool: &PgPool, rebate_id: i64) -> Result<()> {
    let mut tx = pool.begin().await?;

    // 1. Load rebate
    let rebate = match get_rebate(&mut tx, rebate_id).await? {
        Some(r) => r,
        None => return Err(anyhow!("Rebate {} not found", rebate_id)),
    };

    // 2. Load account with pause-tag check
    let account = get_account(&mut tx, rebate.account_id).await?;

    // 3. Validate account (must exist, not closed, status == 0 = Activate)
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

            let wid = row.map(|r| r.0).ok_or_else(|| anyhow!("No wallet found for account {}", rebate.account_id))?;

            sqlx::query(r#"UPDATE trd."_Account" SET "WalletId" = $1 WHERE "Id" = $2"#)
                .bind(wid)
                .bind(account.id)
                .execute(&mut *tx)
                .await?;

            wid
        }
    };

    // 6. Lock wallet row (FOR UPDATE)
    let wallet_row: (i64,) = sqlx::query_as(
        r#"SELECT "Balance" FROM acct."_Wallet" WHERE "Id" = $1 FOR UPDATE"#,
    )
    .bind(wallet_id)
    .fetch_one(&mut *tx)
    .await?;
    let prev_balance = wallet_row.0;

    // 7. Idempotency: if WalletTransaction already exists, just complete
    let already_released: (bool,) = sqlx::query_as(
        r#"SELECT EXISTS(SELECT 1 FROM acct."_WalletTransaction" WHERE "MatterId" = $1)"#,
    )
    .bind(rebate_id)
    .fetch_one(&mut *tx)
    .await?;

    if already_released.0 {
        transit_state(&mut tx, rebate_id, rebate.state_id, STATE_REBATE_COMPLETED, "Release By System").await?;
        tx.commit().await?;
        return Ok(());
    }

    // 8-10. Update wallet balance and insert transaction
    let new_balance = prev_balance + rebate.amount;
    let now = Utc::now();

    sqlx::query(r#"UPDATE acct."_Wallet" SET "Balance" = $1 WHERE "Id" = $2"#)
        .bind(new_balance)
        .bind(wallet_id)
        .execute(&mut *tx)
        .await?;

    sqlx::query(
        r#"INSERT INTO acct."_WalletTransaction"
           ("WalletId", "MatterId", "PrevBalance", "Amount", "CreatedOn", "UpdatedOn")
           VALUES ($1, $2, $3, $4, $5, $5)"#,
    )
    .bind(wallet_id)
    .bind(rebate_id)
    .bind(prev_balance)
    .bind(rebate.amount)
    .bind(now)
    .execute(&mut *tx)
    .await?;

    // 11-12. Transit state to completed
    transit_state(&mut tx, rebate_id, rebate.state_id, STATE_REBATE_COMPLETED, "Release By System").await?;

    tx.commit().await?;
    Ok(())
}
