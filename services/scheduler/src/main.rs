use std::collections::HashMap;
use std::net::SocketAddr;
use std::sync::Arc;
use std::time::Duration;

use chrono::Datelike;

use percent_encoding::{utf8_percent_encode, NON_ALPHANUMERIC};

use anyhow::Result;
use apalis::prelude::*;
use apalis_board_api::framework::{ApiBuilder, RegisterRoute};
use apalis_board_api::sse::TracingBroadcaster;
use apalis_board_api::ui::ServeUI;
use apalis_redis::{connect as redis_connect, ConnectionManager, RedisStorage};
use async_nats::jetstream;
use axum::response::Json;
use axum::{routing::get, Router};
use sqlx::{MySqlPool, PgPool};
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
use grpc::{mono_client::MonoCallbackClient, scheduler_server::SchedulerGrpcServer};
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
    /// NATS JetStream context for publishing/consuming trade events
    pub jetstream: Arc<jetstream::Context>,
    // Per-tenant pool cache: tenant_id → PgPool
    tenant_pools: Arc<RwLock<HashMap<i64, PgPool>>>,
    // Per-MT5-service MySQL pool cache: service_id → MySqlPool
    mt5_pools: Arc<RwLock<HashMap<i32, MySqlPool>>>,
    // Tracks which (tenant_id, year) rebate tables have already been ensured this process lifetime
    ensured_rebate_tables: Arc<RwLock<std::collections::HashSet<(i64, i32)>>>,
}

impl AppContext {
    pub async fn new(config: Config) -> Result<Self> {
        let auth_pool = db::pg_pool(&config.central_db_url()).await?;
        let central_pool = db::pg_pool(&config.central_db_url()).await?;
        let apalis_conn = redis_connect(config.redis_url.clone()).await?;
        let s3 = Arc::new(S3Storage::new(&config).await?);
        let mail = Arc::new(MailSender::new(&config)?);
        let cache = Arc::new(RedisCache::new(&config).await?);
        let mono_callback = Arc::new(MonoCallbackClient::new(&config.mono_grpc_url));

        let nats_client = async_nats::connect(&config.nats_url)
            .await
            .map_err(|e| anyhow::anyhow!("Failed to connect to NATS at {}: {}", config.nats_url, e))?;
        let jetstream_ctx = async_nats::jetstream::new(nats_client);
        nats::client::ensure_trade_stream(&jetstream_ctx).await?;
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
            jetstream,
            tenant_pools: Arc::new(RwLock::new(HashMap::new())),
            mt5_pools: Arc::new(RwLock::new(HashMap::new())),
            ensured_rebate_tables: Arc::new(RwLock::new(std::collections::HashSet::new())),
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

        // Ensure current year and next year's tables exist on first connection.
        let current_year = chrono::Utc::now().year();
        self.ensure_rebate_table(tenant_id, &pool, current_year).await?;
        self.ensure_rebate_table(tenant_id, &pool, current_year + 1).await?;
        // Also ensure _Matter_{year} and _Rebate_{year} partitioned tables.
        db::rebate_calc::ensure_rebate_tables(&pool, current_year).await
            .map_err(|e| anyhow::anyhow!("Failed to ensure rebate tables for year {} tenant {}: {:#}", current_year, tenant_id, e))?;
        db::rebate_calc::ensure_rebate_tables(&pool, current_year + 1).await
            .map_err(|e| anyhow::anyhow!("Failed to ensure rebate tables for year {} tenant {}: {:#}", current_year + 1, tenant_id, e))?;

        Ok(pool)
    }

    /// Ensure trd._TradeRebate_{year} exists for the given tenant.
    /// Uses an in-memory set to avoid redundant DDL calls within the same process lifetime.
    /// Returns Err if table creation fails — callers should propagate the error so NATS retries.
    pub async fn ensure_rebate_table(&self, tenant_id: i64, pool: &PgPool, year: i32) -> Result<()> {
        {
            let seen = self.ensured_rebate_tables.read().await;
            if seen.contains(&(tenant_id, year)) {
                return Ok(());
            }
        }
        db::trade_rebate::ensure_table(pool, year).await
            .map_err(|e| anyhow::anyhow!("Failed to ensure trd._TradeRebate_{} for tenant {}: {:#}", year, tenant_id, e))?;
        let mut seen = self.ensured_rebate_tables.write().await;
        seen.insert((tenant_id, year));
        Ok(())
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

// ── Main ─────────────────────────────────────────────────────────────────────

#[tokio::main]
async fn main() -> Result<()> {
    let _tracing_guard = otel::init_tracing("scheduler");

    let config = Config::from_env()?;
    let http_port = config.port;
    let grpc_port = config.grpc_port;
    let ctx = AppContext::new(config).await?;

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
        .nest("/api/v1", board_api)
        .fallback_service(ServeUI::new())
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
        run_cron_scheduler(ctx_cron).await;
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

async fn run_cron_scheduler(ctx: AppContext) {
    use chrono::{Datelike, Timelike, Utc};

    loop {
        tokio::time::sleep(Duration::from_secs(60)).await;

        let now = Utc::now();
        let hour = now.hour();
        let minute = now.minute();
        let weekday = now.weekday().num_days_from_monday();

        // Close Trade Job: 22:30 UTC daily
        if hour == 22 && minute == 30 {
            let ctx = ctx.clone();
            tokio::spawn(async move {
                if let Err(e) = jobs::close_trade::execute(ctx).await {
                    error!("Scheduled CloseTradeJob error: {:#}", e);
                }
            });
        }

        // Account Daily Confirmation: 21:29 UTC Mon-Fri (DST) or 22:29 (non-DST)
        let dst = utils::is_dst_los_angeles(now);
        let target_hour = if dst { 21 } else { 22 };
        if hour == target_hour && minute == 29 && weekday < 5 {
            let ctx = ctx.clone();
            tokio::spawn(async move {
                if let Err(e) = jobs::account_daily::execute(ctx).await {
                    error!("Scheduled AccountDailyJob error: {:#}", e);
                }
            });
        }

        // Crypto Monitor: every minute (*/1 * * * *)
        {
            let ctx = ctx.clone();
            tokio::spawn(async move {
                if let Err(e) = jobs::crypto::execute(ctx).await {
                    error!("Scheduled CryptoMonitorJob error: {:#}", e);
                }
            });
        }

        // Calculate & Release Rebate: every 2 minutes (*/2 * * * *)
        if minute % 2 == 0 {
            let ctx_calc = ctx.clone();
            tokio::spawn(async move {
                if let Err(e) = jobs::rebate::execute_calculate(ctx_calc).await {
                    error!("Scheduled CalculateRebateJob error: {:#}", e);
                }
            });
            let ctx_release = ctx.clone();
            tokio::spawn(async move {
                if let Err(e) = jobs::rebate::execute_release(ctx_release).await {
                    error!("Scheduled ReleaseRebateJob error: {:#}", e);
                }
            });
        }
    }
}

