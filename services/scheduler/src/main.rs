use std::collections::HashMap;
use std::net::SocketAddr;
use std::sync::Arc;
use std::time::Duration;

use percent_encoding::{utf8_percent_encode, NON_ALPHANUMERIC};

use anyhow::Result;
use apalis::prelude::*;
use apalis_board_api::framework::{ApiBuilder, RegisterRoute};
use apalis_board_api::sse::TracingBroadcaster;
use apalis_board_api::ui::ServeUI;
use apalis_redis::{connect as redis_connect, ConnectionManager, RedisStorage};
use async_nats::jetstream;
use axum::extract::Extension;
use axum::http::StatusCode;
use axum::response::Json;
use axum::routing::post;
use axum::{routing::get, Json as AxumJson, Router};
use chrono::{DateTime, Timelike, Utc};
use sqlx::{MySqlPool, PgPool};
use tokio_cron_scheduler::{Job, JobScheduler};
use tokio::sync::RwLock;
use tonic::transport::Server as TonicServer;
use tower_http::trace::TraceLayer;
use tracing::{error, info};

mod cache;
mod config;
mod db;
mod generated;
mod grpc;
mod jobs;
mod mail;
mod models;
mod nats;
mod report;
mod storage;
mod utils;

use cache::redis::RedisCache;
use config::Config;
use generated::api::v1::scheduler_service_server::SchedulerServiceServer;

const FILE_DESCRIPTOR_SET: &[u8] =
    include_bytes!(concat!(env!("OUT_DIR"), "/reflection_descriptor.bin"));
use grpc::{
    idgen_client::IdgenClient, mono_client::MonoCallbackClient,
    scheduler_server::SchedulerGrpcServer,
};
use jobs::process_request::ProcessReportRequestJob;
use mail::sender::MailSender;
use storage::s3::S3Storage;

/// Shared application context passed to all handlers and jobs.
#[derive(Clone)]
pub struct AppContext {
    pub config: Arc<Config>,
    pub auth_pool: PgPool,
    pub central_pool: PgPool,
    pub apalis_conn: ConnectionManager,
    pub s3: Arc<S3Storage>,
    pub mail: Arc<MailSender>,
    pub cache: Arc<RedisCache>,
    /// gRPC client for calling mono's MonoCallbackService
    pub mono_callback: Arc<MonoCallbackClient>,
    /// gRPC client for generating Snowflake IDs from idgen
    pub idgen: Arc<IdgenClient>,
    /// NATS JetStream context for publishing/consuming trade events
    pub jetstream: Arc<jetstream::Context>,
    // Per-tenant pool cache: tenant_id → PgPool
    tenant_pools: Arc<RwLock<HashMap<i64, PgPool>>>,
    // Per-MT5-service MySQL pool cache: service_id → MySqlPool
    mt5_pools: Arc<RwLock<HashMap<i32, MySqlPool>>>,
}

impl AppContext {
    pub async fn new(config: Config) -> Result<Self> {
        let auth_pool = db::pg_pool(&config.central_db_url()).await?;
        let central_pool = db::pg_pool(&config.central_db_url()).await?;
        let apalis_conn = redis_connect(config.redis_url.clone()).await?;
        let s3 = Arc::new(S3Storage::new(&config).await?);
        let mail = Arc::new(MailSender::new(&config).await?);
        let cache = Arc::new(RedisCache::new(&config).await?);
        let mono_callback = Arc::new(MonoCallbackClient::new(&config.mono_grpc_url));
        let idgen = Arc::new(IdgenClient::new(&config.idgen_grpc_url));

        let nats_client = async_nats::connect(&config.nats_url)
            .await
            .map_err(|e| anyhow::anyhow!("Failed to connect to NATS at {}: {}", config.nats_url, e))?;
        let jetstream_ctx = async_nats::jetstream::new(nats_client);
        nats::client::ensure_trade_stream(&jetstream_ctx).await?;
        nats::client::ensure_event_stream(&jetstream_ctx).await?;
        nats::client::ensure_event_consumer(&jetstream_ctx).await?;
        nats::client::ensure_send_message_stream(&jetstream_ctx).await?;
        nats::client::ensure_send_message_consumer(&jetstream_ctx).await?;
        let jetstream = Arc::new(jetstream_ctx);

        Ok(Self {
            config: Arc::new(config),
            auth_pool,
            central_pool,
            apalis_conn,
            s3,
            mail,
            cache,
            mono_callback,
            idgen,
            jetstream,
            tenant_pools: Arc::new(RwLock::new(HashMap::new())),
            mt5_pools: Arc::new(RwLock::new(HashMap::new())),
        })
    }

    /// Get or create a per-tenant PostgreSQL pool.
    /// Looks up the actual database name from core."_Tenant".DatabaseName.
    pub async fn tenant_pool(&self, tenant_id: i64) -> Result<PgPool> {
        {
            let pools = self.tenant_pools.read().await;
            if let Some(pool) = pools.get(&tenant_id) {
                return Ok(pool.clone());
            }
        }

        let db_name = db::tenant::get_tenant_db_name(&self.central_pool, tenant_id)
            .await?
            .ok_or_else(|| anyhow::anyhow!("Tenant {} not found in core._Tenant", tenant_id))?;
        let url = self.config.tenant_db_url_by_name(&db_name);
        let pool = db::tenant_pg_pool(&url).await?;

        {
            let mut pools = self.tenant_pools.write().await;
            pools.insert(tenant_id, pool.clone());
        }

        // Ensure PG native partition parent tables and pre-create current + next year sub-partitions.
        db::partition::ensure_parent_tables(&pool).await
            .map_err(|e| anyhow::anyhow!("Failed to ensure partition parent tables for tenant {}: {:#}", tenant_id, e))?;
        let current_year = {
            use chrono::Datelike;
            chrono::Utc::now().year()
        };
        for year in [current_year, current_year + 1] {
            db::partition::ensure_year_partition_snake(&pool, "core", "activity_k8s", year).await
                .map_err(|e| anyhow::anyhow!("Failed to ensure activity_k8s_{} for tenant {}: {:#}", year, tenant_id, e))?;
            db::partition::ensure_year_partition_snake(&pool, "acct", "wallet_transaction_k8s", year).await
                .map_err(|e| anyhow::anyhow!("Failed to ensure wallet_transaction_k8s_{} for tenant {}: {:#}", year, tenant_id, e))?;
            db::partition::ensure_year_partition_snake(&pool, "trd", "trade_rebate_k8s", year).await
                .map_err(|e| anyhow::anyhow!("Failed to ensure trade_rebate_k8s_{} for tenant {}: {:#}", year, tenant_id, e))?;
            db::partition::ensure_year_partition_snake(&pool, "trd", "rebate_k8s", year).await
                .map_err(|e| anyhow::anyhow!("Failed to ensure rebate_k8s_{} for tenant {}: {:#}", year, tenant_id, e))?;
            db::partition::ensure_year_partition_snake(&pool, "trd", "sales_rebate_k8s", year).await
                .map_err(|e| anyhow::anyhow!("Failed to ensure sales_rebate_k8s_{} for tenant {}: {:#}", year, tenant_id, e))?;
            db::partition::ensure_year_partition_snake(&pool, "trd", "sales_rebate_item_k8s", year).await
                .map_err(|e| anyhow::anyhow!("Failed to ensure sales_rebate_item_k8s_{} for tenant {}: {:#}", year, tenant_id, e))?;
        }

        Ok(pool)
    }

    /// Get or create a MySQL connection pool for an MT5 service.
    /// Connection string is loaded from trd."_TradeService"."Configuration" JSON in the tenant DB.
    pub async fn mt5_pool(&self, service_id: i32, tenant_pool: &PgPool) -> Result<MySqlPool> {
        {
            let pools = self.mt5_pools.read().await;
            if let Some(pool) = pools.get(&service_id) {
                return Ok(pool.clone());
            }
        }
        let conn_str = db::tenant::get_mt5_connection_string_from_central(tenant_pool, service_id)
            .await?
            .ok_or_else(|| anyhow::anyhow!("No MT5 connection string for service {}", service_id))?;
        let mysql_url = parse_cs_connection_string(&conn_str)?;
        let pool = sqlx::mysql::MySqlPoolOptions::new()
            .max_connections(5)
            .connect(&mysql_url)
            .await?;
        {
            let mut pools = self.mt5_pools.write().await;
            pools.entry(service_id).or_insert_with(|| pool.clone());
        }
        Ok(pool)
    }
}

/// Converts a C# style connection string to a MySQL URL.
/// Input:  "Server=host;Port=3306;Database=db;Uid=user;Pwd=pass;"
/// Output: "mysql://user:pass@host:3306/db"
fn parse_cs_connection_string(cs: &str) -> Result<String> {
    let mut map = std::collections::HashMap::new();
    for part in cs.split(';') {
        let part = part.trim();
        if let Some((k, v)) = part.split_once('=') {
            map.insert(k.trim().to_lowercase(), v.trim().to_string());
        }
    }
    let host = map
        .get("server")
        .or_else(|| map.get("host"))
        .ok_or_else(|| anyhow::anyhow!("Missing Server in connection string"))?;
    let port = map.get("port").map(|s| s.as_str()).unwrap_or("3306");
    let db = map
        .get("database")
        .ok_or_else(|| anyhow::anyhow!("Missing Database in connection string"))?;
    let user = map
        .get("uid")
        .or_else(|| map.get("user id"))
        .or_else(|| map.get("user"))
        .or_else(|| map.get("username"))
        .ok_or_else(|| anyhow::anyhow!("Missing Uid in connection string"))?;
    let pwd = map
        .get("pwd")
        .or_else(|| map.get("password"))
        .cloned()
        .unwrap_or_default();
    let user_enc = utf8_percent_encode(user, NON_ALPHANUMERIC).to_string();
    let pwd_enc = utf8_percent_encode(&pwd, NON_ALPHANUMERIC).to_string();
    Ok(format!("mysql://{}:{}@{}:{}/{}", user_enc, pwd_enc, host, port, db))
}

// ── HTTP health endpoint (axum) ───────────────────────────────────────────────

async fn health() -> Json<serde_json::Value> {
    Json(serde_json::json!({ "status": "ok", "service": "scheduler" }))
}

async fn trigger_sales_rebate_daily(
    Extension(ctx): Extension<AppContext>,
) -> (StatusCode, Json<serde_json::Value>) {
    tokio::spawn(async move {
        if let Err(e) = jobs::sales_rebate::run_daily(ctx).await {
            error!("trigger_sales_rebate_daily error: {:#}", e);
        }
    });
    (
        StatusCode::ACCEPTED,
        Json(serde_json::json!({ "status": "triggered", "job": "sales_rebate_daily" })),
    )
}

async fn trigger_sales_rebate_monthly(
    Extension(ctx): Extension<AppContext>,
) -> (StatusCode, Json<serde_json::Value>) {
    tokio::spawn(async move {
        if let Err(e) = jobs::sales_rebate::run_monthly(ctx).await {
            error!("trigger_sales_rebate_monthly error: {:#}", e);
        }
    });
    (
        StatusCode::ACCEPTED,
        Json(serde_json::json!({ "status": "triggered", "job": "sales_rebate_monthly" })),
    )
}

#[derive(serde::Deserialize)]
struct TriggerCustomBody {
    schedule_type: i16,
    period_start: String,
    period_end: String,
}

#[derive(serde::Deserialize)]
struct TriggerForceBody {
    schedule_type: i16,
    period_start: String,
    period_end: Option<String>, // defaults to Utc::now() when omitted
    sales_account_id: Option<i64>, // restrict recalc to a single sales account
}

async fn trigger_sales_rebate_custom(
    Extension(ctx): Extension<AppContext>,
    AxumJson(body): AxumJson<TriggerCustomBody>,
) -> (StatusCode, Json<serde_json::Value>) {
    let period_start = match DateTime::parse_from_rfc3339(&body.period_start) {
        Ok(dt) => dt.with_timezone(&chrono::Utc),
        Err(e) => {
            return (
                StatusCode::BAD_REQUEST,
                Json(serde_json::json!({ "error": format!("invalid period_start: {}", e) })),
            );
        }
    };
    let period_end = match DateTime::parse_from_rfc3339(&body.period_end) {
        Ok(dt) => dt.with_timezone(&chrono::Utc),
        Err(e) => {
            return (
                StatusCode::BAD_REQUEST,
                Json(serde_json::json!({ "error": format!("invalid period_end: {}", e) })),
            );
        }
    };
    tokio::spawn(async move {
        if let Err(e) =
            jobs::sales_rebate::run_custom(ctx, body.schedule_type, period_start, period_end).await
        {
            error!("trigger_sales_rebate_custom error: {:#}", e);
        }
    });
    (
        StatusCode::ACCEPTED,
        Json(serde_json::json!({
            "status": "triggered",
            "job": "sales_rebate_custom",
            "schedule_type": body.schedule_type,
            "period_start": body.period_start,
            "period_end": body.period_end,
        })),
    )
}

async fn trigger_sales_rebate_force(
    Extension(ctx): Extension<AppContext>,
    AxumJson(body): AxumJson<TriggerForceBody>,
) -> (StatusCode, Json<serde_json::Value>) {
    let period_start = match DateTime::parse_from_rfc3339(&body.period_start) {
        Ok(dt) => dt.with_timezone(&chrono::Utc),
        Err(e) => {
            return (
                StatusCode::BAD_REQUEST,
                Json(serde_json::json!({ "error": format!("invalid period_start: {}", e) })),
            );
        }
    };
    let period_end = match &body.period_end {
        Some(s) => match DateTime::parse_from_rfc3339(s) {
            Ok(dt) => dt.with_timezone(&chrono::Utc),
            Err(e) => {
                return (
                    StatusCode::BAD_REQUEST,
                    Json(serde_json::json!({ "error": format!("invalid period_end: {}", e) })),
                );
            }
        },
        None => chrono::Utc::now(),
    };
    let period_end_str = period_end.to_rfc3339();
    tokio::spawn(async move {
        if let Err(e) = jobs::sales_rebate::run_force(
            ctx,
            body.schedule_type,
            period_start,
            period_end,
            body.sales_account_id,
        )
        .await
        {
            error!("trigger_sales_rebate_force error: {:#}", e);
        }
    });
    (
        StatusCode::ACCEPTED,
        Json(serde_json::json!({
            "status": "triggered",
            "job": "sales_rebate_force",
            "schedule_type": body.schedule_type,
            "period_start": body.period_start,
            "period_end": period_end_str,
        })),
    )
}

// ── Main ─────────────────────────────────────────────────────────────────────

#[tokio::main]
async fn main() -> Result<()> {
    let _tracing_guard = otel::init_tracing("scheduler");

    let config = Config::from_env()?;
    let http_port = config.port;
    let grpc_port = config.grpc_port;
    let ctx = AppContext::new(config).await?;

    // ── Fail-fast: ensure partition tables exist for all tenants before serving ──
    info!("Startup: ensuring partition tables for all tenants...");
    jobs::partition_maintenance::execute(ctx.clone()).await
        .map_err(|e| anyhow::anyhow!("Startup partition check failed: {:#}", e))?;
    info!("Startup: partition tables OK");

    // ── Build apalis storage (for dashboard) and monitor (for worker lifecycle) ──
    let dashboard_storage =
        RedisStorage::<ProcessReportRequestJob>::new(ctx.apalis_conn.clone());

    let ctx_for_monitor = ctx.clone();

    // ── Build axum HTTP router (health + apalis dashboard) ────────────────
    let board_api = ApiBuilder::new(Router::new())
        .register(dashboard_storage)
        .build();

    let broadcaster = TracingBroadcaster::create();
    let http_app = Router::new()
        .route("/health", get(health))
        .route("/trigger/sales-rebate-daily", post(trigger_sales_rebate_daily))
        .route("/trigger/sales-rebate-monthly", post(trigger_sales_rebate_monthly))
        .route("/trigger/sales-rebate-custom", post(trigger_sales_rebate_custom))
        .route("/trigger/sales-rebate-force", post(trigger_sales_rebate_force))
        .nest("/api/v1", board_api)
        .fallback_service(ServeUI::new())
        .layer(axum::Extension(ctx.clone()))
        .layer(axum::Extension(broadcaster))
        .layer(TraceLayer::new_for_http());

    let http_addr = SocketAddr::from(([0, 0, 0, 0], http_port));
    info!("Scheduler HTTP listening on {}", http_addr);

    // ── Build tonic gRPC server ───────────────────────────────────────────
    let grpc_addr = SocketAddr::from(([0, 0, 0, 0], grpc_port));
    info!("Scheduler gRPC listening on {}", grpc_addr);

    let grpc_server = SchedulerServiceServer::new(SchedulerGrpcServer { ctx: ctx.clone() });

    // ── Spawn cron scheduler ──────────────────────────────────────────────
    let ctx_cron = ctx.clone();
    tokio::spawn(async move {
        if let Err(e) = start_cron_scheduler(ctx_cron).await {
            error!("CronScheduler failed to start: {:#}", e);
        }
    });

    // ── Spawn apalis monitor (manages worker lifecycle + heartbeat) ───────
    // Auto-restart on transient errors (e.g. Redis TCP timeout due to AWS idle connection drop).
    // Each restart gets a new UUID worker ID to avoid "still active within threshold" errors.
    tokio::spawn(async move {
        loop {
            let ctx_for_worker = ctx_for_monitor.clone();
            let worker_id = format!("process-report-request-{}", uuid::Uuid::new_v4());
            let monitor = Monitor::new()
                .register(move |_index| {
                    let storage = RedisStorage::<ProcessReportRequestJob>::new(
                        ctx_for_worker.apalis_conn.clone(),
                    );
                    let ctx = ctx_for_worker.clone();
                    WorkerBuilder::new(&worker_id)
                        .backend(storage)
                        .build(move |job: ProcessReportRequestJob, _: WorkerContext| {
                            let ctx = ctx.clone();
                            async move { jobs::process_request::execute(ctx, job).await }
                        })
                });
            if let Err(e) = monitor.run().await {
                error!("Monitor failed: {:#}. Restarting in 5s...", e);
                tokio::time::sleep(Duration::from_secs(5)).await;
            }
        }
    });

    // ── Spawn TradeMonitorService (MT5 poll → NATS publish) ──────────────
    let ctx_tm = ctx.clone();
    tokio::spawn(async move {
        loop {
            match jobs::trade_monitor::run(ctx_tm.clone()).await {
                Err(e) => {
                    error!("TradeMonitorService error: {:#}. Restarting in 5s...", e);
                }
                Ok(()) => {
                    // run() returned Ok but exited (e.g. no MT5 services configured yet)
                    // retry after a longer delay to avoid busy-looping
                    tracing::warn!("TradeMonitorService exited cleanly. Retrying in 60s...");
                }
            }
            tokio::time::sleep(Duration::from_secs(60)).await;
        }
    });

    // ── Spawn TradeHandler (NATS consume → DB write) ──────────────────────
    let ctx_th = ctx.clone();
    tokio::spawn(async move {
        loop {
            if let Err(e) = jobs::trade_handler::run(ctx_th.clone()).await {
                error!("TradeHandler error: {:#}. Restarting in 5s...", e);
                tokio::time::sleep(Duration::from_secs(5)).await;
            }
        }
    });

    // ── Spawn EventTradeHandler (BCR_EVENT_TRADE → point calculation) ─────
    let ctx_eth = ctx.clone();
    tokio::spawn(async move {
        loop {
            if let Err(e) = jobs::event_trade_handler::run(ctx_eth.clone()).await {
                error!("EventTradeHandler error: {:#}. Restarting in 5s...", e);
                tokio::time::sleep(Duration::from_secs(5)).await;
            }
        }
    });

    // ── Spawn SendMessageHandler (BCR_SEND_MESSAGE → batch email via SES) ─
    let ctx_smh = ctx.clone();
    tokio::spawn(async move {
        loop {
            if let Err(e) = jobs::send_message_handler::run(ctx_smh.clone()).await {
                error!("SendMessageHandler error: {:#}. Restarting in 5s...", e);
                tokio::time::sleep(Duration::from_secs(5)).await;
            }
        }
    });

    // ── Spawn HTTP server ─────────────────────────────────────────────────
    tokio::spawn(async move {
        let listener = tokio::net::TcpListener::bind(http_addr).await.unwrap();
        axum::serve(listener, http_app).await.unwrap();
    });

    // ── Start gRPC server (blocks) ────────────────────────────────────────
    let is_dev = std::env::var("ASPNETCORE_ENVIRONMENT").as_deref() == Ok("Development");
    let mut grpc_builder = TonicServer::builder().add_service(grpc_server);
    if is_dev {
        let reflection = tonic_reflection::server::Builder::configure()
            .register_encoded_file_descriptor_set(FILE_DESCRIPTOR_SET)
            .build()
            .expect("Failed to build gRPC reflection service");
        grpc_builder = grpc_builder.add_service(reflection);
        info!("gRPC reflection enabled (Development)");
    }
    grpc_builder.serve(grpc_addr).await?;

    Ok(())
}

async fn start_cron_scheduler(ctx: AppContext) -> Result<()> {
    let sched = JobScheduler::new().await?;

    // Close Trade: 22:30 UTC daily
    let c = ctx.clone();
    sched.add(Job::new_async("0 30 22 * * *", move |_, _| {
        let ctx = c.clone();
        Box::pin(async move {
            if let Err(e) = jobs::close_trade::execute(ctx).await {
                error!("CloseTradeJob error: {:#}", e);
            }
        })
    })?).await?;

    // Account Daily Confirmation: Mon-Fri, 21:29 (DST) or 22:29 (non-DST)
    // Schedule both times; handler checks which one is correct for the current DST state.
    for trigger_hour in [21u32, 22u32] {
        let c = ctx.clone();
        let expr = format!("0 29 {} * * Mon,Tue,Wed,Thu,Fri", trigger_hour);
        sched.add(Job::new_async(expr.as_str(), move |_, _| {
            let ctx = c.clone();
            Box::pin(async move {
                let now = Utc::now();
                let expected = if utils::is_dst_los_angeles(now) { 21 } else { 22 };
                if now.hour() == expected {
                    if let Err(e) = jobs::account_daily::execute(ctx).await {
                        error!("AccountDailyJob error: {:#}", e);
                    }
                }
            })
        })?).await?;
    }

    // Crypto Monitor: every minute
    let c = ctx.clone();
    sched.add(Job::new_async("0 * * * * *", move |_, _| {
        let ctx = c.clone();
        Box::pin(async move {
            if let Err(e) = jobs::crypto::execute(ctx).await {
                error!("CryptoMonitorJob error: {:#}", e);
            }
        })
    })?).await?;

    // Partition Maintenance: 1st of month at 02:00 UTC
    let c = ctx.clone();
    sched.add(Job::new_async("0 0 2 1 * *", move |_, _| {
        let ctx = c.clone();
        Box::pin(async move {
            if let Err(e) = jobs::partition_maintenance::execute(ctx).await {
                error!("PartitionMaintenanceJob error: {:#}", e);
            }
        })
    })?).await?;

    // Daily SalesRebate: 03:00 UTC (accounts for ~30min trade_rebate_k8s write lag)
    let c = ctx.clone();
    sched.add(Job::new_async("0 0 3 * * *", move |_, _| {
        let ctx = c.clone();
        Box::pin(async move {
            if let Err(e) = jobs::sales_rebate::run_daily(ctx).await {
                error!("SalesRebateDaily error: {:#}", e);
            }
        })
    })?).await?;

    // Monthly SalesRebate: 1st of month 03:00 UTC
    let c = ctx.clone();
    sched.add(Job::new_async("0 0 3 1 * *", move |_, _| {
        let ctx = c.clone();
        Box::pin(async move {
            if let Err(e) = jobs::sales_rebate::run_monthly(ctx).await {
                error!("SalesRebateMonthly error: {:#}", e);
            }
        })
    })?).await?;

    // Calculate Rebate: every 2 minutes
    let c = ctx.clone();
    sched.add(Job::new_async("0 0/2 * * * *", move |_, _| {
        let ctx = c.clone();
        Box::pin(async move {
            if let Err(e) = jobs::rebate::execute_calculate(ctx).await {
                error!("CalculateRebateJob error: {:#}", e);
            }
        })
    })?).await?;

    // Release Rebate: every 2 minutes
    let c = ctx.clone();
    sched.add(Job::new_async("0 0/2 * * * *", move |_, _| {
        let ctx = c.clone();
        Box::pin(async move {
            if let Err(e) = jobs::rebate::execute_release(ctx).await {
                error!("ReleaseRebateJob error: {:#}", e);
            }
        })
    })?).await?;

    info!("CronScheduler started (8 tasks, 9 cron entries — AccountDaily registered twice for DST)");
    sched.start().await?;
    Ok(())
}

