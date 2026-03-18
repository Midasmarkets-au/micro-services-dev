#![allow(dead_code)]

use anyhow::Result;
use chrono::{DateTime, Utc};
use serde::{Deserialize, Serialize};
use sqlx::MySqlPool;

/// Mirrors MT4_DAILY table
#[derive(Debug, Clone, sqlx::FromRow, Serialize, Deserialize)]
pub struct Mt4Daily {
    #[sqlx(rename = "LOGIN")]
    pub login: i64,
    #[sqlx(rename = "TIME")]
    pub time: Option<DateTime<Utc>>,
    #[sqlx(rename = "BALANCE")]
    pub balance: Option<f64>,
    #[sqlx(rename = "EQUITY")]
    pub equity: Option<f64>,
    #[sqlx(rename = "MARGIN")]
    pub margin: Option<f64>,
    #[sqlx(rename = "MARGIN_FREE")]
    pub margin_free: Option<f64>,
    #[sqlx(rename = "MARGIN_LEVEL")]
    pub margin_level: Option<f64>,
    #[sqlx(rename = "PROFIT")]
    pub profit: Option<f64>,
}

/// Mirrors MT4_TRADES table (closed trades)
#[derive(Debug, Clone, sqlx::FromRow, Serialize, Deserialize)]
pub struct Mt4Trade {
    #[sqlx(rename = "TICKET")]
    pub ticket: i64,
    #[sqlx(rename = "LOGIN")]
    pub login: i64,
    #[sqlx(rename = "SYMBOL")]
    pub symbol: Option<String>,
    #[sqlx(rename = "DIGITS")]
    pub digits: Option<i32>,
    #[sqlx(rename = "CMD")]
    pub cmd: Option<i32>,
    #[sqlx(rename = "VOLUME")]
    pub volume: Option<f64>,
    #[sqlx(rename = "OPEN_TIME")]
    pub open_time: Option<DateTime<Utc>>,
    #[sqlx(rename = "OPEN_PRICE")]
    pub open_price: Option<f64>,
    #[sqlx(rename = "CLOSE_TIME")]
    pub close_time: Option<DateTime<Utc>>,
    #[sqlx(rename = "CLOSE_PRICE")]
    pub close_price: Option<f64>,
    #[sqlx(rename = "PROFIT")]
    pub profit: Option<f64>,
    #[sqlx(rename = "COMMISSION")]
    pub commission: Option<f64>,
    #[sqlx(rename = "SWAPS")]
    pub swaps: Option<f64>,
    #[sqlx(rename = "COMMENT")]
    pub comment: Option<String>,
}

pub async fn get_daily_equity(
    pool: &MySqlPool,
    logins: &[i64],
    from: DateTime<Utc>,
    to: DateTime<Utc>,
) -> Result<Vec<Mt4Daily>> {
    if logins.is_empty() {
        return Ok(vec![]);
    }
    // Build IN clause manually for MySQL
    let placeholders: String = logins.iter().enumerate()
        .map(|(i, _)| format!("${}", i + 3))
        .collect::<Vec<_>>()
        .join(", ");
    let sql = format!(
        r#"SELECT LOGIN, TIME, BALANCE, EQUITY, MARGIN, MARGIN_FREE, MARGIN_LEVEL, PROFIT
           FROM MT4_DAILY
           WHERE TIME >= ? AND TIME < ?
           AND LOGIN IN ({})"#,
        placeholders
    );
    let mut q = sqlx::query_as::<_, Mt4Daily>(&sql)
        .bind(from)
        .bind(to);
    for login in logins {
        q = q.bind(login);
    }
    let rows = q.fetch_all(pool).await?;
    Ok(rows)
}

pub async fn get_closed_trades(
    pool: &MySqlPool,
    logins: &[i64],
    from: DateTime<Utc>,
    to: DateTime<Utc>,
) -> Result<Vec<Mt4Trade>> {
    if logins.is_empty() {
        return Ok(vec![]);
    }
    let in_clause = logins.iter().map(|_| "?").collect::<Vec<_>>().join(", ");
    let sql = format!(
        r#"SELECT TICKET, LOGIN, SYMBOL, DIGITS, CMD, VOLUME, OPEN_TIME, OPEN_PRICE,
                  CLOSE_TIME, CLOSE_PRICE, PROFIT, COMMISSION, SWAPS, COMMENT
           FROM MT4_TRADES
           WHERE CLOSE_TIME >= ? AND CLOSE_TIME < ?
           AND CMD IN (0, 1)
           AND LOGIN IN ({})"#,
        in_clause
    );
    let mut q = sqlx::query_as::<_, Mt4Trade>(&sql)
        .bind(from)
        .bind(to);
    for login in logins {
        q = q.bind(login);
    }
    let rows = q.fetch_all(pool).await?;
    Ok(rows)
}
