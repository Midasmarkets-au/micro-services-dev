//! Per-row distributed locks for the rebate pipeline.
//!
//! The cron sweep and the NATS event handlers both drive the same rows. Neither
//! `generate_rebates` nor `process_rebate` claims its row in the database — a
//! trade_rebate stays `status = 0` until every rebate has been inserted — so two
//! concurrent workers would each read "no existing rebates" and insert a full
//! set, doubling the payout. These locks are what keeps the two paths disjoint.
//!
//! The tenant-wide lock in `rebate::execute_calculate` cannot serve this purpose:
//! it is held for the whole sweep, so every event arriving during a sweep would
//! be dropped rather than merely ordered after it.

use std::time::Duration;

use anyhow::Result;
use tracing::warn;

use crate::cache::redis::RedisCache;

/// Must exceed the worst case for a single row — a cross-currency agent chain
/// issues an MT price query per level — while staying well under the NATS
/// `ack_wait` of 300s, so a crashed worker's lock expires before JetStream
/// redelivers the message to someone else.
const ROW_LOCK_TTL: Duration = Duration::from_secs(120);

pub fn calc_key(tenant_id: i64, trade_rebate_id: i64) -> String {
    format!("rebate_calc_lock:{}:{}", tenant_id, trade_rebate_id)
}

pub fn release_key(tenant_id: i64, rebate_id: i64) -> String {
    format!("rebate_release_lock:{}:{}", tenant_id, rebate_id)
}

/// Runs `work` while holding `key`, releasing it on both the success and the
/// error path. Returns `Ok(None)` without running `work` when the lock is
/// already held, which callers should treat as "someone else has this row"
/// rather than as a failure.
///
/// `work` is a future rather than a closure because futures are lazy: nothing
/// in it executes until the lock has been acquired.
pub async fn with_row_lock<T, F>(cache: &RedisCache, key: &str, work: F) -> Result<Option<T>>
where
    F: std::future::Future<Output = Result<T>>,
{
    let token = uuid::Uuid::new_v4().to_string();
    if !cache.set_nx(key, &token, ROW_LOCK_TTL).await? {
        return Ok(None);
    }

    let result = work.await;

    if let Err(e) = cache.delete(key).await {
        // The TTL still bounds how long the row stays blocked, so this degrades
        // into a delay rather than a stuck row.
        warn!("failed to release row lock {}: {:#}", key, e);
    }

    result.map(Some)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn keys_are_scoped_per_tenant_and_row() {
        assert_eq!(calc_key(10000, 42), "rebate_calc_lock:10000:42");
        assert_eq!(release_key(10000, 42), "rebate_release_lock:10000:42");
        // The two stages lock independently: releasing a rebate must not be
        // blocked by a concurrent recalculation of a different row.
        assert_ne!(calc_key(10000, 42), release_key(10000, 42));
    }
}
