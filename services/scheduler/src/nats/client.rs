use anyhow::Result;
use async_nats::jetstream::{self, consumer, stream};
use serde::{Deserialize, Serialize};

pub const STREAM_NAME: &str = "BCR_TRADE";
pub const SUBJECT_TRADE: &str = "trade.meta";
pub const CONSUMER_NAME: &str = "trade-handler";

pub const STREAM_EVENT: &str = "BCR_EVENT_TRADE";
pub const SUBJECT_EVENT: &str = "trade.event";

/// MQSource mirrors C# EventShopPointTransaction.MQSource.
/// SourceType=2 corresponds to EventShopPointTransactionSourceTypes.Trade.
#[derive(Serialize)]
#[serde(rename_all = "PascalCase")]
struct MqSource {
    source_type: u8,
    row_id: i64,
    tenant_id: i64,
}

/// Ensures the BCR_EVENT_TRADE JetStream stream exists.
/// Uses get-or-create semantics so it is safe to call on every startup.
pub async fn ensure_event_stream(js: &jetstream::Context) -> Result<()> {
    js.get_or_create_stream(stream::Config {
        name: STREAM_EVENT.to_string(),
        subjects: vec![SUBJECT_EVENT.to_string()],
        retention: stream::RetentionPolicy::WorkQueue,
        ..Default::default()
    })
    .await
    .map_err(|e| anyhow::anyhow!("Failed to get/create stream {}: {}", STREAM_EVENT, e))?;

    Ok(())
}

/// Publishes an MQSource message (SourceType=Trade) to BCR_EVENT_TRADE.
pub async fn publish_event_mq_source(
    js: &jetstream::Context,
    tenant_id: i64,
    trade_rebate_id: i64,
) -> Result<()> {
    let payload = serde_json::to_vec(&MqSource {
        source_type: 2,
        row_id: trade_rebate_id,
        tenant_id,
    })?;
    js.publish(SUBJECT_EVENT, payload.into())
        .await
        .map_err(|e| anyhow::anyhow!("Failed to publish to {}: {}", SUBJECT_EVENT, e))?
        .await
        .map_err(|e| anyhow::anyhow!("Failed to ack publish to {}: {}", SUBJECT_EVENT, e))?;
    Ok(())
}

pub const EVENT_CONSUMER_NAME: &str = "event-trade-handler";

pub const STREAM_SEND_MESSAGE: &str = "BCR_SEND_MESSAGE";
pub const SUBJECT_SEND_MESSAGE: &str = "send.message";
pub const SEND_MESSAGE_CONSUMER_NAME: &str = "send-message-handler";

/// Ensures the durable pull consumer for BCR_EVENT_TRADE exists.
/// Must be called after ensure_event_stream().
pub async fn ensure_event_consumer(js: &jetstream::Context) -> Result<()> {
    let stream = js
        .get_stream(STREAM_EVENT)
        .await
        .map_err(|e| anyhow::anyhow!("Failed to get stream {}: {}", STREAM_EVENT, e))?;

    stream
        .get_or_create_consumer(
            EVENT_CONSUMER_NAME,
            consumer::pull::Config {
                durable_name: Some(EVENT_CONSUMER_NAME.to_string()),
                ack_policy: consumer::AckPolicy::Explicit,
                max_deliver: -1,
                max_ack_pending: 10,
                ack_wait: std::time::Duration::from_secs(300),
                ..Default::default()
            },
        )
        .await
        .map_err(|e| {
            anyhow::anyhow!("Failed to get/create consumer {}: {}", EVENT_CONSUMER_NAME, e)
        })?;

    Ok(())
}

/// Ensures the BCR_SEND_MESSAGE JetStream stream exists.
/// Uses get-or-create semantics so it is safe to call on every startup.
pub async fn ensure_send_message_stream(js: &jetstream::Context) -> Result<()> {
    js.get_or_create_stream(stream::Config {
        name: STREAM_SEND_MESSAGE.to_string(),
        subjects: vec![SUBJECT_SEND_MESSAGE.to_string()],
        retention: stream::RetentionPolicy::WorkQueue,
        ..Default::default()
    })
    .await
    .map_err(|e| {
        anyhow::anyhow!("Failed to get/create stream {}: {}", STREAM_SEND_MESSAGE, e)
    })?;

    Ok(())
}

/// Ensures the durable pull consumer for BCR_SEND_MESSAGE exists.
/// Must be called after ensure_send_message_stream().
pub async fn ensure_send_message_consumer(js: &jetstream::Context) -> Result<()> {
    let stream = js
        .get_stream(STREAM_SEND_MESSAGE)
        .await
        .map_err(|e| anyhow::anyhow!("Failed to get stream {}: {}", STREAM_SEND_MESSAGE, e))?;

    stream
        .get_or_create_consumer(
            SEND_MESSAGE_CONSUMER_NAME,
            consumer::pull::Config {
                durable_name: Some(SEND_MESSAGE_CONSUMER_NAME.to_string()),
                ack_policy: consumer::AckPolicy::Explicit,
                max_deliver: 3,
                max_ack_pending: 10,
                ack_wait: std::time::Duration::from_secs(120),
                ..Default::default()
            },
        )
        .await
        .map_err(|e| {
            anyhow::anyhow!(
                "Failed to get/create consumer {}: {}",
                SEND_MESSAGE_CONSUMER_NAME,
                e
            )
        })?;

    Ok(())
}

// ── Rebate pipeline ───────────────────────────────────────────────────────────
//
// Separate from BCR_EVENT_TRADE because that stream is published conditionally —
// `trade_handler` skips stop-outs, margin calls and sub-minute trades, which is
// the loyalty-points rule. Rebates must be calculated for every closed trade, so
// reusing it would leave those trades to the cron sweep alone. Keeping the
// streams apart also stops a backlog in one pipeline from delaying the other.

pub const STREAM_REBATE_CALC: &str = "BCR_REBATE_CALC";
pub const SUBJECT_REBATE_CALC: &str = "rebate.calc";
pub const REBATE_CALC_CONSUMER_NAME: &str = "rebate-calc-handler";

pub const STREAM_REBATE_RELEASE: &str = "BCR_REBATE_RELEASE";
pub const SUBJECT_REBATE_RELEASE: &str = "rebate.release";
pub const REBATE_RELEASE_CONSUMER_NAME: &str = "rebate-release-handler";

/// A trade_rebate row awaiting calculation.
#[derive(Serialize, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct RebateCalcMsg {
    pub tenant_id: i64,
    pub trade_rebate_id: i64,
}

/// A calculated rebate awaiting release into the payee's wallet.
#[derive(Serialize, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct RebateReleaseMsg {
    pub tenant_id: i64,
    pub rebate_id: i64,
}

/// `max_ack_pending` is higher than the trade streams' 10 because one rebate
/// calculation walks a multi-level agent chain and queries MT prices, so it is
/// slower than the plain insert those streams perform.
const REBATE_MAX_ACK_PENDING: i64 = 50;

async fn ensure_work_queue(
    js: &jetstream::Context,
    stream_name: &str,
    subject: &str,
    consumer_name: &str,
) -> Result<()> {
    let stream = js
        .get_or_create_stream(stream::Config {
            name: stream_name.to_string(),
            subjects: vec![subject.to_string()],
            retention: stream::RetentionPolicy::WorkQueue,
            ..Default::default()
        })
        .await
        .map_err(|e| anyhow::anyhow!("Failed to get/create stream {}: {}", stream_name, e))?;

    stream
        .get_or_create_consumer(
            consumer_name,
            consumer::pull::Config {
                durable_name: Some(consumer_name.to_string()),
                ack_policy: consumer::AckPolicy::Explicit,
                max_deliver: -1,
                max_ack_pending: REBATE_MAX_ACK_PENDING,
                ack_wait: std::time::Duration::from_secs(300),
                ..Default::default()
            },
        )
        .await
        .map_err(|e| anyhow::anyhow!("Failed to get/create consumer {}: {}", consumer_name, e))?;

    Ok(())
}

pub async fn ensure_rebate_calc_stream(js: &jetstream::Context) -> Result<()> {
    ensure_work_queue(
        js,
        STREAM_REBATE_CALC,
        SUBJECT_REBATE_CALC,
        REBATE_CALC_CONSUMER_NAME,
    )
    .await
}

pub async fn ensure_rebate_release_stream(js: &jetstream::Context) -> Result<()> {
    ensure_work_queue(
        js,
        STREAM_REBATE_RELEASE,
        SUBJECT_REBATE_RELEASE,
        REBATE_RELEASE_CONSUMER_NAME,
    )
    .await
}

pub async fn publish_rebate_calc(
    js: &jetstream::Context,
    tenant_id: i64,
    trade_rebate_id: i64,
) -> Result<()> {
    let payload = serde_json::to_vec(&RebateCalcMsg {
        tenant_id,
        trade_rebate_id,
    })?;
    js.publish(SUBJECT_REBATE_CALC, payload.into())
        .await
        .map_err(|e| anyhow::anyhow!("Failed to publish to {}: {}", SUBJECT_REBATE_CALC, e))?
        .await
        .map_err(|e| anyhow::anyhow!("Failed to ack publish to {}: {}", SUBJECT_REBATE_CALC, e))?;
    Ok(())
}

pub async fn publish_rebate_release(
    js: &jetstream::Context,
    tenant_id: i64,
    rebate_id: i64,
) -> Result<()> {
    let payload = serde_json::to_vec(&RebateReleaseMsg {
        tenant_id,
        rebate_id,
    })?;
    js.publish(SUBJECT_REBATE_RELEASE, payload.into())
        .await
        .map_err(|e| anyhow::anyhow!("Failed to publish to {}: {}", SUBJECT_REBATE_RELEASE, e))?
        .await
        .map_err(|e| {
            anyhow::anyhow!("Failed to ack publish to {}: {}", SUBJECT_REBATE_RELEASE, e)
        })?;
    Ok(())
}

/// Ensures the BCR_TRADE JetStream stream and durable pull consumer exist.
/// Uses get-or-create semantics so it is safe to call on every startup.
pub async fn ensure_trade_stream(js: &jetstream::Context) -> Result<()> {
    // Create or get the stream
    let stream = js
        .get_or_create_stream(stream::Config {
            name: STREAM_NAME.to_string(),
            subjects: vec![SUBJECT_TRADE.to_string()],
            retention: stream::RetentionPolicy::WorkQueue,
            ..Default::default()
        })
        .await
        .map_err(|e| anyhow::anyhow!("Failed to get/create stream {}: {}", STREAM_NAME, e))?;

    // Create or get the durable pull consumer.
    // max_deliver: -1 = unlimited retries (no data loss on transient failures).
    // ack_wait: 5 minutes — gives enough time for DDL + DB write before redelivery.
    stream
        .get_or_create_consumer(
            CONSUMER_NAME,
            consumer::pull::Config {
                durable_name: Some(CONSUMER_NAME.to_string()),
                ack_policy: consumer::AckPolicy::Explicit,
                max_deliver: -1,
                max_ack_pending: 10,
                ack_wait: std::time::Duration::from_secs(300),
                ..Default::default()
            },
        )
        .await
        .map_err(|e| {
            anyhow::anyhow!("Failed to get/create consumer {}: {}", CONSUMER_NAME, e)
        })?;

    Ok(())
}
