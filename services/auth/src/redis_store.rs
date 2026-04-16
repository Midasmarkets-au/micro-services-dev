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

/// Add a jti to the blocklist (logout). TTL = remaining token lifetime.
/// Key: `auth:blocklist:jti:{jti}`
pub async fn blocklist_jti(pool: &Pool, jti: &str, ttl_secs: u64) -> Result<(), String> {
    if ttl_secs == 0 {
        return Ok(());
    }
    let mut conn = pool.get().await.map_err(|e| e.to_string())?;
    let key = format!("auth:blocklist:jti:{}", jti);
    conn.set_ex::<_, _, ()>(&key, 1u8, ttl_secs)
        .await
        .map_err(|e| e.to_string())
}

/// Check whether a jti is in the blocklist.
pub async fn is_jti_blocked(pool: &Pool, jti: &str) -> bool {
    let Ok(mut conn) = pool.get().await else { return false };
    let key = format!("auth:blocklist:jti:{}", jti);
    conn.exists::<_, bool>(&key).await.unwrap_or(false)
}

const REVOKE_TTL_SECS: u64 = 24 * 3600; // 1 day — matches max access token lifetime

/// Record a party-level revocation timestamp (ban/lockout).
/// Key: `auth:revoked:party:{party_id}` → Unix timestamp of revocation
pub async fn revoke_party(pool: &Pool, party_id: i64) -> Result<(), String> {
    let mut conn = pool.get().await.map_err(|e| e.to_string())?;
    let key = format!("auth:revoked:party:{}", party_id);
    let now = chrono::Utc::now().timestamp();
    conn.set_ex::<_, _, ()>(&key, now, REVOKE_TTL_SECS)
        .await
        .map_err(|e| e.to_string())
}

/// Get the revocation timestamp for a party, if any.
pub async fn get_party_revoke_ts(pool: &Pool, party_id: i64) -> Option<i64> {
    let Ok(mut conn) = pool.get().await else { return None };
    let key = format!("auth:revoked:party:{}", party_id);
    conn.get::<_, Option<i64>>(&key).await.unwrap_or(None)
}

/// Log a Redis error without propagating (fire-and-forget pattern).
pub fn log_redis_error(op: &str, err: impl std::fmt::Display) {
    error!("Redis {} failed: {}", op, err);
}
