use anyhow::Result;
use futures::StreamExt;
use tracing::{error, info, warn};

use crate::db::{tenant, trade_rebate};
use crate::models::meta_trade::MetaTrade;
use crate::nats::client::{CONSUMER_NAME, STREAM_NAME};
use crate::AppContext;

const ACCOUNT_TENANT_HASH_KEY: &str = "account:tenant:map";

/// Entry point: runs the NATS pull consumer loop indefinitely.
pub async fn run(ctx: AppContext) -> Result<()> {
    let stream = ctx
        .jetstream
        .get_stream(STREAM_NAME)
        .await
        .map_err(|e| anyhow::anyhow!("Failed to get NATS stream {}: {}", STREAM_NAME, e))?;

    let consumer = stream
        .get_consumer::<async_nats::jetstream::consumer::pull::Config>(CONSUMER_NAME)
        .await
        .map_err(|e| anyhow::anyhow!("Failed to get NATS consumer {}: {}", CONSUMER_NAME, e))?;

    info!("TradeHandler: listening on NATS stream {}", STREAM_NAME);

    loop {
        let mut messages = match consumer.fetch().max_messages(10).messages().await {
            Ok(m) => m,
            Err(e) => {
                warn!("TradeHandler: fetch error (will retry): {:#}", e);
                tokio::time::sleep(std::time::Duration::from_secs(1)).await;
                continue;
            }
        };

        while let Some(msg_result) = messages.next().await {
            match msg_result {
                Ok(msg) => {
                    if let Err(e) = process_message(&ctx, msg).await {
                        error!("TradeHandler: failed to process message: {:#}", e);
                    }
                }
                Err(e) => error!("TradeHandler: message error: {:#}", e),
            }
        }

        tokio::time::sleep(std::time::Duration::from_secs(5)).await;
    }
}

async fn process_message(
    ctx: &AppContext,
    msg: async_nats::jetstream::Message,
) -> Result<()> {
    let trade: MetaTrade = serde_json::from_slice(&msg.payload)?;

    // Find which tenant owns this account
    let tenant_id = match get_account_tenant_id(ctx, trade.account_number, trade.service_id).await? {
        Some(id) => id,
        None => {
            warn!(
                "TradeHandler: no tenant found for account {} service {}",
                trade.account_number, trade.service_id
            );
            msg.ack().await.ok();
            return Ok(());
        }
    };

    let pool = ctx.tenant_pool(tenant_id).await?;

    // Dedup check across all partitions of trade_rebate_k8s
    if trade_rebate::exists(&pool, trade.ticket, trade.service_id).await? {
        msg.ack().await.ok();
        return Ok(());
    }

    // Build rebate with account info
    let mut rebate = trade.to_new_trade_rebate();
    rebate.id = ctx.idgen.generate_id().await?;
    if let Some((account_id, currency_id, refer_path)) =
        tenant::find_account_by_number(&pool, trade.account_number, trade.service_id).await?
    {
        rebate.account_id = Some(account_id);
        rebate.currency_id = currency_id;
        rebate.refer_path = refer_path;
    }

    trade_rebate::insert(&pool, &rebate).await?;
    msg.ack().await.ok();

    info!(
        "TradeHandler: saved TradeRebateK8s ticket={} service={} tenant={}",
        trade.ticket, trade.service_id, tenant_id
    );
    Ok(())
}

/// Finds the tenant that owns the given account number + service ID combination.
/// Uses Redis Hash cache (no TTL) to avoid repeated DB scans.
async fn get_account_tenant_id(
    ctx: &AppContext,
    account_number: i64,
    service_id: i32,
) -> Result<Option<i64>> {
    let field = format!("{}:{}", account_number, service_id);

    if let Some(cached) = ctx.cache.hget(ACCOUNT_TENANT_HASH_KEY, &field).await? {
        if let Ok(id) = cached.parse::<i64>() {
            return Ok(Some(id));
        }
    }

    let tenant_ids = tenant::get_all_tenant_ids(&ctx.central_pool).await?;
    for tenant_id in tenant_ids {
        let pool = ctx.tenant_pool(tenant_id).await?;
        if let Some((account_id, _, _)) =
            tenant::find_account_by_number(&pool, account_number, service_id).await?
        {
            let _ = account_id; // we only need tenant_id here
            ctx.cache
                .hset(ACCOUNT_TENANT_HASH_KEY, &field, &tenant_id.to_string())
                .await?;
            return Ok(Some(tenant_id));
        }
    }

    Ok(None)
}
