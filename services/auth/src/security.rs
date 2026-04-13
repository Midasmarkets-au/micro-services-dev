use deadpool_redis::{Pool, redis::AsyncCommands};
use tracing::warn;

/// MD5 hex digest — mirrors mono's `Utils.Md5Hash`.
pub fn md5_hash(input: &str) -> String {
    format!("{:x}", md5::compute(input.as_bytes()))
}

/// Redis-based login lockout mirroring mono's LoginSecurityService.
///
/// Rules:
///   - Sliding 15-minute window: key `auth:login_attempts:{email}` with TTL 900s
///   - After 5 failures → lock key `auth:login_locked:{email}` for 1800s (30 min)
const WINDOW_SECS: u64 = 900;   // 15 minutes
const LOCK_SECS: u64 = 1800;    // 30 minutes
const MAX_ATTEMPTS: i64 = 5;

/// Returns true if the email is currently locked out.
pub async fn is_locked_out(pool: &Pool, email: &str) -> bool {
    let Ok(mut conn) = pool.get().await else { return false };
    let key = format!("auth:login_locked:{}", email);
    let exists: bool = conn.exists(&key).await.unwrap_or(false);
    exists
}

/// Record a failed login attempt. Returns true if the account is now locked.
pub async fn record_failure(pool: &Pool, email: &str) -> bool {
    let Ok(mut conn) = pool.get().await else { return false };
    let attempts_key = format!("auth:login_attempts:{}", email);

    // Increment attempt counter with sliding window
    let count: i64 = conn.incr(&attempts_key, 1).await.unwrap_or(1);
    // Reset TTL on every increment to keep the window sliding
    let _: Result<(), _> = conn.expire(&attempts_key, WINDOW_SECS as i64).await;

    if count >= MAX_ATTEMPTS {
        warn!("Login lockout triggered for {}", email);
        let lock_key = format!("auth:login_locked:{}", email);
        let _: Result<(), _> = conn.set_ex(&lock_key, 1u8, LOCK_SECS).await;
        // Clear attempt counter so the window resets after unlock
        let _: Result<(), _> = conn.del(&attempts_key).await;
        return true;
    }
    false
}

/// Clear failure counter on successful login.
pub async fn clear_failures(pool: &Pool, email: &str) {
    let Ok(mut conn) = pool.get().await else { return };
    let key = format!("auth:login_attempts:{}", email);
    let _: Result<(), _> = conn.del(&key).await;
}
