use std::env;

#[derive(Clone, Debug)]
pub struct Config {
    pub port: u16,
    pub grpc_port: u16,

    // Shared PostgreSQL credentials (DB_HOST / DB_PORT / DB_USERNAME / DB_PASSWORD)
    pub db_host: String,
    pub db_port: u16,
    pub db_user: String,
    pub db_password: String,

    // Database names
    /// Central DB (portal_central) — used for AuthDb and tenant list
    pub central_db_name: String,
    /// Prefix for per-tenant DBs, e.g. "portal_tenant_" → "portal_tenant_10000"
    pub tenant_db_name_prefix: String,

    // Redis  (REDIS_CONNECTION = host:port, REDIS_PASSWORD)
    pub redis_url: String,

    // S3  (AWS_S3_*)
    pub s3_bucket: String,
    pub s3_region: String,
    pub s3_endpoint: Option<String>,
    pub s3_access_key: String,
    pub s3_secret_key: String,

    // Email (SMTP)
    pub smtp_host: String,
    pub smtp_port: u16,
    pub smtp_user: String,
    pub smtp_password: String,
    pub smtp_from: String,

    // mono gRPC URL (for NotifyReportDone callback)
    pub mono_grpc_url: String,

    // Worker settings
    pub worker_concurrency: usize,
}

impl Config {
    pub fn from_env() -> anyhow::Result<Self> {
        dotenvy::dotenv().ok();

        let db_host = env_str("DB_HOST", "localhost");
        let db_port = env_u16("DB_PORT", 5432);
        let db_user = env_str("DB_USERNAME", "postgres");
        let db_password = env_str("DB_PASSWORD", "");

        // Build Redis URL from REDIS_CONNECTION (host:port) + optional REDIS_PASSWORD
        let redis_url = build_redis_url();

        Ok(Self {
            port: env_u16("SCHEDULER_PORT", 8090),
            grpc_port: env_u16("SCHEDULER_GRPC_PORT", 50053),

            db_host,
            db_port,
            db_user,
            db_password,

            central_db_name: env_str("DB_DATABASE", "portal_central"),
            tenant_db_name_prefix: env_str("TENANT_DB_NAME_PREFIX", "portal_tenant_"),

            redis_url,

            s3_bucket: env_str("AWS_S3_BUCKET", "reports"),
            s3_region: env_str("AWS_S3_REGION", "ap-southeast-2"),
            s3_endpoint: env::var("S3_ENDPOINT").ok(),
            s3_access_key: env_str("AWS_S3_ACCESS_KEY", ""),
            s3_secret_key: env_str("AWS_S3_ACCESS_SECRET", ""),

            smtp_host: env_str("SMTP_HOST", "localhost"),
            smtp_port: env_u16("SMTP_PORT", 587),
            smtp_user: env_str("SMTP_USER", ""),
            smtp_password: env_str("SMTP_PASSWORD", ""),
            smtp_from: env_str("SMTP_FROM", "noreply@example.com"),

            mono_grpc_url: env_str("MONO_GRPC_URL", "http://mono:9000"),

            worker_concurrency: env_usize("WORKER_CONCURRENCY", 4),
        })
    }

    /// PostgreSQL URL for the central DB (AuthDb + tenant list).
    pub fn central_db_url(&self) -> String {
        self.build_pg_url(&self.central_db_name)
    }

    /// PostgreSQL URL for a per-tenant DB using the actual database name from core._Tenant.
    pub fn tenant_db_url_by_name(&self, db_name: &str) -> String {
        self.build_pg_url(db_name)
    }

    fn build_pg_url(&self, db: &str) -> String {
        format!(
            "postgresql://{}:{}@{}:{}/{}",
            self.db_user, self.db_password, self.db_host, self.db_port, db
        )
    }
}

/// Build a Redis URL from REDIS_CONNECTION (host:port) and optional REDIS_PASSWORD.
fn build_redis_url() -> String {
    // Allow a fully-formed REDIS_URL override first
    if let Ok(url) = env::var("REDIS_URL") {
        return url;
    }

    let conn = env_str("REDIS_CONNECTION", "redis:6379");
    let password = env::var("REDIS_PASSWORD").ok();

    match password {
        Some(pw) if !pw.is_empty() => format!("redis://:{}@{}", pw, conn),
        _ => format!("redis://{}", conn),
    }
}

fn env_str(key: &str, default: &str) -> String {
    env::var(key).unwrap_or_else(|_| default.to_string())
}

fn env_u16(key: &str, default: u16) -> u16 {
    env::var(key)
        .ok()
        .and_then(|v| v.parse().ok())
        .unwrap_or(default)
}

fn env_usize(key: &str, default: usize) -> usize {
    env::var(key)
        .ok()
        .and_then(|v| v.parse().ok())
        .unwrap_or(default)
}
