pub mod auth;
pub mod mt4;
pub mod mt5;
pub mod rebate;
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
