use anyhow::Result;
use sqlx::{MySqlPool, PgPool};

use crate::grpc::idgen_client::IdgenClient;
use crate::models::rebate::{
    AgentAccount, DirectRule, DirectSchemaItem, MtPrice, NewRebate, TargetAccount, TradeRebateRow,
    MATTER_TYPE_REBATE, STATE_REBATE_ON_HOLD,
};

// ── Table name helpers ────────────────────────────────────────────────────────

/// snake_case partitioned table for trade rebate records (scheduler-only).
pub const TRADE_REBATE_TABLE: &str = "trd.trade_rebate_k8s";

/// snake_case partitioned table for rebate records (scheduler-only).
/// Replaces the application-level year tables trd."_Rebate_{year}".
pub const REBATE_TABLE: &str = "trd.rebate_k8s";

// ── Configuration ─────────────────────────────────────────────────────────────

/// Check if rebate calculation is enabled for this tenant (siteId=0 = global).
/// Reads cfg."_Configuration" where Category='Public', Key='RebateEnabled'.
pub async fn is_rebate_enabled(pool: &PgPool) -> Result<bool> {
    let row: Option<(String,)> = sqlx::query_as(
        r#"SELECT "Value" FROM core."_Configuration"
           WHERE "Category" = 'Public'
             AND "Key" = 'RebateEnabled'
             AND "RowId" = 0
           LIMIT 1"#,
    )
    .fetch_optional(pool)
    .await?;

    if let Some((value,)) = row {
        // Value is JSON: {"Value": true}
        if let Ok(v) = serde_json::from_str::<serde_json::Value>(&value) {
            return Ok(v["Value"].as_bool().unwrap_or(false));
        }
    }
    Ok(false)
}

// ── Pagination helpers ────────────────────────────────────────────────────────

/// Returns (min_id, max_id) of pending records (Status=0 or Status=5) in the given table.
/// Returns (0, 0) if no pending records.
pub async fn get_min_max_id(pool: &PgPool, table: &str) -> Result<(i64, i64)> {
    let row: Option<(Option<i64>, Option<i64>)> = sqlx::query_as(&format!(
        r#"SELECT MIN(id), MAX(id) FROM {table}
           WHERE (status = 0 OR status = 5)
             AND created_on >= NOW() - INTERVAL '1 year'"#
    ))
    .fetch_optional(pool)
    .await?;

    match row {
        Some((Some(min), Some(max))) => Ok((min, max)),
        _ => Ok((0, 0)),
    }
}

/// Returns a page of IDs of pending records within [min_id, max_id].
pub async fn get_pending_ids(
    pool: &PgPool,
    table: &str,
    min_id: i64,
    max_id: i64,
    page: i64,
    size: i64,
) -> Result<Vec<i64>> {
    let offset = (page - 1) * size;
    let rows: Vec<(i64,)> = sqlx::query_as(&format!(
        r#"SELECT id FROM {table}
           WHERE id >= $1 AND id <= $2
             AND (status = 0 OR status = 5)
             AND created_on >= NOW() - INTERVAL '1 year'
           ORDER BY id
           LIMIT $3 OFFSET $4"#
    ))
    .bind(min_id)
    .bind(max_id)
    .bind(size)
    .bind(offset)
    .fetch_all(pool)
    .await?;
    Ok(rows.into_iter().map(|(id,)| id).collect())
}

// ── TradeRebate ───────────────────────────────────────────────────────────────

/// Fetch a single TradeRebate row from the year-partitioned table.
pub async fn get_trade_rebate(
    pool: &PgPool,
    table: &str,
    id: i64,
) -> Result<Option<TradeRebateRow>> {
    let row = sqlx::query_as::<_, TradeRebateRow>(&format!(
        r#"SELECT id, account_id, trade_service_id, ticket, account_number,
                  currency_id, volume, symbol, refer_path,
                  closed_on, opened_on, status
           FROM {table}
           WHERE id = $1"#
    ))
    .bind(id)
    .fetch_optional(pool)
    .await?;
    Ok(row)
}

/// Update the Status of a TradeRebate row.
pub async fn update_trade_rebate_status(
    pool: &PgPool,
    table: &str,
    id: i64,
    status: i32,
) -> Result<()> {
    sqlx::query(&format!(
        r#"UPDATE {table} SET status = $1, updated_on = NOW() WHERE id = $2"#
    ))
    .bind(status)
    .bind(id)
    .execute(pool)
    .await?;
    Ok(())
}

// ── Account / Rules ───────────────────────────────────────────────────────────

/// Returns (DistributionType, RebateDirectSchemaId) for the given account's RebateClientRule.
/// Returns None if no rule exists.
pub async fn get_distribution_type(
    pool: &PgPool,
    account_id: i64,
) -> Result<Option<(i16, Option<i64>)>> {
    let row: Option<(i16, Option<i64>)> = sqlx::query_as(
        r#"SELECT "DistributionType", "RebateDirectSchemaId"
           FROM trd."_RebateClientRule"
           WHERE "ClientAccountId" = $1
           LIMIT 1"#,
    )
    .bind(account_id)
    .fetch_optional(pool)
    .await?;
    Ok(row)
}

/// Check if an account is active (AccountStatusTypes::Activate = 0).
pub async fn is_account_active(pool: &PgPool, account_id: i64) -> Result<bool> {
    let row: Option<(i16,)> = sqlx::query_as(
        r#"SELECT "Status" FROM trd."_Account" WHERE "Id" = $1 LIMIT 1"#,
    )
    .bind(account_id)
    .fetch_optional(pool)
    .await?;
    Ok(row.map(|(s,)| s == 0).unwrap_or(false))
}

/// Returns sorted agent accounts for Allocation/LevelPercentage modes.
/// Mirrors mono's GetSortedAgentAccountsForAllocation:
///   1. Parse ReferPath UIDs from the client account
///   2. Find all parent accounts that have a RebateAgentRule, ordered by Level
///   3. Skip accounts before the first top-level agent (TakeWhile(!IsTopLevelAgent))
pub async fn get_sorted_agent_accounts(
    pool: &PgPool,
    account_id: i64,
) -> Result<Vec<AgentAccount>> {
    // Get ReferPath of the client account
    let refer_path: Option<String> = sqlx::query_scalar(
        r#"SELECT "ReferPath" FROM trd."_Account" WHERE "Id" = $1 LIMIT 1"#,
    )
    .bind(account_id)
    .fetch_optional(pool)
    .await?;

    let refer_path = match refer_path {
        Some(p) if !p.is_empty() => p,
        _ => return Ok(vec![]),
    };

    // Parse UIDs from ReferPath (dot-separated)
    let uids: Vec<i64> = refer_path
        .split('.')
        .filter_map(|s| s.trim().parse::<i64>().ok())
        .collect();

    if uids.is_empty() {
        return Ok(vec![]);
    }

    // Build parameterized query for IN clause
    let placeholders: Vec<String> = (1..=uids.len()).map(|i| format!("${}", i)).collect();
    let in_clause = placeholders.join(", ");
    let sql = format!(
        r#"SELECT a."Id", a."Uid", a."PartyId", a."CurrencyId", a."FundType",
                  a."Level", a."AgentAccountId",
                  r."Schema"::text AS "RuleSchema",
                  r."LevelSetting"::text AS "RuleLevelSetting"
           FROM trd."_Account" a
           INNER JOIN trd."_RebateAgentRule" r ON r."AgentAccountId" = a."Id"
           WHERE a."Uid" IN ({in_clause})
           ORDER BY a."Level" ASC"#
    );

    let mut query = sqlx::query_as::<_, AgentAccount>(&sql);

    for uid in &uids {
        query = query.bind(uid);
    }

    let parent_accounts = query.fetch_all(pool).await?;

    // Skip accounts before the first top-level agent (TakeWhile(!IsTopLevelAgent))
    let skip_count = parent_accounts
        .iter()
        .take_while(|a| !a.is_top_level_agent())
        .count();

    Ok(parent_accounts.into_iter().skip(skip_count).collect())
}

/// Returns Direct rules for the given source account (excludes AccountNumber=59999999).
pub async fn get_direct_rules(pool: &PgPool, account_id: i64) -> Result<Vec<DirectRule>> {
    let rows = sqlx::query_as::<_, DirectRule>(
        r#"SELECT dr."RebateDirectSchemaId", dr."TargetAccountId"
           FROM trd."_RebateDirectRule" dr
           INNER JOIN trd."_Account" src ON src."Id" = dr."SourceTradeAccountId"
           WHERE dr."SourceTradeAccountId" = $1
             AND src."AccountNumber" != 59999999"#,
    )
    .bind(account_id)
    .fetch_all(pool)
    .await?;
    Ok(rows)
}

/// Returns the DirectSchemaItem for the given schema + trimmed symbol code.
pub async fn get_direct_schema_item(
    pool: &PgPool,
    schema_id: i64,
    symbol_code: &str,
) -> Result<Option<DirectSchemaItem>> {
    let row = sqlx::query_as::<_, DirectSchemaItem>(
        r#"SELECT "Rate", "Pips", "Commission", "SymbolCode"
           FROM trd."_RebateDirectSchemaItem"
           WHERE "RebateDirectSchemaId" = $1
             AND "SymbolCode" = $2
             AND ("Rate" > 0 OR "Pips" > 0 OR "Commission" > 0)
           LIMIT 1"#,
    )
    .bind(schema_id)
    .bind(symbol_code)
    .fetch_optional(pool)
    .await?;
    Ok(row)
}

/// Returns the target account info for Direct mode.
pub async fn get_target_account(pool: &PgPool, account_id: i64) -> Result<Option<TargetAccount>> {
    let row = sqlx::query_as::<_, TargetAccount>(
        r#"SELECT "Id", "PartyId", "CurrencyId", "FundType", "ServiceId"
           FROM trd."_Account"
           WHERE "Id" = $1
           LIMIT 1"#,
    )
    .bind(account_id)
    .fetch_optional(pool)
    .await?;
    Ok(row)
}

/// Returns the symbol's category name (e.g. "FOREX", "GOLD", "CRYPTO").
pub async fn get_symbol_category(pool: &PgPool, symbol_code: &str) -> Result<Option<String>> {
    let row: Option<(String,)> = sqlx::query_as(
        r#"SELECT "Category" FROM trd."_Symbol" WHERE "Code" = $1 LIMIT 1"#,
    )
    .bind(symbol_code)
    .fetch_optional(pool)
    .await?;
    Ok(row.map(|(c,)| c))
}

/// Returns the symbol's CategoryId (used for Allocation schema lookup). INT4 in DB.
pub async fn get_symbol_category_id(pool: &PgPool, symbol_code: &str) -> Result<Option<i32>> {
    let row: Option<(i32,)> = sqlx::query_as(
        r#"SELECT "CategoryId" FROM trd."_Symbol" WHERE "Code" = $1 LIMIT 1"#,
    )
    .bind(symbol_code)
    .fetch_optional(pool)
    .await?;
    Ok(row.map(|(id,)| id))
}

/// Returns existing Rebate records for a TradeRebateId (for PendingResend diff adjustment).
/// Returns (account_id, amount) pairs.
pub async fn get_existing_rebates(
    pool: &PgPool,
    trade_rebate_id: i64,
) -> Result<Vec<(i64, rust_decimal::Decimal)>> {
    let rows: Vec<(i64, rust_decimal::Decimal)> = sqlx::query_as(&format!(
        r#"SELECT account_id, amount FROM {REBATE_TABLE} WHERE trade_rebate_id = $1"#
    ))
    .bind(trade_rebate_id)
    .fetch_all(pool)
    .await?;
    Ok(rows)
}

// ── MT5 price ─────────────────────────────────────────────────────────────────

/// Query MT5 price table for a symbol. Returns (BidLast, Digits).
pub async fn get_mt5_price(mt5_pool: &MySqlPool, symbol_code: &str) -> Result<Option<MtPrice>> {
    let row: Option<(f64, u32)> = sqlx::query_as(
        r#"SELECT BidLast, Digits FROM mt5_prices WHERE Symbol = ? LIMIT 1"#,
    )
    .bind(symbol_code)
    .fetch_optional(mt5_pool)
    .await?;
    Ok(row.map(|(bid, digits)| MtPrice {
        bid,
        digits: digits as i32,
    }))
}

// ── Write ─────────────────────────────────────────────────────────────────────

/// Insert a rebate record in a transaction:
///   1. Generate a Snowflake ID from idgen
///   2. INSERT INTO core.matter_k8s with the Snowflake ID
///   3. INSERT INTO trd.rebate_k8s with Id = matter_id (PG routes to correct year partition via CreatedOn)
///
/// Returns the new matter/rebate Id.
pub async fn insert_rebate(pool: &PgPool, idgen: &IdgenClient, rebate: &NewRebate) -> Result<i64> {
    let matter_id = idgen.generate_id().await?;

    let mut tx = pool.begin().await?;

    // Write to the native-partitioned matter_k8s table; PostgreSQL routes to the
    // correct year partition based on PostedOn automatically.
    sqlx::query(
        r#"INSERT INTO core.matter_k8s (id, type, state_id, posted_on, stated_on)
           VALUES ($1, $2, $3, NOW(), NOW())"#
    )
    .bind(matter_id)
    .bind(MATTER_TYPE_REBATE)
    .bind(STATE_REBATE_ON_HOLD)
    .execute(&mut *tx)
    .await?;

    sqlx::query(
        r#"INSERT INTO trd.rebate_k8s
           (id, party_id, account_id, fund_type, currency_id,
            amount, trade_rebate_id, hold_until_on, information, created_on)
           VALUES ($1, $2, $3, $4, $5, $6, $7, NOW(), $8, NOW())"#
    )
    .bind(matter_id)
    .bind(rebate.party_id)
    .bind(rebate.account_id)
    .bind(rebate.fund_type)
    .bind(rebate.currency_id)
    .bind(rebate.amount)
    .bind(rebate.trade_rebate_id)
    .bind(&rebate.information)
    .execute(&mut *tx)
    .await?;

    tx.commit().await?;
    Ok(matter_id)
}
