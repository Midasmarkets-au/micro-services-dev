use anyhow::Result;
use async_nats::jetstream::{self, consumer, stream};

pub const STREAM_NAME: &str = "BCR_TRADE";
pub const SUBJECT_TRADE: &str = "trade.meta";
pub const CONSUMER_NAME: &str = "trade-handler";

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
