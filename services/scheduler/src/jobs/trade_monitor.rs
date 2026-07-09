use std::collections::HashMap;
use anyhow::Result;
use chrono::{DateTime, Utc};
use tracing::{error, info, warn};

use rust_decimal::Decimal;
use rust_decimal::prelude::FromPrimitive;

use crate::db::{mt4, mt5, tenant};
use crate::models::meta_trade::MetaTrade;
use crate::nats::client::SUBJECT_TRADE;
use crate::AppContext;

const DEDUP_HASH_KEY: &str = "trade:queue:dedup";
const LAST_TIME_KEY_PREFIX: &str = "trade:monitor:last_time:";
const LAST_DEAL_KEY_PREFIX: &str = "trade:monitor:last_deal:";

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum Platform {
    Mt4,
    Mt5,
}

/// Entry point: discovers all MT4/MT5 services across all tenants and spawns a polling loop per service.
pub async fn run(ctx: AppContext) -> Result<()> {
    let tenant_ids = tenant::get_all_tenant_ids(&ctx.central_pool).await?;

    // Collect unique (service_id, platform, tenant_pool) triples across all tenants
    let mut service_tenant: Vec<(i32, Platform, sqlx::PgPool)> = vec![];
    let mut seen_service_ids = std::collections::HashSet::new();

    for tenant_id in tenant_ids {
        let tenant_pool = match ctx.tenant_pool(tenant_id).await {
            Ok(p) => p,
            Err(e) => {
                warn!("TradeMonitor: failed to get tenant {} pool: {:#}", tenant_id, e);
                continue;
            }
        };

        for (platform, ids_result) in [
            (Platform::Mt5, tenant::get_mt5_service_ids_from_central(&tenant_pool).await),
            (Platform::Mt4, tenant::get_mt4_service_ids_from_central(&tenant_pool).await),
        ] {
            let ids = match ids_result {
                Ok(ids) => ids,
                Err(e) => {
                    warn!("TradeMonitor: failed to get {:?} services for tenant {}: {:#}", platform, tenant_id, e);
                    continue;
                }
            };
            for service_id in ids {
                if seen_service_ids.insert(service_id) {
                    service_tenant.push((service_id, platform, tenant_pool.clone()));
                }
            }
        }
    }

    if service_tenant.is_empty() {
        warn!("TradeMonitor: no MT4/MT5 trade services found in any tenant trd._TradeService. Trade monitoring will exit.");
        return Ok(());
    }

    info!("TradeMonitor: found {} service(s): {:?}", service_tenant.len(), service_tenant.iter().map(|(id, p, _)| (*id, *p)).collect::<Vec<_>>());

    let mut handles = vec![];
    for (service_id, platform, tenant_pool) in service_tenant {
        let ctx2 = ctx.clone();
        handles.push(tokio::spawn(async move {
            poll_service_loop(ctx2, service_id, platform, tenant_pool).await;
        }));
    }
    for h in handles {
        if let Err(e) = h.await {
            error!("TradeMonitor: poll task panicked: {:?}", e);
        }
    }
    Ok(())
}

/// Runs forever, polling one MT4/MT5 service every 1 second.
async fn poll_service_loop(ctx: AppContext, service_id: i32, platform: Platform, tenant_pool: sqlx::PgPool) {
    info!("TradeMonitor: starting poll loop for service {} ({:?})", service_id, platform);
    let mut round: u64 = 0;
    loop {
        round += 1;
        // Every 12 hours clear dedup hash to prevent unbounded growth
        if round.is_multiple_of(43200) {
            if let Err(e) = ctx.cache.del(DEDUP_HASH_KEY).await {
                warn!("TradeMonitor: failed to clear dedup cache: {:#}", e);
            }
        }

        if let Err(e) = poll_once(&ctx, service_id, platform, &tenant_pool).await {
            error!("TradeMonitor: error polling service {}: {:#}", service_id, e);
        }
        tokio::time::sleep(std::time::Duration::from_secs(1)).await;
    }
}

async fn poll_once(ctx: &AppContext, service_id: i32, platform: Platform, tenant_pool: &sqlx::PgPool) -> Result<()> {
    // ctx.mt5_pool is platform-agnostic despite the name — it just resolves the
    // MySQL connection string from trd."_TradeService"."Configuration" by service_id.
    let mysql_pool = ctx.mt5_pool(service_id, tenant_pool).await?;

    let (after_time, after_id) = read_cursor(ctx, service_id).await?;

    let trades = match platform {
        Platform::Mt5 => poll_mt5_trades(&mysql_pool, service_id, after_time, after_id).await?,
        Platform::Mt4 => poll_mt4_trades(&mysql_pool, service_id, after_time, after_id).await?,
    };
    if trades.is_empty() {
        return Ok(());
    }

    publish_and_advance_cursor(ctx, service_id, after_time, after_id, trades).await
}

async fn read_cursor(ctx: &AppContext, service_id: i32) -> Result<(DateTime<Utc>, u64)> {
    let last_time_key = format!("{}{}", LAST_TIME_KEY_PREFIX, service_id);
    let last_deal_key = format!("{}{}", LAST_DEAL_KEY_PREFIX, service_id);

    let after_time: DateTime<Utc> = ctx
        .cache
        .get_string(&last_time_key)
        .await?
        .and_then(|s| s.parse::<DateTime<Utc>>().ok())
        .unwrap_or_else(|| Utc::now() - chrono::Duration::hours(24));

    let after_id: u64 = ctx
        .cache
        .get_string(&last_deal_key)
        .await?
        .and_then(|s| s.parse().ok())
        .unwrap_or(0);

    Ok((after_time, after_id))
}

async fn poll_mt5_trades(
    mt5_pool: &sqlx::MySqlPool,
    service_id: i32,
    after_time: DateTime<Utc>,
    after_deal: u64,
) -> Result<Vec<MetaTrade>> {
    let closed_deals = mt5::poll_closed_deals(mt5_pool, after_time, after_deal, 200).await?;
    if closed_deals.is_empty() {
        return Ok(vec![]);
    }

    // Fetch open deals for open price/time
    let position_ids: Vec<u64> = closed_deals.iter().map(|d| d.position_id).collect();
    let open_deals = mt5::get_open_deals_by_positions(mt5_pool, &position_ids).await?;
    let open_map: HashMap<u64, &mt5::Mt5OpenDeal> =
        open_deals.iter().map(|o| (o.position_id, o)).collect();

    Ok(closed_deals
        .iter()
        .map(|closed| {
            let open_opt = open_map.get(&closed.position_id).copied();
            build_meta_trade(closed, open_opt, service_id)
        })
        .collect())
}

async fn poll_mt4_trades(
    mt4_pool: &sqlx::MySqlPool,
    service_id: i32,
    after_time: DateTime<Utc>,
    after_ticket: u64,
) -> Result<Vec<MetaTrade>> {
    let closed_trades = mt4::poll_closed_trades(mt4_pool, after_time, after_ticket as i64, 200).await?;
    Ok(closed_trades
        .iter()
        .map(|t| build_meta_trade_mt4(t, service_id))
        .collect())
}

/// Publishes each trade not already enqueued (dedup hash), then advances the
/// Redis cursor. `trades` must be in ascending (close time, id) order.
///
/// Cursor must advance for every trade seen, even ones already enqueued
/// (dedup hit) — otherwise a batch that's entirely dedup hits never moves
/// last_time/last_id, the next poll fetches the exact same batch again, and
/// the poller stalls permanently.
async fn publish_and_advance_cursor(
    ctx: &AppContext,
    service_id: i32,
    after_time: DateTime<Utc>,
    after_id: u64,
    trades: Vec<MetaTrade>,
) -> Result<()> {
    let mut last_time = after_time;
    let mut last_id = after_id;
    let count = trades.len();

    for trade in trades {
        last_time = trade.close_at.unwrap_or(last_time);
        last_id = trade.ticket as u64;

        let field = format!("{}:{}", service_id, trade.ticket);
        if ctx.cache.hget(DEDUP_HASH_KEY, &field).await?.is_some() {
            continue; // already enqueued
        }

        let payload = serde_json::to_vec(&trade)?;
        ctx.jetstream
            .publish(SUBJECT_TRADE, payload.into())
            .await
            .map_err(|e| anyhow::anyhow!("NATS publish error: {}", e))?
            .await
            .map_err(|e| anyhow::anyhow!("NATS publish ack error: {}", e))?;

        ctx.cache.hset(DEDUP_HASH_KEY, &field, "1").await?;
    }

    // Update cursor (TTL = 30 days; keys are refreshed every poll so they stay alive)
    let last_time_key = format!("{}{}", LAST_TIME_KEY_PREFIX, service_id);
    let last_deal_key = format!("{}{}", LAST_DEAL_KEY_PREFIX, service_id);
    let cursor_ttl = std::time::Duration::from_secs(30 * 24 * 3600);
    ctx.cache
        .set_string(&last_time_key, &last_time.to_rfc3339(), cursor_ttl)
        .await?;
    ctx.cache
        .set_string(&last_deal_key, &last_id.to_string(), cursor_ttl)
        .await?;

    info!("TradeMonitor: service {} enqueued {} deals", service_id, count);
    Ok(())
}

fn build_meta_trade(
    closed: &mt5::Mt5ClosedDeal,
    open: Option<&mt5::Mt5OpenDeal>,
    service_id: i32,
) -> MetaTrade {
    MetaTrade {
        id: 0,
        tenant_id: 0,
        account_number: closed.login as i64,
        service_id,
        ticket: closed.deal as i64,
        symbol: closed.symbol.clone(),
        cmd: closed.action as i32,
        open_at: open.map(|o| o.time_msc),
        close_at: Some(closed.time_msc),
        time_stamp: closed.timestamp,
        position: Some(closed.position_id as i64),
        digits: closed.digits as i32,
        volume: closed.volume_closed as f64 / 10000.0,
        volume_original: (closed.volume_closed / 100) as i32,
        open_price: open.and_then(|o| Decimal::from_f64(o.price)),
        close_price: Decimal::from_f64(closed.price),
        reason: closed.reason as i32,
        profit: Decimal::from_f64(closed.profit).unwrap_or(Decimal::ZERO),
        commission: Decimal::from_f64(closed.commission).unwrap_or(Decimal::ZERO),
        swaps: Decimal::from_f64(closed.storage).unwrap_or(Decimal::ZERO),
    }
}

/// MT4 stores open+close data in a single MT4_TRADES row (unlike MT5's separate
/// open/close deal rows), so unlike build_meta_trade there's no position join.
fn build_meta_trade_mt4(t: &mt4::Mt4ClosedTrade, service_id: i32) -> MetaTrade {
    MetaTrade {
        id: 0,
        tenant_id: 0,
        account_number: t.login,
        service_id,
        ticket: t.ticket,
        symbol: t.symbol.clone(),
        cmd: t.cmd,
        open_at: Some(t.open_time),
        close_at: Some(t.close_time),
        time_stamp: t.timestamp as i64,
        position: None,
        digits: t.digits,
        // MT4's VOLUME is a native int already in the "x100 lots" convention —
        // unlike MT5's VolumeClosed it does not need dividing before it lands
        // in trade_rebate_k8s.volume (see volume_original below).
        volume: t.volume as f64 / 100.0,
        volume_original: t.volume,
        open_price: Decimal::from_f64(t.open_price),
        close_price: Decimal::from_f64(t.close_price),
        reason: t.reason,
        profit: Decimal::from_f64(t.profit).unwrap_or(Decimal::ZERO),
        commission: Decimal::from_f64(t.commission).unwrap_or(Decimal::ZERO),
        swaps: Decimal::from_f64(t.swaps).unwrap_or(Decimal::ZERO),
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn sample_trade(volume: i32) -> mt4::Mt4ClosedTrade {
        mt4::Mt4ClosedTrade {
            ticket: 12345,
            login: 43591372,
            symbol: "EURUSD".to_string(),
            digits: 5,
            cmd: 0,
            volume,
            open_time: Utc::now(),
            open_price: 1.10000,
            close_time: Utc::now(),
            close_price: 1.10500,
            profit: 50.0,
            commission: -2.0,
            swaps: 0.0,
            reason: 0,
            timestamp: 1_700_000_000,
        }
    }

    #[test]
    fn volume_original_is_unscaled_native_int() {
        // A 1.00-lot MT4 trade is natively stored as VOLUME=100 — this must land
        // in trade_rebate_k8s.volume as-is (no division), matching the old C#
        // "MT4: Volume = x.Volume" (unscaled) behavior.
        let t = sample_trade(100);
        let m = build_meta_trade_mt4(&t, 20);
        assert_eq!(m.volume_original, 100);
    }

    #[test]
    fn volume_float_represents_lots() {
        // The float `volume` field represents actual lots, matching MT5's
        // post-scaling semantics (e.g. VOLUME=100 -> 1.00 lots).
        let t = sample_trade(250);
        let m = build_meta_trade_mt4(&t, 20);
        assert!((m.volume - 2.5).abs() < f64::EPSILON);
    }

    #[test]
    fn maps_service_and_ticket_fields() {
        let t = sample_trade(100);
        let m = build_meta_trade_mt4(&t, 20);
        assert_eq!(m.service_id, 20);
        assert_eq!(m.ticket, 12345);
        assert_eq!(m.account_number, 43591372);
        assert!(m.position.is_none());
        assert!(m.open_at.is_some());
        assert!(m.close_at.is_some());
    }
}
