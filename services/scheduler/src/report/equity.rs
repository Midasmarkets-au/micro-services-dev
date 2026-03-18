#![allow(dead_code)]
/// MT4 Equity report — mirrors ReportService.Equity.cs

use anyhow::Result;
use chrono::{DateTime, Utc};
use serde::{Deserialize, Serialize};

use crate::AppContext;

#[derive(Debug, Serialize, Deserialize)]
pub struct Mt4EquityDailyRecord {
    pub login: i64,
    pub date: Option<DateTime<Utc>>,
    pub balance: Option<f64>,
    pub equity: Option<f64>,
    pub margin: Option<f64>,
    pub margin_free: Option<f64>,
    pub profit: Option<f64>,
}

#[derive(Debug, Serialize, Deserialize)]
pub struct Mt4EquityMonthlyRecord {
    pub login: i64,
    pub month: Option<String>,
    pub trade_count: i64,
    pub total_profit: Option<f64>,
    pub total_commission: Option<f64>,
    pub total_swap: Option<f64>,
    pub total_volume: Option<f64>,
}

pub async fn generate_mt4_equity_daily(
    _ctx: &AppContext,
    mt4_conn_str: &str,
    logins: &[i64],
    from: DateTime<Utc>,
    to: DateTime<Utc>,
) -> Result<Vec<Mt4EquityDailyRecord>> {
    let mt4_pool = crate::db::mysql_pool(mt4_conn_str).await?;
    let dailies = crate::db::mt4::get_daily_equity(&mt4_pool, logins, from, to).await?;

    let records = dailies
        .into_iter()
        .map(|d| Mt4EquityDailyRecord {
            login: d.login,
            date: d.time,
            balance: d.balance,
            equity: d.equity,
            margin: d.margin,
            margin_free: d.margin_free,
            profit: d.profit,
        })
        .collect();

    Ok(records)
}

pub async fn generate_mt4_equity_monthly(
    _ctx: &AppContext,
    mt4_conn_str: &str,
    logins: &[i64],
    from: DateTime<Utc>,
    to: DateTime<Utc>,
) -> Result<Vec<Mt4EquityMonthlyRecord>> {
    let mt4_pool = crate::db::mysql_pool(mt4_conn_str).await?;
    let trades = crate::db::mt4::get_closed_trades(&mt4_pool, logins, from, to).await?;

    // Group by login + month
    let mut map: std::collections::HashMap<(i64, String), Mt4EquityMonthlyRecord> =
        std::collections::HashMap::new();

    for trade in trades {
        let month = trade
            .close_time
            .map(|t| t.format("%Y-%m").to_string())
            .unwrap_or_default();
        let entry = map.entry((trade.login, month.clone())).or_insert(Mt4EquityMonthlyRecord {
            login: trade.login,
            month: Some(month),
            trade_count: 0,
            total_profit: Some(0.0),
            total_commission: Some(0.0),
            total_swap: Some(0.0),
            total_volume: Some(0.0),
        });
        entry.trade_count += 1;
        *entry.total_profit.get_or_insert(0.0) += trade.profit.unwrap_or(0.0);
        *entry.total_commission.get_or_insert(0.0) += trade.commission.unwrap_or(0.0);
        *entry.total_swap.get_or_insert(0.0) += trade.swaps.unwrap_or(0.0);
        *entry.total_volume.get_or_insert(0.0) += trade.volume.unwrap_or(0.0);
    }

    let mut records: Vec<Mt4EquityMonthlyRecord> = map.into_values().collect();
    records.sort_by(|a, b| a.login.cmp(&b.login).then(a.month.cmp(&b.month)));
    Ok(records)
}
