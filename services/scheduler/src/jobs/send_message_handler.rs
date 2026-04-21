/// SendMessageHandler: consumes BCR_SEND_MESSAGE NATS stream.
///
/// Handles one active category:
///   category=2 (BatchEmail): send a templated marketing email to one recipient.
///     Mirrors: PollSendMessageHandler → BatchSendEmailService.SendEmailByTopicIdWithContent
///
/// category=1 (GeneralEmail) is declared but has no active producer in mono; it is logged and acked.
use anyhow::Result;
use futures::StreamExt;
use serde::Deserialize;
use std::collections::HashMap;
use std::time::Duration;
use tracing::{error, info, warn};

use crate::nats::client::{SEND_MESSAGE_CONSUMER_NAME, STREAM_SEND_MESSAGE};
use crate::AppContext;

const CATEGORY_BATCH_EMAIL: u8 = 2;
const MAX_TRY_TIME: i32 = 3;
const TWO_DAYS_SECS: u64 = 2 * 24 * 3600;

/// Mirrors C# SendMessageMqDTO.
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
struct SendMessageMqDto {
    category: u8,
    data: String,
}

/// Mirrors C# SendBatchEmailDTO.
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
struct SendBatchEmailDto {
    tenant_id: i64,
    #[allow(dead_code)]
    user_id: i64,
    email: String,
    language: String,
    topic_key: String,
    topic_id: i64,
}

/// Template content for one language (from SendBatchEmailInfo.Contents[lang]).
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
struct SendLanguageSpec {
    title: String,
    sub_title: String,
    content: String,
}

/// SendBatchEmailInfo stored in Configuration table (only fields we need).
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
struct SendBatchEmailInfo {
    #[allow(dead_code)]
    total: Option<i64>,
    contents: Option<HashMap<String, SendLanguageSpec>>,
}

/// Entry point: runs the NATS pull consumer loop indefinitely.
pub async fn run(ctx: AppContext) -> Result<()> {
    let stream = ctx
        .jetstream
        .get_stream(STREAM_SEND_MESSAGE)
        .await
        .map_err(|e| {
            anyhow::anyhow!(
                "SendMessageHandler: failed to get stream {}: {}",
                STREAM_SEND_MESSAGE,
                e
            )
        })?;

    let consumer = stream
        .get_consumer::<async_nats::jetstream::consumer::pull::Config>(SEND_MESSAGE_CONSUMER_NAME)
        .await
        .map_err(|e| {
            anyhow::anyhow!(
                "SendMessageHandler: failed to get consumer {}: {}",
                SEND_MESSAGE_CONSUMER_NAME,
                e
            )
        })?;

    info!(
        "SendMessageHandler: listening on NATS stream {}",
        STREAM_SEND_MESSAGE
    );

    loop {
        let mut messages = match consumer.fetch().max_messages(10).messages().await {
            Ok(m) => m,
            Err(e) => {
                warn!("SendMessageHandler: fetch error (will retry): {:#}", e);
                tokio::time::sleep(Duration::from_secs(1)).await;
                continue;
            }
        };

        while let Some(msg_result) = messages.next().await {
            match msg_result {
                Ok(msg) => {
                    if let Err(e) = process_message(&ctx, msg).await {
                        error!("SendMessageHandler: failed to process message: {:#}", e);
                    }
                }
                Err(e) => error!("SendMessageHandler: message error: {:#}", e),
            }
        }

        tokio::time::sleep(Duration::from_secs(1)).await;
    }
}

async fn process_message(ctx: &AppContext, msg: async_nats::jetstream::Message) -> Result<()> {
    let dto: SendMessageMqDto = match serde_json::from_slice(&msg.payload) {
        Ok(d) => d,
        Err(e) => {
            warn!("SendMessageHandler: invalid message payload: {:#}", e);
            msg.ack().await.ok();
            return Ok(());
        }
    };

    match dto.category {
        CATEGORY_BATCH_EMAIL => {
            let batch_dto: SendBatchEmailDto = match serde_json::from_str(&dto.data) {
                Ok(d) => d,
                Err(e) => {
                    warn!("SendMessageHandler: invalid BatchEmail data: {:#}", e);
                    msg.ack().await.ok();
                    return Ok(());
                }
            };

            let delivered = msg
                .info()
                .map(|i| i.delivered)
                .unwrap_or(1);

            // Mirror C# MaxTryTime=3: after 3 deliveries, give up and ack to avoid infinite loop.
            if delivered > MAX_TRY_TIME as i64 {
                warn!(
                    "SendMessageHandler: BatchEmail max retries exceeded for email={} topic_key={}, giving up",
                    batch_dto.email, batch_dto.topic_key
                );
                let stats_key = batch_email_stats_key(batch_dto.tenant_id);
                ctx.cache.hincrby(&stats_key, "Failed", 1).await.ok();
                check_and_finalize(ctx, batch_dto.tenant_id, batch_dto.topic_id, &batch_dto.topic_key).await.ok();
                msg.ack().await.ok();
                return Ok(());
            }

            match process_batch_email(ctx, &batch_dto).await {
                Ok(true) => {
                    // success
                    msg.ack().await.ok();
                }
                Ok(false) => {
                    // skip (dedup / no-promotion) — still ack so we don't retry
                    msg.ack().await.ok();
                }
                Err(e) => {
                    // transient error — do NOT ack; NATS will redeliver
                    error!(
                        "SendMessageHandler: BatchEmail send failed email={} (attempt {}): {:#}",
                        batch_dto.email, delivered, e
                    );
                    // Return the error so the message is not acked and NATS redelivers.
                    return Err(e);
                }
            }
        }
        1 => {
            // GeneralEmail — no active producer; log and skip
            info!("SendMessageHandler: GeneralEmail (category=1) not yet implemented, skipping");
            msg.ack().await.ok();
        }
        other => {
            warn!("SendMessageHandler: unknown category={}, skipping", other);
            msg.ack().await.ok();
        }
    }

    Ok(())
}

/// Process a single BatchEmail message.
/// Returns Ok(true) on sent, Ok(false) on skip (dedup/no-promo), Err on transient failure.
/// Mirrors: BatchSendEmailService.SendEmailByTopicIdWithContent
async fn process_batch_email(ctx: &AppContext, dto: &SendBatchEmailDto) -> Result<bool> {
    // 1. No-promotion check (Redis Set: GeneralJob_NoPromotionEmailCacheKey)
    let no_promo_key = "GeneralJob_NoPromotionEmailCacheKey";
    if ctx.cache.sismember(no_promo_key, &dto.email).await? {
        let stats_key = batch_email_stats_key(dto.tenant_id);
        ctx.cache.hincrby(&stats_key, "NoPromotion", 1).await.ok();
        info!(
            "SendMessageHandler: BatchEmail no-promotion email={} topic_key={}, skipping",
            dto.email, dto.topic_key
        );
        return Ok(false);
    }

    // 2. Dedup check (Redis Set: send_batch_email_has_sent_hkey_topic_{topic_key})
    let sent_key = batch_email_has_sent_key(&dto.topic_key);
    if ctx.cache.sismember(&sent_key, &dto.email).await? {
        info!(
            "SendMessageHandler: BatchEmail already sent email={} topic_key={}, skipping",
            dto.email, dto.topic_key
        );
        return Ok(false);
    }

    // 3. Get tenant pool
    let pool = ctx.tenant_pool(dto.tenant_id).await?;

    // 4. Fetch topic content from DB
    let topic_content = get_topic_content(&pool, dto.topic_id, &dto.language).await?;
    let topic_content = match topic_content {
        Some(c) => c,
        None => {
            warn!(
                "SendMessageHandler: no topic content found topicId={} language={}",
                dto.topic_id, dto.language
            );
            return Ok(false);
        }
    };

    // 5. Fetch batch email info (title/subtitle/content) from Config table
    let batch_info = get_batch_email_info(&pool, dto.tenant_id).await?;
    let batch_info = match batch_info {
        Some(i) => i,
        None => {
            warn!(
                "SendMessageHandler: no SendBatchEmailInfo found for tenant={}",
                dto.tenant_id
            );
            return Ok(false);
        }
    };

    let lang_spec = match &batch_info.contents {
        Some(contents) => {
            contents
                .get(&dto.language)
                .or_else(|| contents.get("en"))
                .ok_or_else(|| {
                    anyhow::anyhow!(
                        "No content for language={} or 'en' in tenant={}",
                        dto.language,
                        dto.tenant_id
                    )
                })?
        }
        None => {
            warn!(
                "SendMessageHandler: BatchEmailInfo has no Contents for tenant={}",
                dto.tenant_id
            );
            return Ok(false);
        }
    };

    // 6. Apply template: {{Title}}, {{Subtitle}}, {{Content}}
    let applied = topic_content
        .replace("{{Title}}", &lang_spec.title)
        .replace("{{Subtitle}}", &lang_spec.sub_title)
        .replace("{{Content}}", &lang_spec.content);

    if applied.is_empty() {
        warn!(
            "SendMessageHandler: applied template is empty for tenant={} topicId={}",
            dto.tenant_id, dto.topic_id
        );
        return Ok(false);
    }

    // 7. Fetch sender email / display name
    let sender_email = get_config_string(&pool, dto.tenant_id, "DefaultEmailAddress").await?;
    let sender_display = get_config_string(&pool, dto.tenant_id, "DefaultEmailDisplayName").await?;

    let (sender_email, sender_display) = match (sender_email, sender_display) {
        (Some(e), Some(d)) if !e.is_empty() && !d.is_empty() => (e, d),
        _ => {
            warn!(
                "SendMessageHandler: sender email/display missing for tenant={}",
                dto.tenant_id
            );
            return Ok(false);
        }
    };

    // 8. Send via SES (MailSender uses the configured from address — override not supported yet;
    //    we use the configured SES_FROM. The tenant sender_email is informational for now.)
    // NOTE: mono's IEmailSender honours senderEmail / senderDisplayName headers.
    //       scheduler's MailSender uses a single SES_FROM. The subject line doubles as title.
    let _ = sender_display; // available if header support is added later
    let _ = sender_email;
    ctx.mail.send(&dto.email, &lang_spec.title, &applied).await?;

    info!(
        "SendMessageHandler: BatchEmail sent to={} topic_key={} tenant={}",
        dto.email, dto.topic_key, dto.tenant_id
    );

    // 9. Mark as sent in Redis (TTL 2 days)
    ctx.cache
        .sadd_with_ttl(&sent_key, &dto.email, Duration::from_secs(TWO_DAYS_SECS))
        .await?;

    // 10. Increment Sent counter
    let stats_key = batch_email_stats_key(dto.tenant_id);
    ctx.cache.hincrby(&stats_key, "Sent", 1).await?;

    // 11. Check if job is finished
    check_and_finalize(ctx, dto.tenant_id, dto.topic_id, &dto.topic_key).await?;

    Ok(true)
}

/// Check if Sent + Failed == Total; if so, update Status to "Job Finished".
async fn check_and_finalize(
    ctx: &AppContext,
    tenant_id: i64,
    _topic_id: i64,
    _topic_key: &str,
) -> Result<()> {
    let stats_key = batch_email_stats_key(tenant_id);
    let total = ctx.cache.hget_i64(&stats_key, "Total").await?.unwrap_or(0);
    let sent = ctx.cache.hget_i64(&stats_key, "Sent").await?.unwrap_or(0);
    let failed = ctx.cache.hget_i64(&stats_key, "Failed").await?.unwrap_or(0);

    if total > 0 && sent + failed >= total {
        ctx.cache.hset(&stats_key, "Status", "Job Finished").await?;
        info!(
            "SendMessageHandler: BatchEmail job finished tenant={} sent={} failed={} total={}",
            tenant_id, sent, failed, total
        );
    }

    Ok(())
}

// ── DB helpers ────────────────────────────────────────────────────────────────

/// Fetch the raw Content string from pub."_TopicContent" for a given topicId+language.
/// Falls back to 'en' if the requested language is not found.
async fn get_topic_content(
    pool: &sqlx::PgPool,
    topic_id: i64,
    language: &str,
) -> Result<Option<String>> {
    let row = sqlx::query_scalar::<_, String>(
        r#"
        SELECT "Content"
        FROM pub."_TopicContent"
        WHERE "TopicId" = $1 AND "Language" IN ($2, 'en')
        ORDER BY CASE WHEN "Language" = $2 THEN 0 ELSE 1 END
        LIMIT 1
        "#,
    )
    .bind(topic_id)
    .bind(language)
    .fetch_optional(pool)
    .await?;

    Ok(row)
}

/// Fetch the SendBatchEmailInfo JSON from pub."_Configuration".
async fn get_batch_email_info(
    pool: &sqlx::PgPool,
    _tenant_id: i64,
) -> Result<Option<SendBatchEmailInfo>> {
    let raw = sqlx::query_scalar::<_, String>(
        r#"
        SELECT "Value"
        FROM pub."_Configuration"
        WHERE "Category" = 'Public' AND "RowId" = 0 AND "Key" = 'SendBatchEmailSpecKey'
        ORDER BY id DESC
        LIMIT 1
        "#,
    )
    .fetch_optional(pool)
    .await?;

    match raw {
        None => Ok(None),
        Some(json) => {
            let info: SendBatchEmailInfo = serde_json::from_str(&json)?;
            Ok(Some(info))
        }
    }
}

/// Fetch a string value from pub."_Configuration" for a given key under 'Public'/rowId=0.
async fn get_config_string(
    pool: &sqlx::PgPool,
    _tenant_id: i64,
    key: &str,
) -> Result<Option<String>> {
    // Config values are stored as JSON {"Value":"..."}; extract the inner string.
    let raw = sqlx::query_scalar::<_, String>(
        r#"
        SELECT "Value"
        FROM pub."_Configuration"
        WHERE "Category" = 'Public' AND "RowId" = 0 AND "Key" = $1
        ORDER BY id DESC
        LIMIT 1
        "#,
    )
    .bind(key)
    .fetch_optional(pool)
    .await?;

    match raw {
        None => Ok(None),
        Some(json) => {
            // Try to parse {"Value":"..."} first; fall back to raw string.
            #[derive(Deserialize)]
            struct StringValue {
                #[serde(rename = "Value")]
                value: String,
            }
            if let Ok(sv) = serde_json::from_str::<StringValue>(&json) {
                Ok(Some(sv.value))
            } else {
                Ok(Some(json))
            }
        }
    }
}

// ── Redis key helpers (mirror C# CacheKeys) ───────────────────────────────────

fn batch_email_has_sent_key(topic_key: &str) -> String {
    format!("send_batch_email_has_sent_hkey_topic_{}", topic_key)
}

fn batch_email_stats_key(tenant_id: i64) -> String {
    format!("send_batch_emails_hkey_tid{}", tenant_id)
}
