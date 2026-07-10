pub mod auth;
pub mod event;
pub mod mt4;
pub mod mt5;
pub mod partition;
pub mod rebate;
pub mod rebate_calc;
pub mod sales_rebate;
pub mod tenant;
pub mod trade_rebate;

use anyhow::Result;
use sqlx::{MySqlPool, PgPool};

/// Create a PostgreSQL pool from a connection URL.
pub async fn pg_pool(url: &str) -> Result<PgPool> {
    let pool = PgPool::connect(url).await?;
    Ok(pool)
}

/// Create a per-tenant PostgreSQL pool.
pub async fn tenant_pg_pool(url: &str) -> Result<PgPool> {
    let pool = PgPool::connect(url).await?;
    Ok(pool)
}

/// Create a MySQL pool from a connection URL.
pub async fn mysql_pool(url: &str) -> Result<MySqlPool> {
    let pool = MySqlPool::connect(url).await?;
    Ok(pool)
}

/// MT4/MT5 DATETIME columns (OPEN_TIME/CLOSE_TIME/TimeMsc) are naive, stored in whatever
/// timezone the MySQL server session resolves to (observed on the shared MT4/MT5 host:
/// `time_zone=SYSTEM`, e.g. UTC-4) — NOT UTC. sqlx decodes them straight into
/// `DateTime<Utc>` with no conversion, so every value read (or bound) against these columns
/// is off by the server's UTC offset unless corrected. Queried per call (cheap, single-row)
/// rather than cached, so it stays correct across DST transitions without any cache
/// invalidation to worry about.
///
/// Returns UTC - local, i.e. the amount to ADD to a local value to get UTC (and to SUBTRACT
/// from a UTC value to get local).
pub async fn mysql_utc_offset(pool: &MySqlPool) -> Result<chrono::Duration> {
    let (seconds,): (i64,) =
        sqlx::query_as("SELECT TIMESTAMPDIFF(SECOND, NOW(), UTC_TIMESTAMP())")
            .fetch_one(pool)
            .await?;
    Ok(chrono::Duration::seconds(seconds))
}
