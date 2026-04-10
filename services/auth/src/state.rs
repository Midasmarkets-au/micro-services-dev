use crate::keys::RsaKeyPair;
use sqlx::PgPool;

pub struct AppState {
    pub pool: PgPool,
    pub redis: deadpool_redis::Pool,
    pub key_pair: RsaKeyPair,
    pub access_token_lifetime: i64,
    /// Whether to set Secure flag on cookies (true in production/HTTPS)
    pub secure_cookie: bool,
    /// mono gRPC address for callbacks (2FA, login log, etc.)
    pub mono_grpc_addr: Option<String>,
}
