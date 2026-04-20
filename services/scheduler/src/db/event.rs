use anyhow::Result;
use chrono::{DateTime, Utc};
use sqlx::PgPool;

// ── Source type constants (mirror EventShopPointTransactionSourceTypes) ───────
#[allow(dead_code)]
pub const SOURCE_TYPE_OPEN_ACCOUNT: i16 = 1;
pub const SOURCE_TYPE_TRADE: i16 = 2;
#[allow(dead_code)]
pub const SOURCE_TYPE_DEPOSIT: i16 = 3;
// ── Status constant (mirrors EventShopPointTransactionStatusTypes.Success) ────
pub const STATUS_SUCCESS: i16 = 1;
// ── Hardcoded EventId=1 (system has only one active event) ────────────────────
pub const EVENT_ID: i64 = 1;

/// Minimal account info needed for trade point processing.
#[derive(Debug, sqlx::FromRow)]
#[allow(dead_code)]
pub struct AccountForTrade {
    #[sqlx(rename = "AgentAccountId")]
    pub agent_account_id: Option<i64>,
    #[sqlx(rename = "CurrencyId")]
    pub currency_id: i32,
    #[sqlx(rename = "Role")]
    pub role: i16,
}

/// Minimal TradeRebate fields needed for point calculation.
#[derive(Debug, sqlx::FromRow)]
#[allow(dead_code)]
pub struct TradeRebateForEvent {
    pub id: i64,
    pub account_id: Option<i64>,
    pub volume: i32,
    pub currency_id: i32,
    pub ticket: i64,
    pub account_number: i64,
    pub trade_service_id: i32,
    pub opened_on: DateTime<Utc>,
    pub closed_on: DateTime<Utc>,
    pub reason: i32,
}

/// Read a TradeRebate from trd.trade_rebate_k8s by id.
pub async fn get_trade_rebate(pool: &PgPool, id: i64) -> Result<Option<TradeRebateForEvent>> {
    let row = sqlx::query_as::<_, TradeRebateForEvent>(
        r#"SELECT id, account_id, volume, currency_id, ticket, account_number,
                  trade_service_id, opened_on, closed_on, reason
           FROM trd.trade_rebate_k8s
           WHERE id = $1
           LIMIT 1"#,
    )
    .bind(id)
    .fetch_optional(pool)
    .await?;
    Ok(row)
}

/// Get the EventPartyId for a given account within event_id=1.
/// Mirrors: tenantDbContext.EventParties
///     .Where(x => x.EventId == eventId && x.Party.Accounts.Any(y => y.Id == accountId))
///     .Select(x => x.Id)
pub async fn get_event_party_id(pool: &PgPool, account_id: i64) -> Result<Option<i64>> {
    let row: Option<(i64,)> = sqlx::query_as(
        r#"SELECT ep."Id"
           FROM event."_EventParty" ep
           INNER JOIN trd."_Account" a ON a."PartyId" = ep."PartyId"
           WHERE ep."EventId" = $1
             AND a."Id" = $2
           LIMIT 1"#,
    )
    .bind(EVENT_ID)
    .bind(account_id)
    .fetch_optional(pool)
    .await?;
    Ok(row.map(|(id,)| id))
}

/// Get minimal account fields needed for trade processing.
pub async fn get_account_for_trade(pool: &PgPool, account_id: i64) -> Result<Option<AccountForTrade>> {
    let row = sqlx::query_as::<_, AccountForTrade>(
        r#"SELECT "AgentAccountId", "CurrencyId", "Role"
           FROM trd."_Account"
           WHERE "Id" = $1
           LIMIT 1"#,
    )
    .bind(account_id)
    .fetch_optional(pool)
    .await?;
    Ok(row)
}

/// Check if a point transaction already exists for this trade (dedup by source_content).
/// Mirrors: CheckIfTransactionExistAsync
pub async fn check_point_transaction_exists(
    pool: &PgPool,
    event_party_id: i64,
    account_id: i64,
    source_content: &str,
) -> Result<bool> {
    let row: (bool,) = sqlx::query_as(
        r#"SELECT EXISTS(
            SELECT 1 FROM event."_EventShopPointTransaction"
            WHERE "EventPartyId" = $1
              AND "AccountId" = $2
              AND "SourceType" = $3
              AND "SourceContent" = $4
        )"#,
    )
    .bind(event_party_id)
    .bind(account_id)
    .bind(SOURCE_TYPE_TRADE)
    .bind(source_content)
    .fetch_one(pool)
    .await?;
    Ok(row.0)
}

/// Get IDs of EventShopClientPoint rows where this account is the child.
/// Mirrors: GetValidClientPointIdsAsync (parentRole=null → all roles)
/// Excludes Sales-role parents that have OpenAccount=0.
pub async fn get_valid_client_point_ids(pool: &PgPool, account_id: i64) -> Result<Vec<i64>> {
    let rows: Vec<(i64,)> = sqlx::query_as(
        r#"SELECT "Id"
           FROM event."_EventShopClientPoint"
           WHERE "ChildAccountId" = $1
             AND ("ParentAccountRole" != 300 OR "OpenAccount" > 0)"#,
        // 300 = AccountRoleTypes.Sales
    )
    .bind(account_id)
    .fetch_all(pool)
    .await?;
    Ok(rows.into_iter().map(|(id,)| id).collect())
}

/// Update and insert atomically:
///   UPDATE event._EventShopPoint  SET Point += point * 10_000, TotalPoint += point * 10_000
///   INSERT event._EventShopPointTransaction (Point = point * 10_000)
///
/// Mirrors ChangePointAsync exactly: C# passes `point` and the SQL applies
/// `point.ToScaledFromCents()` (×10_000) before storing.
///
/// Callers pass the pre-scale value:
///   client: (Volume * 100 * 10_000) * pointMultiplier
///   agent:  (Volume * 100 * 10_000) * pointMultiplier * 0.3
/// This function applies the final ×10_000, matching:
///   stored = point * 10_000 * 10_000 = Volume * 10_000_000_000 * multiplier
pub async fn change_point(
    pool: &PgPool,
    event_party_id: i64,
    point: i64,
    account_id: i64,
    trade_rebate_id: i64,
    source_content: &str,
) -> Result<()> {
    let point_stored = point * 10_000;

    sqlx::query(
        r#"UPDATE event."_EventShopPoint"
           SET "Point" = "Point" + $1,
               "TotalPoint" = "TotalPoint" + $1,
               "UpdatedOn" = NOW()
           WHERE "EventPartyId" = $2"#,
    )
    .bind(point_stored)
    .bind(event_party_id)
    .execute(pool)
    .await?;

    sqlx::query(
        r#"INSERT INTO event."_EventShopPointTransaction"
           ("EventPartyId", "Point", "SourceType", "SourceId", "SourceContent", "Status", "AccountId", "CreatedOn", "UpdatedOn")
           VALUES ($1, $2, $3, $4, $5, $6, $7, NOW(), NOW())"#,
    )
    .bind(event_party_id)
    .bind(point_stored)
    .bind(SOURCE_TYPE_TRADE)
    .bind(trade_rebate_id)
    .bind(source_content)
    .bind(STATUS_SUCCESS)
    .bind(account_id)
    .execute(pool)
    .await?;

    Ok(())
}

/// UPDATE event._EventShopClientPoint SET Volume += adjusted_volume WHERE Id = id
/// adjusted_volume is already USC-adjusted by caller.
/// Mirrors: AddTradePointByIdAsync
pub async fn add_trade_volume_to_client_point(
    pool: &PgPool,
    client_point_id: i64,
    adjusted_volume: i64,
) -> Result<()> {
    sqlx::query(
        r#"UPDATE event."_EventShopClientPoint"
           SET "Volume" = "Volume" + $1,
               "UpdatedOn" = NOW()
           WHERE "Id" = $2"#,
    )
    .bind(adjusted_volume)
    .bind(client_point_id)
    .execute(pool)
    .await?;
    Ok(())
}

/// Insert an EventShopClientPoint row if it doesn't already exist.
/// Mirrors: TryEnsureParentHasClientPoint
/// parent_role: 300=Sales, 400=Client, 200=Agent (AccountRoleTypes)
pub async fn ensure_client_point_exists(
    pool: &PgPool,
    child_account_id: i64,
    parent_account_id: i64,
    parent_role: i16,
) -> Result<()> {
    sqlx::query(
        r#"INSERT INTO event."_EventShopClientPoint"
           ("ChildAccountId", "ParentAccountId", "ParentAccountRole",
            "OpenAccount", "Volume", "DepositAmount", "CreatedOn", "UpdatedOn")
           VALUES ($1, $2, $3, 0, 0, 0, NOW(), NOW())
           ON CONFLICT ("ChildAccountId", "ParentAccountId") DO NOTHING"#,
    )
    .bind(child_account_id)
    .bind(parent_account_id)
    .bind(parent_role)
    .execute(pool)
    .await?;
    Ok(())
}

// ── OpenAccount helpers ────────────────────────────────────────────────────────

/// Minimal account info needed for OpenAccount processing.
#[derive(Debug, sqlx::FromRow)]
#[allow(dead_code)]
pub struct AccountForOpenAccount {
    pub id: i64,
    pub role: i16,
    #[sqlx(rename = "SalesAccountId")]
    pub sales_account_id: Option<i64>,
    #[sqlx(rename = "AgentAccountId")]
    pub agent_account_id: Option<i64>,
}

/// Load account fields needed for ProcessOpenAccountSourceAsync.
pub async fn get_account_for_open_account(pool: &PgPool, account_id: i64) -> Result<Option<AccountForOpenAccount>> {
    let row = sqlx::query_as::<_, AccountForOpenAccount>(
        r#"SELECT "Id" as id, "Role" as role, "SalesAccountId", "AgentAccountId"
           FROM trd."_Account"
           WHERE "Id" = $1
           LIMIT 1"#,
    )
    .bind(account_id)
    .fetch_optional(pool)
    .await?;
    Ok(row)
}

/// Insert an EventShopClientPoint row with a specific OpenAccount value.
/// Mirrors: EventShopClientPoint.Build(accountId, parentId, role, openAccountValue)
/// parent_role: 200=Agent, 300=Sales
/// open_account: initial OpenAccount value (5 for Agent-under-Sales, 6 for Client-direct-to-Sales, 1 for Client-to-Agent)
pub async fn insert_client_point_with_open_account(
    pool: &PgPool,
    child_account_id: i64,
    parent_account_id: i64,
    parent_role: i16,
    open_account: i32,
) -> Result<()> {
    sqlx::query(
        r#"INSERT INTO event."_EventShopClientPoint"
           ("ChildAccountId", "ParentAccountId", "ParentAccountRole",
            "OpenAccount", "Volume", "DepositAmount", "CreatedOn", "UpdatedOn")
           VALUES ($1, $2, $3, $4, 0, 0, NOW(), NOW())
           ON CONFLICT ("ChildAccountId", "ParentAccountId") DO NOTHING"#,
    )
    .bind(child_account_id)
    .bind(parent_account_id)
    .bind(parent_role)
    .bind(open_account)
    .execute(pool)
    .await?;
    Ok(())
}

/// Check if a ClientPoint row already links child→parent.
/// Used to detect if an Agent is new under a Sales parent.
/// Mirrors: IsNewAccountForSalesAsync
pub async fn is_new_account_for_parent(pool: &PgPool, child_account_id: i64, parent_account_id: i64) -> Result<bool> {
    let row: (bool,) = sqlx::query_as(
        r#"SELECT NOT EXISTS(
            SELECT 1 FROM event."_EventShopClientPoint"
            WHERE "ChildAccountId" = $1
              AND "ParentAccountId" = $2
        )"#,
    )
    .bind(child_account_id)
    .bind(parent_account_id)
    .fetch_one(pool)
    .await?;
    Ok(row.0)
}

// ── Deposit helpers ────────────────────────────────────────────────────────────

/// Minimal deposit info for ProcessDepositSourceAsync.
#[derive(Debug, sqlx::FromRow)]
#[allow(dead_code)]
pub struct DepositForEvent {
    pub id: i64,
    #[sqlx(rename = "TargetAccountId")]
    pub target_account_id: Option<i64>,
    #[sqlx(rename = "Amount")]
    pub amount: i64,
}

/// Load deposit fields needed for ProcessDepositSourceAsync.
pub async fn get_deposit_for_event(pool: &PgPool, deposit_id: i64) -> Result<Option<DepositForEvent>> {
    let row = sqlx::query_as::<_, DepositForEvent>(
        r#"SELECT d."Id" as id, d."TargetAccountId", d."Amount"
           FROM pmt."_Deposit" d
           WHERE d."Id" = $1
           LIMIT 1"#,
    )
    .bind(deposit_id)
    .fetch_optional(pool)
    .await?;
    Ok(row)
}

/// Get IDs of EventShopClientPoint rows where this account is the child,
/// filtered to Agent-role parents only (for deposit accumulation).
/// Mirrors: GetValidClientPointIdsAsync(childAccountId, AccountRoleTypes.Agent)
pub async fn get_valid_client_point_ids_for_deposit(pool: &PgPool, account_id: i64) -> Result<Vec<i64>> {
    let rows: Vec<(i64,)> = sqlx::query_as(
        r#"SELECT "Id"
           FROM event."_EventShopClientPoint"
           WHERE "ChildAccountId" = $1
             AND "ParentAccountRole" = 200"#,
        // 200 = AccountRoleTypes.Agent
    )
    .bind(account_id)
    .fetch_all(pool)
    .await?;
    Ok(rows.into_iter().map(|(id,)| id).collect())
}

/// UPDATE event._EventShopClientPoint SET DepositAmount += amount WHERE Id = id.
/// Mirrors: AddDepositPointByIdAsync
pub async fn add_deposit_amount_to_client_point(pool: &PgPool, client_point_id: i64, amount: i64) -> Result<()> {
    sqlx::query(
        r#"UPDATE event."_EventShopClientPoint"
           SET "DepositAmount" = "DepositAmount" + $1,
               "UpdatedOn" = NOW()
           WHERE "Id" = $2"#,
    )
    .bind(amount)
    .bind(client_point_id)
    .execute(pool)
    .await?;
    Ok(())
}
