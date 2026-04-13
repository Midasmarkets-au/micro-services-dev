use crate::grpc::auth_client::MonoAuthClient;
use crate::keys::RsaKeyPair;
use sqlx::PgPool;
use tokio::sync::Mutex;

pub struct AppState {
    pub pool: PgPool,
    pub redis: deadpool_redis::Pool,
    pub key_pair: RsaKeyPair,
    pub access_token_lifetime: i64,
    /// Whether to set Secure flag on cookies (true in production/HTTPS)
    pub secure_cookie: bool,
    /// mono gRPC client for callbacks (2FA, login log, etc.)
    pub mono_client: Option<Mutex<MonoAuthClient>>,
}
