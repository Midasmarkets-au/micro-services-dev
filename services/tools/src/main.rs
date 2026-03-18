use std::collections::HashMap;
use std::str::FromStr;
use std::sync::Arc;
use std::time::Duration;

use anyhow::{Context, Result};
use apalis::prelude::*;
use apalis_cron::{CronStream, Tick};
use aws_credential_types::Credentials;
use aws_sdk_s3::Client as S3Client;
use chrono::{Datelike, Utc};
use cron::Schedule as CronSchedule;
use reqwest::Client;
use serde::{Deserialize, Serialize};
use serde_json::{json, Value};
use tracing::{error, info};

const FF_CALENDAR_URL: &str = "https://nfs.faireconomy.media/ff_calendar_thisweek.json";
const CLAUDE_API_URL: &str = "https://api.anthropic.com/v1/messages";

fn weekly_filename() -> (String, String) {
    let week = Utc::now().date_naive().iso_week().week();
    let filename = format!("index-{week}.json");
    let s3_key = format!("data/{filename}");
    (filename, s3_key)
}

/// Daily 09:00 UTC
const CRON_SCHEDULE: &str = "0 0 9 * * *";

const LANGS: &[(&str, &str)] = &[
    ("zh_CN", "简体中文"),
    ("zh_TW", "繁體中文"),
    ("ja", "日本語"),
    ("ko", "한국어"),
    ("vi", "Tiếng Việt"),
    ("ms", "Bahasa Melayu"),
    ("mn", "Монгол"),
    ("th", "ภาษาไทย"),
];

// ── App context ───────────────────────────────────────────────────────────────

#[derive(Clone)]
struct AppCtx {
    http: Client,
    anthropic_key: String,
    s3: Arc<S3Client>,
    s3_bucket: String,
}

impl AppCtx {
    fn from_env() -> Result<Self> {
        let anthropic_key =
            std::env::var("ANTHROPIC_API_KEY").context("ANTHROPIC_API_KEY not set")?;
        let access_key = std::env::var("AWS_S3_ACCESS_KEY").context("AWS_S3_ACCESS_KEY not set")?;
        let secret_key =
            std::env::var("AWS_S3_ACCESS_SECRET").context("AWS_S3_ACCESS_SECRET not set")?;
        let region =
            std::env::var("AWS_S3_REGION").unwrap_or_else(|_| "ap-southeast-2".to_string());
        let s3_bucket = std::env::var("AWS_S3_BUCKET").context("AWS_S3_BUCKET not set")?;

        let creds = Credentials::new(&access_key, &secret_key, None, None, "env");
        let s3_config = aws_sdk_s3::Config::builder()
            .credentials_provider(creds)
            .region(aws_sdk_s3::config::Region::new(region))
            .build();

        Ok(Self {
            http: Client::builder()
                .timeout(Duration::from_secs(120))
                .build()?,
            anthropic_key,
            s3: Arc::new(S3Client::from_conf(s3_config)),
            s3_bucket,
        })
    }
}

// ── Data types ────────────────────────────────────────────────────────────────

#[derive(Debug, Deserialize, Serialize, Clone)]
struct CalendarEvent {
    title: String,
    #[serde(flatten)]
    extra: HashMap<String, Value>,
}

#[derive(Debug, Serialize)]
struct TranslatedEvent {
    #[serde(rename = "en")]
    title: String,
    #[serde(flatten)]
    translations: HashMap<String, String>,
    #[serde(flatten)]
    extra: HashMap<String, Value>,
}

// ── Main job logic ────────────────────────────────────────────────────────────

async fn fetch_translate_upload(ctx: &AppCtx) -> Result<()> {
    info!("Starting FF Calendar fetch & translate job");

    let (output_file, s3_key) = weekly_filename();
    info!("Output: {output_file}  S3: {s3_key}");

    // 1. Fetch
    let events: Vec<CalendarEvent> = ctx
        .http
        .get(FF_CALENDAR_URL)
        .send()
        .await?
        .error_for_status()?
        .json()
        .await?;
    info!("Fetched {} events", events.len());

    if events.is_empty() {
        upload_s3(
            ctx,
            &s3_key,
            serde_json::to_vec_pretty(&Vec::<TranslatedEvent>::new())?,
        )
        .await?;
        return Ok(());
    }

    // 2. Unique titles
    let mut unique_titles: Vec<String> = events
        .iter()
        .map(|e| e.title.clone())
        .collect::<std::collections::HashSet<_>>()
        .into_iter()
        .collect();
    unique_titles.sort_unstable();

    info!(
        "Translating {} unique titles into {} languages",
        unique_titles.len(),
        LANGS.len()
    );

    // 3. Translate in batches of 30
    let mut translations: HashMap<String, HashMap<String, String>> = HashMap::new();
    let total_batches = (unique_titles.len() + 29) / 30;
    for (i, chunk) in unique_titles.chunks(30).enumerate() {
        info!(
            "  Batch {}/{} ({} titles)",
            i + 1,
            total_batches,
            chunk.len()
        );
        let batch = translate_with_claude(&ctx.http, &ctx.anthropic_key, chunk).await?;
        translations.extend(batch);
    }

    // 4. Build translated events
    let translated: Vec<TranslatedEvent> = events
        .into_iter()
        .map(|e| {
            let lang_map = translations.get(&e.title).cloned().unwrap_or_default();
            TranslatedEvent {
                title: e.title,
                translations: lang_map,
                extra: e.extra,
            }
        })
        .collect();

    // 5. Save local file
    let json_bytes = serde_json::to_vec_pretty(&translated)?;
    std::fs::write(&output_file, &json_bytes)?;
    info!("Saved: {output_file}");

    // 6. Upload to S3
    upload_s3(ctx, &s3_key, json_bytes).await?;

    Ok(())
}

async fn upload_s3(ctx: &AppCtx, s3_key: &str, data: Vec<u8>) -> Result<()> {
    info!("Uploading to s3://{}/{s3_key}", ctx.s3_bucket);
    ctx.s3
        .put_object()
        .bucket(&ctx.s3_bucket)
        .key(s3_key)
        .body(data.into())
        .content_type("application/json")
        .send()
        .await
        .with_context(|| format!("S3 upload failed: {s3_key}"))?;
    info!("S3 upload complete");
    Ok(())
}

// ── Claude translation ────────────────────────────────────────────────────────

async fn translate_with_claude(
    http: &Client,
    api_key: &str,
    titles: &[String],
) -> Result<HashMap<String, HashMap<String, String>>> {
    let lang_list = LANGS
        .iter()
        .map(|(code, name)| format!("{code}({name})"))
        .collect::<Vec<_>>()
        .join(", ");

    let numbered = titles
        .iter()
        .enumerate()
        .map(|(i, t)| format!("{}. {}", i + 1, t))
        .collect::<Vec<_>>()
        .join("\n");

    let example = format!(
        "{{\"idx\":1,{}}}",
        LANGS
            .iter()
            .map(|(code, _)| format!("\"{code}\":\"翻译\""))
            .collect::<Vec<_>>()
            .join(",")
    );

    let prompt = format!(
        "你是专业财经翻译。将以下财经日历标题翻译成多种语言。\n\
        目标语言：{lang_list}\n\n\
        要求：\n\
        - 使用标准财经术语（m/m=月率, y/y=年率, q/q=季率）\n\
        - 缩写使用权威全称（CPI=消费者物价指数, PPI=生产者物价指数 等）\n\
        - 严格输出一个JSON数组，每个元素格式：{example}\n\
        - 只输出JSON数组，不要任何解释或markdown代码块\n\n\
        标题列表：\n{numbered}"
    );

    let resp: Value = http
        .post(CLAUDE_API_URL)
        .header("x-api-key", api_key)
        .header("anthropic-version", "2023-06-01")
        .json(&json!({
            "model": "claude-opus-4-6",
            "max_tokens": 8192,
            "messages": [{ "role": "user", "content": prompt }]
        }))
        .send()
        .await
        .context("Claude API request failed")?
        .error_for_status()
        .context("Claude API error")?
        .json()
        .await?;

    let text = resp["content"][0]["text"]
        .as_str()
        .context("Unexpected Claude response structure")?
        .trim()
        .trim_start_matches("```json")
        .trim_start_matches("```")
        .trim_end_matches("```")
        .trim();

    let array: Vec<Value> = serde_json::from_str(text)
        .with_context(|| format!("Failed to parse Claude response:\n{text}"))?;

    let mut result = HashMap::new();
    for obj in &array {
        let Some(idx) = obj["idx"].as_u64() else {
            continue;
        };
        let idx = idx as usize;
        if idx < 1 || idx > titles.len() {
            continue;
        }
        let mut lang_map = HashMap::new();
        for (code, _) in LANGS {
            if let Some(v) = obj[*code].as_str() {
                lang_map.insert(code.to_string(), v.to_string());
            }
        }
        result.insert(titles[idx - 1].clone(), lang_map);
    }

    info!("Parsed {}/{} translations", result.len(), titles.len());
    Ok(result)
}

// ── Health server ─────────────────────────────────────────────────────────────

async fn start_health_server(port: u16) {
    use tokio::io::AsyncWriteExt;
    use tokio::net::TcpListener;

    let listener = TcpListener::bind(format!("0.0.0.0:{port}"))
        .await
        .expect("failed to bind health server");
    info!("Health server listening on :{port}");

    loop {
        if let Ok((mut stream, _)) = listener.accept().await {
            tokio::spawn(async move {
                let _ = stream
                    .write_all(b"HTTP/1.1 200 OK\r\nContent-Length: 2\r\n\r\nOK")
                    .await;
            });
        }
    }
}

// ── Entry point ───────────────────────────────────────────────────────────────

#[tokio::main]
async fn main() -> Result<()> {
    let _ = dotenvy::dotenv();

    tracing_subscriber::fmt()
        .with_env_filter(std::env::var("RUST_LOG").unwrap_or_else(|_| "tools=info".to_string()))
        .init();

    let health_port: u16 = std::env::var("HEALTH_PORT")
        .ok()
        .and_then(|v| v.parse().ok())
        .unwrap_or(8080);
    tokio::spawn(start_health_server(health_port));

    let ctx = AppCtx::from_env()?;

    // Run once on startup to ensure data is current
    info!("Running initial fetch on startup ...");
    if let Err(e) = fetch_translate_upload(&ctx).await {
        error!("Startup fetch failed: {e:#}");
    }

    // Schedule daily cron: 09:00 UTC
    info!("Starting cron worker — {CRON_SCHEDULE} (daily 09:00 UTC)");
    let schedule = CronSchedule::from_str(CRON_SCHEDULE).context("Invalid cron expression")?;

    let worker = WorkerBuilder::new("ff-calendar-daily")
        .backend(CronStream::new_with_timezone(schedule, Utc))
        .data(ctx)
        .build(cron_job);

    let _ = worker.run().await;
    Ok(())
}

async fn cron_job(_tick: Tick<Utc>, ctx: Data<AppCtx>) {
    if let Err(e) = fetch_translate_upload(&ctx).await {
        error!("Weekly cron job failed: {e:#}");
    }
}
