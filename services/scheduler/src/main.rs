use std::collections::HashMap;
use std::net::SocketAddr;
use std::sync::Arc;
use std::time::Duration;

use anyhow::Result;
use apalis::prelude::*;
use apalis_board_api::framework::{ApiBuilder, RegisterRoute};
use apalis_board_api::ui::ServeUI;
use apalis_postgres::PostgresStorage;
use axum::response::Json;
use axum::{routing::get, Router};
use sqlx::PgPool;
use tokio::sync::RwLock;
use tonic::transport::Server as TonicServer;
use tower_http::trace::TraceLayer;
use tracing::{error, info};
use tracing_subscriber::{layer::SubscriberExt, util::SubscriberInitExt};

mod cache;
mod config;
mod db;
mod generated;
mod grpc;
mod jobs;
mod mail;
mod report;
mod storage;

use cache::redis::RedisCache;
use config::Config;
use generated::api::v1::scheduler_service_server::SchedulerServiceServer;
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
    pub apalis_pool: PgPool,
    pub s3: Arc<S3Storage>,
    pub mail: Arc<MailSender>,
    pub cache: Arc<RedisCache>,
    /// gRPC client for calling mono's MonoCallbackService
    pub mono_callback: Arc<MonoCallbackClient>,
    // Per-tenant pool cache: tenant_id → PgPool
    tenant_pools: Arc<RwLock<HashMap<i64, PgPool>>>,
}

impl AppContext {
    pub async fn new(config: Config) -> Result<Self> {
        let auth_pool = db::pg_pool(&config.central_db_url()).await?;
        let central_pool = db::pg_pool(&config.central_db_url()).await?;
        let apalis_pool = db::pg_pool(&config.hangfire_db_url()).await?;
        let s3 = Arc::new(S3Storage::new(&config).await?);
        let mail = Arc::new(MailSender::new(&config)?);
        let cache = Arc::new(RedisCache::new(&config).await?);
        let mono_callback = Arc::new(MonoCallbackClient::new(&config.mono_grpc_url));

        Ok(Self {
            config: Arc::new(config),
            auth_pool,
            central_pool,
            apalis_pool,
            s3,
            mail,
            cache,
            mono_callback,
            tenant_pools: Arc::new(RwLock::new(HashMap::new())),
        })
    }

    /// Get or create a per-tenant PostgreSQL pool.
    pub async fn tenant_pool(&self, tenant_id: i64) -> Result<PgPool> {
        {
            let pools = self.tenant_pools.read().await;
            if let Some(pool) = pools.get(&tenant_id) {
                return Ok(pool.clone());
            }
        }

        let url = self.config.tenant_db_url(tenant_id);
        let pool = db::tenant_pg_pool(&url).await?;

        {
            let mut pools = self.tenant_pools.write().await;
            pools.insert(tenant_id, pool.clone());
        }

        Ok(pool)
    }
}

// ── HTTP health endpoint (axum) ───────────────────────────────────────────────

async fn health() -> Json<serde_json::Value> {
    Json(serde_json::json!({ "status": "ok", "service": "scheduler" }))
}

// ── Main ─────────────────────────────────────────────────────────────────────

#[tokio::main]
async fn main() -> Result<()> {
    tracing_subscriber::registry()
        .with(tracing_subscriber::EnvFilter::new(
            std::env::var("RUST_LOG").unwrap_or_else(|_| "info".to_string()),
        ))
        .with(tracing_subscriber::fmt::layer())
        .init();

    let config = Config::from_env()?;
    let http_port = config.port;
    let grpc_port = config.grpc_port;
    let ctx = AppContext::new(config).await?;

    // ── Setup apalis PostgreSQL storage (run migrations) ──────────────────
    PostgresStorage::<(), (), ()>::setup(&ctx.apalis_pool)
        .await
        .expect("Failed to setup apalis PostgreSQL storage");

    // ── Build apalis worker for ProcessReportRequest ──────────────────────
    let process_request_storage = PostgresStorage::<ProcessReportRequestJob>::new(&ctx.apalis_pool);
    let dashboard_storage = process_request_storage.clone();

    let ctx_for_worker = ctx.clone();
    let worker = WorkerBuilder::new("process-report-request")
        .backend(process_request_storage)
        .build(move |job: ProcessReportRequestJob, _: WorkerContext| {
            let ctx = ctx_for_worker.clone();
            async move { jobs::process_request::execute(ctx, job).await }
        });

    // ── Build axum HTTP router (health + apalis dashboard) ────────────────
    let board_api = ApiBuilder::new(Router::new())
        .register(dashboard_storage)
        .build();

    let http_app = Router::new()
        .route("/health", get(health))
        .nest("/board/api/v1", board_api)
        .fallback_service(ServeUI::new())
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

    // ── Spawn apalis worker ───────────────────────────────────────────────
    tokio::spawn(async move {
        worker.run().await.expect("Worker failed");
    });

    // ── Spawn HTTP server ─────────────────────────────────────────────────
    tokio::spawn(async move {
        let listener = tokio::net::TcpListener::bind(http_addr).await.unwrap();
        axum::serve(listener, http_app).await.unwrap();
    });

    // ── Start gRPC server (blocks) ────────────────────────────────────────
    TonicServer::builder()
        .add_service(grpc_server)
        .serve(grpc_addr)
        .await?;

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
        let dst = is_dst_now();
        let target_hour = if dst { 21 } else { 22 };
        if hour == target_hour && minute == 29 && weekday < 5 {
            let ctx = ctx.clone();
            tokio::spawn(async move {
                if let Err(e) = jobs::account_daily::execute(ctx).await {
                    error!("Scheduled AccountDailyJob error: {:#}", e);
                }
            });
        }
    }
}

fn is_dst_now() -> bool {
    let month = chrono::Utc::now().month();
    (3..=11).contains(&month)
}

use chrono::Datelike;
