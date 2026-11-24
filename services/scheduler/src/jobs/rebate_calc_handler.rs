//! Event-driven rebate calculation.
//!
//! Calculates a single trade_rebate as soon as `trade_handler` has written it,
//! instead of waiting for the next `execute_calculate` sweep. The sweep stays in
//! place as a backstop: anything this handler drops — a lost message, a locked
//! row, a failed calculation — still has `status = 0` and is picked up within
//! two minutes.

use anyhow::Result;
use futures::StreamExt;
use tracing::{error, info, warn};

use crate::db::rebate_calc as db;
use crate::jobs::{rebate_calc, row_lock};
use crate::nats::client::{RebateCalcMsg, REBATE_CALC_CONSUMER_NAME, STREAM_REBATE_CALC};
use crate::AppContext;

pub async fn run(ctx: AppContext) -> Result<()> {
    let stream = ctx
        .jetstream
        .get_stream(STREAM_REBATE_CALC)
        .await
        .map_err(|e| anyhow::anyhow!("Failed to get NATS stream {}: {}", STREAM_REBATE_CALC, e))?;

    let consumer = stream
        .get_consumer::<async_nats::jetstream::consumer::pull::Config>(REBATE_CALC_CONSUMER_NAME)
        .await
        .map_err(|e| {
            anyhow::anyhow!("Failed to get NATS consumer {}: {}", REBATE_CALC_CONSUMER_NAME, e)
        })?;

    info!("RebateCalcHandler: listening on NATS stream {}", STREAM_REBATE_CALC);

    loop {
        let mut messages = match consumer.messages().await {
            Ok(m) => m,
            Err(e) => {
                warn!("RebateCalcHandler: failed to open message stream (will retry): {:#}", e);
                tokio::time::sleep(std::time::Duration::from_secs(1)).await;
                continue;
            }
        };

        while let Some(msg_result) = messages.next().await {
            match msg_result {
                Ok(msg) => {
                    if let Err(e) = process_message(&ctx, msg).await {
                        error!("RebateCalcHandler: failed to process message: {:#}", e);
                    }
                }
                Err(e) => {
                    error!("RebateCalcHandler: message stream error: {:#}", e);
                    break;
                }
            }
        }

        warn!("RebateCalcHandler: message stream ended, reconnecting");
        tokio::time::sleep(std::time::Duration::from_secs(1)).await;
    }
}

async fn process_message(ctx: &AppContext, msg: async_nats::jetstream::Message) -> Result<()> {
    let m: RebateCalcMsg = match serde_json::from_slice(&msg.payload) {
        Ok(m) => m,
        Err(e) => {
            // A payload that cannot parse never will. Leaving it unacked would
            // occupy an ack-pending slot and head-of-line-block the stream.
            error!(
                "RebateCalcHandler: terminating unparseable message (seq {}): {:#}",
                msg.info().map(|i| i.stream_sequence).unwrap_or(0),
                e
            );
            msg.ack_with(async_nats::jetstream::AckKind::Term).await.ok();
            return Ok(());
        }
    };

    let pool = ctx.tenant_pool(m.tenant_id).await?;

    if !db::is_rebate_enabled(&pool).await? {
        msg.ack().await.ok();
        return Ok(());
    }

    let lock_key = row_lock::calc_key(m.tenant_id, m.trade_rebate_id);
    let outcome = row_lock::with_row_lock(
        &ctx.cache,
        &lock_key,
        rebate_calc::generate_rebates(
            ctx,
            &pool,
            m.tenant_id,
            m.trade_rebate_id,
            db::TRADE_REBATE_TABLE,
        ),
    )
    .await;

    match outcome {
        Ok(Some(())) => {
            msg.ack().await.ok();
            info!(
                "RebateCalcHandler: calculated trade_rebate_id={} tenant={}",
                m.trade_rebate_id, m.tenant_id
            );
        }
        Ok(None) => {
            // The cron sweep holds this row. Redelivering would only contend
            // again, and whichever worker holds it will finish the work.
            msg.ack().await.ok();
            info!(
                "RebateCalcHandler: trade_rebate_id={} locked by another worker, leaving to it",
                m.trade_rebate_id
            );
        }
        Err(e) => {
            // Ack anyway: the row keeps `status = 0`, so the sweep retries it
            // with fresh state rather than this message replaying indefinitely.
            msg.ack().await.ok();
            error!(
                "RebateCalcHandler: failed for trade_rebate_id={} tenant={}, leaving to cron: {:#}",
                m.trade_rebate_id, m.tenant_id, e
            );
        }
    }

    Ok(())
}
