//! Event-driven rebate release.
//!
//! Credits a single rebate to the payee's wallet as soon as it has been
//! calculated, instead of waiting for the next `release_rebate` sweep. That
//! sweep stays in place as a backstop: anything dropped here still has matter
//! state `RebateOnHold` and is picked up within two minutes.

use anyhow::Result;
use futures::StreamExt;
use tracing::{error, info, warn};

use crate::db;
use crate::jobs::row_lock;
use crate::nats::client::{
    RebateReleaseMsg, REBATE_RELEASE_CONSUMER_NAME, STREAM_REBATE_RELEASE,
};
use crate::AppContext;

pub async fn run(ctx: AppContext) -> Result<()> {
    let stream = ctx
        .jetstream
        .get_stream(STREAM_REBATE_RELEASE)
        .await
        .map_err(|e| {
            anyhow::anyhow!("Failed to get NATS stream {}: {}", STREAM_REBATE_RELEASE, e)
        })?;

    let consumer = stream
        .get_consumer::<async_nats::jetstream::consumer::pull::Config>(REBATE_RELEASE_CONSUMER_NAME)
        .await
        .map_err(|e| {
            anyhow::anyhow!(
                "Failed to get NATS consumer {}: {}",
                REBATE_RELEASE_CONSUMER_NAME,
                e
            )
        })?;

    info!("RebateReleaseHandler: listening on NATS stream {}", STREAM_REBATE_RELEASE);

    loop {
        let mut messages = match consumer.messages().await {
            Ok(m) => m,
            Err(e) => {
                warn!("RebateReleaseHandler: failed to open message stream (will retry): {:#}", e);
                tokio::time::sleep(std::time::Duration::from_secs(1)).await;
                continue;
            }
        };

        while let Some(msg_result) = messages.next().await {
            match msg_result {
                Ok(msg) => {
                    if let Err(e) = process_message(&ctx, msg).await {
                        error!("RebateReleaseHandler: failed to process message: {:#}", e);
                    }
                }
                Err(e) => {
                    error!("RebateReleaseHandler: message stream error: {:#}", e);
                    break;
                }
            }
        }

        warn!("RebateReleaseHandler: message stream ended, reconnecting");
        tokio::time::sleep(std::time::Duration::from_secs(1)).await;
    }
}

async fn process_message(ctx: &AppContext, msg: async_nats::jetstream::Message) -> Result<()> {
    let m: RebateReleaseMsg = match serde_json::from_slice(&msg.payload) {
        Ok(m) => m,
        Err(e) => {
            error!(
                "RebateReleaseHandler: terminating unparseable message (seq {}): {:#}",
                msg.info().map(|i| i.stream_sequence).unwrap_or(0),
                e
            );
            msg.ack_with(async_nats::jetstream::AckKind::Term).await.ok();
            return Ok(());
        }
    };

    // Mirrors the sweep's check so the kill switch disables both paths.
    if ctx.cache.get_string("ReleaseRebateDisabledKey").await?.is_some() {
        msg.ack().await.ok();
        return Ok(());
    }

    let pool = ctx.tenant_pool(m.tenant_id).await?;

    if !db::rebate_calc::is_rebate_enabled(&pool).await? {
        msg.ack().await.ok();
        return Ok(());
    }

    let lock_key = row_lock::release_key(m.tenant_id, m.rebate_id);
    let outcome = row_lock::with_row_lock(
        &ctx.cache,
        &lock_key,
        db::rebate::process_rebate(&pool, m.rebate_id),
    )
    .await;

    // Every branch acks. The rebate keeps its `RebateOnHold` state until it is
    // actually credited, so the sweep is the retry mechanism — redelivering here
    // would only contend with it.
    msg.ack().await.ok();

    match outcome {
        Ok(Some(())) => info!(
            "RebateReleaseHandler: released rebate_id={} tenant={}",
            m.rebate_id, m.tenant_id
        ),
        Ok(None) => info!(
            "RebateReleaseHandler: rebate_id={} locked by another worker, leaving to it",
            m.rebate_id
        ),
        Err(e) => error!(
            "RebateReleaseHandler: failed for rebate_id={} tenant={}, leaving to cron: {:#}",
            m.rebate_id, m.tenant_id, e
        ),
    }

    Ok(())
}
