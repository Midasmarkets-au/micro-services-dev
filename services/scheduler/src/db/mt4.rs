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

/// 已平仓 trade（TradeMonitor 轮询专用）。MT4 把开仓/平仓数据存在同一行里
/// （不像 MT5 拆成两条 deal），所以不需要额外的 position join。
///
/// 除了 TICKET/LOGIN/CMD/CLOSE_TIME（分别是主键、必填账号、WHERE 里已约束为
/// 0/1、WHERE 里已约束为非哨兵值，实测都不为 NULL）以外的字段全部用 Option
/// 接——真实数据里 REASON 等文档标称 NOT NULL 的列实际出现过 NULL。
#[derive(Debug, sqlx::FromRow)]
pub struct Mt4ClosedTrade {
    #[sqlx(rename = "TICKET")]
    pub ticket: i64,
    #[sqlx(rename = "LOGIN")]
    pub login: i64,
    #[sqlx(rename = "SYMBOL")]
    pub symbol: Option<String>,
    #[sqlx(rename = "DIGITS")]
    pub digits: Option<i32>,
    #[sqlx(rename = "CMD")]
    pub cmd: i32,
    #[sqlx(rename = "VOLUME")]
    pub volume: Option<i32>,
    #[sqlx(rename = "OPEN_TIME")]
    pub open_time: Option<DateTime<Utc>>,
    #[sqlx(rename = "OPEN_PRICE")]
    pub open_price: Option<f64>,
    #[sqlx(rename = "CLOSE_TIME")]
    pub close_time: DateTime<Utc>,
    #[sqlx(rename = "CLOSE_PRICE")]
    pub close_price: Option<f64>,
    #[sqlx(rename = "PROFIT")]
    pub profit: Option<f64>,
    #[sqlx(rename = "COMMISSION")]
    pub commission: Option<f64>,
    #[sqlx(rename = "SWAPS")]
    pub swaps: Option<f64>,
    #[sqlx(rename = "REASON")]
    pub reason: Option<i32>,
    // MT4's TIMESTAMP is documented as int(11) but is effectively an ever-growing
    // internal modification counter on busy servers, not a plain unix-seconds
    // value — real values have been observed to exceed i32::MAX. Widen to i64.
    #[sqlx(rename = "TIMESTAMP")]
    pub timestamp: Option<i64>,
}

/// 查询新的已平仓 trades（cursor-based 分页，供 TradeMonitor 轮询）。
/// CLOSE_TIME > epoch 用来排除还未平仓的单子 —— MT4_TRADES 把开仓和平仓数据
/// 放在同一行，未平仓的单子 CLOSE_TIME 停在 1970-01-01 哨兵值，不是这一行不存在。
///
/// `after_time` is UTC (matches the Redis cursor); converted to the server's local time
/// before binding, and OPEN_TIME/CLOSE_TIME on the returned rows are converted back to UTC,
/// so every caller of this function only ever deals in UTC.
pub async fn poll_closed_trades(
    pool: &MySqlPool,
    after_time: DateTime<Utc>,
    after_ticket: i64,
    limit: u32,
) -> Result<Vec<Mt4ClosedTrade>> {
    let offset = super::mysql_utc_offset(pool).await?;
    let local_after_time = after_time - offset;

    let sql = r#"
        SELECT TICKET, LOGIN, SYMBOL, DIGITS, CMD, VOLUME, OPEN_TIME, OPEN_PRICE,
               CLOSE_TIME, CLOSE_PRICE, PROFIT, COMMISSION, SWAPS, REASON, TIMESTAMP
        FROM MT4_TRADES
        WHERE CMD IN (0, 1)
          AND CLOSE_TIME > '1970-01-01 00:00:00'
          AND (CLOSE_TIME > ? OR (CLOSE_TIME = ? AND TICKET > ?))
        ORDER BY CLOSE_TIME ASC, TICKET ASC
        LIMIT ?
    "#;
    let mut rows = sqlx::query_as::<_, Mt4ClosedTrade>(sql)
        .bind(local_after_time)
        .bind(local_after_time)
        .bind(after_ticket)
        .bind(limit)
        .fetch_all(pool)
        .await?;

    for row in &mut rows {
        row.open_time = row.open_time.map(|t| t + offset);
        row.close_time += offset;
    }

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
