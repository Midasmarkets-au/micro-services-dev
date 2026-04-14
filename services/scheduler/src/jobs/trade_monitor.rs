use std::collections::HashMap;
use anyhow::Result;
use chrono::{DateTime, Utc};
use tracing::{error, info, warn};

use rust_decimal::Decimal;
use rust_decimal::prelude::FromPrimitive;

use crate::db::{mt5, tenant};
use crate::models::meta_trade::MetaTrade;
use crate::nats::client::SUBJECT_TRADE;
use crate::AppContext;

const DEDUP_HASH_KEY: &str = "trade:queue:dedup";
const LAST_TIME_KEY_PREFIX: &str = "trade:monitor:last_time:";
const LAST_DEAL_KEY_PREFIX: &str = "trade:monitor:last_deal:";

/// Entry point: discovers all MT5 services across all tenants and spawns a polling loop per service.
pub async fn run(ctx: AppContext) -> Result<()> {
    let tenant_ids = tenant::get_all_tenant_ids(&ctx.central_pool).await?;

    // Collect unique (service_id, tenant_pool) pairs across all tenants
    let mut service_tenant: Vec<(i32, sqlx::PgPool)> = vec![];
    let mut seen_service_ids = std::collections::HashSet::new();

    for tenant_id in tenant_ids {
        let tenant_pool = match ctx.tenant_pool(tenant_id).await {
            Ok(p) => p,
            Err(e) => {
                warn!("TradeMonitor: failed to get tenant {} pool: {:#}", tenant_id, e);
                continue;
            }
        };
        let service_ids = match tenant::get_mt5_service_ids_from_central(&tenant_pool).await {
            Ok(ids) => ids,
            Err(e) => {
                warn!("TradeMonitor: failed to get MT5 services for tenant {}: {:#}", tenant_id, e);
                continue;
            }
        };
        for service_id in service_ids {
            if seen_service_ids.insert(service_id) {
                service_tenant.push((service_id, tenant_pool.clone()));
            }
        }
    }

    if service_tenant.is_empty() {
        warn!("TradeMonitor: no MT5 trade services found in any tenant trd._TradeService. Trade monitoring will exit.");
        return Ok(());
    }

    info!("TradeMonitor: found {} MT5 service(s): {:?}", service_tenant.len(), service_tenant.iter().map(|(id, _)| id).collect::<Vec<_>>());

    let mut handles = vec![];
    for (service_id, tenant_pool) in service_tenant {
        let ctx2 = ctx.clone();
        handles.push(tokio::spawn(async move {
            poll_service_loop(ctx2, service_id, tenant_pool).await;
        }));
    }
    for h in handles {
        if let Err(e) = h.await {
            error!("TradeMonitor: poll task panicked: {:?}", e);
        }
    }
    Ok(())
}

/// Runs forever, polling one MT5 service every 1 second.
async fn poll_service_loop(ctx: AppContext, service_id: i32, tenant_pool: sqlx::PgPool) {
    info!("TradeMonitor: starting poll loop for service {}", service_id);
    let mut round: u64 = 0;
    loop {
        round += 1;
        // Every 12 hours clear dedup hash to prevent unbounded growth
        if round.is_multiple_of(43200) {
            if let Err(e) = ctx.cache.del(DEDUP_HASH_KEY).await {
                warn!("TradeMonitor: failed to clear dedup cache: {:#}", e);
            }
        }

        if let Err(e) = poll_once(&ctx, service_id, &tenant_pool).await {
            error!("TradeMonitor: error polling service {}: {:#}", service_id, e);
        }
        tokio::time::sleep(std::time::Duration::from_secs(1)).await;
    }
}

async fn poll_once(ctx: &AppContext, service_id: i32, tenant_pool: &sqlx::PgPool) -> Result<()> {
    let mt5_pool = ctx.mt5_pool(service_id, tenant_pool).await?;

    // Read cursor from Redis
    let last_time_key = format!("{}{}", LAST_TIME_KEY_PREFIX, service_id);
    let last_deal_key = format!("{}{}", LAST_DEAL_KEY_PREFIX, service_id);

    let after_time: DateTime<Utc> = ctx
        .cache
        .get_string(&last_time_key)
        .await?
        .and_then(|s| s.parse::<DateTime<Utc>>().ok())
        .unwrap_or_else(|| Utc::now() - chrono::Duration::hours(24));

    let after_deal: u64 = ctx
        .cache
        .get_string(&last_deal_key)
        .await?
        .and_then(|s| s.parse().ok())
        .unwrap_or(0);

    let closed_deals = mt5::poll_closed_deals(&mt5_pool, after_time, after_deal, 200).await?;
    if closed_deals.is_empty() {
        return Ok(());
    }

    // Fetch open deals for open price/time
    let position_ids: Vec<u64> = closed_deals.iter().map(|d| d.position_id).collect();
    let open_deals = mt5::get_open_deals_by_positions(&mt5_pool, &position_ids).await?;
    let open_map: HashMap<u64, &mt5::Mt5OpenDeal> =
        open_deals.iter().map(|o| (o.position_id, o)).collect();

    let mut last_time = after_time;
    let mut last_deal = after_deal;

    for closed in &closed_deals {
        let open_opt = open_map.get(&closed.position_id).copied();
        let trade = build_meta_trade(closed, open_opt, service_id);

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

        last_time = closed.time_msc;
        last_deal = closed.deal;
    }

    // Update cursor (TTL = 30 days; keys are refreshed every poll so they stay alive)
    let cursor_ttl = std::time::Duration::from_secs(30 * 24 * 3600);
    ctx.cache
        .set_string(&last_time_key, &last_time.to_rfc3339(), cursor_ttl)
        .await?;
    ctx.cache
        .set_string(&last_deal_key, &last_deal.to_string(), cursor_ttl)
        .await?;

    info!(
        "TradeMonitor: service {} enqueued {} deals",
        service_id,
        closed_deals.len()
    );
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
