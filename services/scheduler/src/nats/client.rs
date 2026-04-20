use anyhow::Result;
use async_nats::jetstream::{self, consumer, stream};
use serde::Serialize;

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
