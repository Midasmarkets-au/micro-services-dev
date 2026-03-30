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

/// 已平仓 deal（用于 TradeMonitor 轮询）
#[derive(Debug, sqlx::FromRow)]
pub struct Mt5ClosedDeal {
    #[sqlx(rename = "Deal")]
    pub deal: u64,
    #[sqlx(rename = "Login")]
    pub login: u64,
    #[sqlx(rename = "TimeMsc")]
    pub time_msc: chrono::DateTime<chrono::Utc>,
    #[sqlx(rename = "Symbol")]
    pub symbol: String,
    #[sqlx(rename = "Price")]
    pub price: f64,           // close price
    #[sqlx(rename = "VolumeClosed")]
    pub volume_closed: u64,
    #[sqlx(rename = "Volume")]
    pub volume: u64,
    #[sqlx(rename = "Profit")]
    pub profit: f64,
    #[sqlx(rename = "Commission")]
    pub commission: f64,
    #[sqlx(rename = "Storage")]
    pub storage: f64,         // → Swaps
    #[sqlx(rename = "Reason")]
    pub reason: u32,
    #[sqlx(rename = "Action")]
    pub action: u32,
    #[sqlx(rename = "Digits")]
    pub digits: u32,
    #[sqlx(rename = "PositionID")]
    pub position_id: u64,
    #[sqlx(rename = "Timestamp")]
    pub timestamp: i64,       // bigint
}

/// 开仓 deal（仅获取 OpenPrice 和 OpenAt）
#[derive(Debug, sqlx::FromRow)]
pub struct Mt5OpenDeal {
    #[sqlx(rename = "PositionID")]
    pub position_id: u64,
    #[sqlx(rename = "TimeMsc")]
    pub time_msc: chrono::DateTime<chrono::Utc>,
    #[sqlx(rename = "Price")]
    pub price: f64,           // open price
}

/// 查询新的已平仓 deals（TradeMonitor 专用，cursor-based 分页）
pub async fn poll_closed_deals(
    pool: &MySqlPool,
    after_time: chrono::DateTime<chrono::Utc>,
    after_deal: u64,
    limit: u32,
) -> Result<Vec<Mt5ClosedDeal>> {
    let sql = r#"
        SELECT Deal, Login, TimeMsc, Symbol, Price, VolumeClosed, Volume,
               Profit, Commission, Storage, Reason, Action, Digits, PositionID, Timestamp
        FROM mt5_deals_2025
        WHERE VolumeClosed > 0
          AND Action IN (0, 1)
          AND Login < 82200000
          AND (TimeMsc > ? OR (TimeMsc = ? AND Deal > ?))
        ORDER BY TimeMsc ASC, Deal ASC
        LIMIT ?
    "#;
    let rows = sqlx::query_as::<_, Mt5ClosedDeal>(sql)
        .bind(after_time)
        .bind(after_time)
        .bind(after_deal)
        .bind(limit)
        .fetch_all(pool)
        .await?;
    Ok(rows)
}

/// 批量查询对应的开仓 deal（获取 OpenPrice、OpenAt）
pub async fn get_open_deals_by_positions(
    pool: &MySqlPool,
    position_ids: &[u64],
) -> Result<Vec<Mt5OpenDeal>> {
    if position_ids.is_empty() {
        return Ok(vec![]);
    }
    let placeholders = position_ids.iter().map(|_| "?").collect::<Vec<_>>().join(", ");
    let sql = format!(
        r#"SELECT PositionID, TimeMsc, Price
           FROM mt5_deals_2025
           WHERE PositionID IN ({})
             AND Entry = 0"#,
        placeholders
    );
    let mut q = sqlx::query_as::<_, Mt5OpenDeal>(&sql);
    for id in position_ids {
        q = q.bind(id);
    }
    let rows = q.fetch_all(pool).await?;
    Ok(rows)
}
