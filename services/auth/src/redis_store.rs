use deadpool_redis::{Config, Pool, Runtime, redis::AsyncCommands};
use serde::{Deserialize, Serialize};
use tracing::error;

#[derive(Debug, Serialize, Deserialize)]
pub struct RefreshTokenData {
    pub user_id: i64,
    pub tenant_id: i64,
    pub party_id: String,
}

const REFRESH_TTL_SECS: u64 = 30 * 24 * 3600; // 30 days

pub fn create_pool(redis_url: &str) -> Result<Pool, deadpool_redis::CreatePoolError> {
    let cfg = Config::from_url(redis_url);
    cfg.create_pool(Some(Runtime::Tokio1))
}

/// Store a refresh token in Redis. Key: `auth:refresh:{token}`
pub async fn store_refresh_token(
    pool: &Pool,
    token: &str,
    data: &RefreshTokenData,
) -> Result<(), String> {
    let mut conn = pool.get().await.map_err(|e| e.to_string())?;
    let key = format!("auth:refresh:{}", token);
    let value = serde_json::to_string(data).map_err(|e| e.to_string())?;
    conn.set_ex::<_, _, ()>(&key, &value, REFRESH_TTL_SECS)
        .await
        .map_err(|e| e.to_string())
}

/// Retrieve and consume a refresh token (single-use).
pub async fn consume_refresh_token(
    pool: &Pool,
    token: &str,
) -> Result<Option<RefreshTokenData>, String> {
    let mut conn = pool.get().await.map_err(|e| e.to_string())?;
    let key = format!("auth:refresh:{}", token);
    let value: Option<String> = conn.get_del(&key).await.map_err(|e| e.to_string())?;
    match value {
        None => Ok(None),
        Some(v) => {
            let data: RefreshTokenData =
                serde_json::from_str(&v).map_err(|e| e.to_string())?;
            Ok(Some(data))
        }
    }
}

/// Delete a refresh token (logout).
pub async fn delete_refresh_token(pool: &Pool, token: &str) {
    let Ok(mut conn) = pool.get().await else { return };
    let key = format!("auth:refresh:{}", token);
    let _: Result<(), _> = conn.del(&key).await;
}

const PWD_RESET_TTL_SECS: u64 = 3600; // 1 hour

/// Store a password reset token. Key: `auth:pwd_reset:{token}` → email
pub async fn store_password_reset_token(
    pool: &Pool,
    token: &str,
    email: &str,
) -> Result<(), String> {
    let mut conn = pool.get().await.map_err(|e| e.to_string())?;
    let key = format!("auth:pwd_reset:{}", token);
    conn.set_ex::<_, _, ()>(&key, email, PWD_RESET_TTL_SECS)
        .await
        .map_err(|e| e.to_string())
}

/// Consume a password reset token (single-use). Returns the associated email.
pub async fn consume_password_reset_token(
    pool: &Pool,
    token: &str,
) -> Result<Option<String>, String> {
    let mut conn = pool.get().await.map_err(|e| e.to_string())?;
    let key = format!("auth:pwd_reset:{}", token);
    let value: Option<String> = conn.get_del(&key).await.map_err(|e| e.to_string())?;
    Ok(value)
}

/// Consume a god-mode one-time key written by mono (key: `godmode:key:{uuid}`).
/// Returns the stored access token and deletes the key atomically (single-use).
pub async fn consume_godmode_key(pool: &Pool, key: &str) -> Result<Option<String>, String> {
    let mut conn = pool.get().await.map_err(|e| e.to_string())?;
    let redis_key = format!("godmode:key:{}", key);
    let value: Option<String> = conn.get_del(&redis_key).await.map_err(|e| e.to_string())?;
    Ok(value)
}

/// Log a Redis error without propagating (fire-and-forget pattern).
pub fn log_redis_error(op: &str, err: impl std::fmt::Display) {
    error!("Redis {} failed: {}", op, err);
}
