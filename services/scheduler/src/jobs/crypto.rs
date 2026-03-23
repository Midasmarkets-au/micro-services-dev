use anyhow::Result;
use tracing::info;

use crate::AppContext;

/// Trigger CryptoMonitor job in mono via gRPC.
/// Cron: every minute (*/1 * * * *).
/// Actual execution and concurrency control ([DisableConcurrentExecution]) remain in mono/Hangfire.
pub async fn execute(ctx: AppContext) -> Result<()> {
    info!("CryptoMonitorJob: triggering mono");
    ctx.mono_callback.trigger_crypto_monitor().await;
    Ok(())
}
