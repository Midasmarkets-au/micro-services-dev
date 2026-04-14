use crate::grpc::auth_client::MonoAuthClient;
use crate::keys::RsaKeyPair;
use sqlx::PgPool;
use tokio::sync::Mutex;

/// Twilio Verify configuration. Present only when SMS OTP is enabled.
pub struct TwilioConfig {
    pub account_sid: String,
    pub auth_token: String,
    pub service_sid: String,
}

pub struct AppState {
    pub pool: PgPool,
    pub redis: deadpool_redis::Pool,
    pub key_pair: RsaKeyPair,
    pub access_token_lifetime: i64,
    /// Whether to set Secure flag on cookies (true in production/HTTPS)
    pub secure_cookie: bool,
    /// mono gRPC client for callbacks (2FA, login log, etc.)
    pub mono_client: Option<Mutex<MonoAuthClient>>,
    /// IPInfo API base URL (default: https://ipinfo.io)
    pub ipinfo_endpoint: String,
    /// IPInfo API token (empty = no token)
    pub ipinfo_token: String,
    /// Twilio Verify config (None = SMS OTP disabled)
    pub twilio: Option<TwilioConfig>,
}
