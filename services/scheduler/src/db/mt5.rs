#![allow(dead_code)]

use anyhow::Result;
use chrono::{DateTime, Utc};
use serde::{Deserialize, Serialize};
use sqlx::MySqlPool;

/// Mirrors mt5_daily table
#[derive(Debug, Clone, sqlx::FromRow, Serialize, Deserialize)]
pub struct Mt5Daily {
    pub login: i64,
    pub time: Option<DateTime<Utc>>,
    pub balance: Option<f64>,
    pub equity: Option<f64>,
    pub margin: Option<f64>,
    pub margin_free: Option<f64>,
    pub profit: Option<f64>,
    pub floating: Option<f64>,
}

/// Mirrors mt5_deals_2025 (or current year partition)
#[derive(Debug, Clone, sqlx::FromRow, Serialize, Deserialize)]
pub struct Mt5Deal {
    pub deal: i64,
    pub login: i64,
    pub time: Option<DateTime<Utc>>,
    pub symbol: Option<String>,
    pub volume: Option<f64>,
    pub price: Option<f64>,
    pub profit: Option<f64>,
    pub commission: Option<f64>,
    pub swap: Option<f64>,
    pub action: Option<i32>,
    pub entry: Option<i32>,
    pub comment: Option<String>,
}

/// Mirrors mt5_positions table
#[derive(Debug, Clone, sqlx::FromRow, Serialize, Deserialize)]
pub struct Mt5Position {
    pub position: i64,
    pub login: i64,
    pub symbol: Option<String>,
    pub volume: Option<f64>,
    pub price_open: Option<f64>,
    pub profit: Option<f64>,
    pub time_create: Option<DateTime<Utc>>,
}

pub async fn get_daily_equity(
    pool: &MySqlPool,
    logins: &[i64],
    from: DateTime<Utc>,
    to: DateTime<Utc>,
) -> Result<Vec<Mt5Daily>> {
    if logins.is_empty() {
        return Ok(vec![]);
    }
    let in_clause = logins.iter().map(|_| "?").collect::<Vec<_>>().join(", ");
    let sql = format!(
        r#"SELECT login, time, balance, equity, margin, margin_free, profit, floating
           FROM mt5_daily
           WHERE time >= ? AND time < ?
           AND login IN ({})"#,
        in_clause
    );
    let mut q = sqlx::query_as::<_, Mt5Daily>(&sql)
        .bind(from)
        .bind(to);
    for login in logins {
        q = q.bind(login);
    }
    let rows = q.fetch_all(pool).await?;
    Ok(rows)
}

pub async fn get_deals(
    pool: &MySqlPool,
    logins: &[i64],
    from: DateTime<Utc>,
    to: DateTime<Utc>,
) -> Result<Vec<Mt5Deal>> {
    if logins.is_empty() {
        return Ok(vec![]);
    }
    let in_clause = logins.iter().map(|_| "?").collect::<Vec<_>>().join(", ");
    // Use the 2025 partition table (matches C# Mt5Deals2025s)
    let sql = format!(
        r#"SELECT deal, login, time, symbol, volume, price, profit, commission, swap, action, entry, comment
           FROM mt5_deals_2025
           WHERE time >= ? AND time < ?
           AND login IN ({})
           AND entry = 1"#, // entry=1 = deal out (closed)
        in_clause
    );
    let mut q = sqlx::query_as::<_, Mt5Deal>(&sql)
        .bind(from)
        .bind(to);
    for login in logins {
        q = q.bind(login);
    }
    let rows = q.fetch_all(pool).await?;
    Ok(rows)
}

pub async fn get_open_positions(pool: &MySqlPool, logins: &[i64]) -> Result<Vec<Mt5Position>> {
    if logins.is_empty() {
        return Ok(vec![]);
    }
    let in_clause = logins.iter().map(|_| "?").collect::<Vec<_>>().join(", ");
    let sql = format!(
        r#"SELECT position, login, symbol, volume, price_open, profit, time_create
           FROM mt5_positions
           WHERE login IN ({})"#,
        in_clause
    );
    let mut q = sqlx::query_as::<_, Mt5Position>(&sql);
    for login in logins {
        q = q.bind(login);
    }
    let rows = q.fetch_all(pool).await?;
    Ok(rows)
}
