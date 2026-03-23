use anyhow::Result;
use tracing::info;

use crate::AppContext;

/// Trigger CalculateRebate job in mono via gRPC.
/// Cron: every 2 minutes (*/2 * * * *).
/// Actual execution and concurrency control ([DisableConcurrentExecution]) remain in mono/Hangfire.
pub async fn execute_calculate(ctx: AppContext) -> Result<()> {
    info!("CalculateRebateJob: triggering mono");
    ctx.mono_callback.trigger_calculate_rebate().await;
    Ok(())
}

/// Trigger ReleaseRebate job in mono via gRPC.
/// Cron: every 2 minutes (*/2 * * * *).
/// Actual execution and concurrency control ([DisableConcurrentExecution]) remain in mono/Hangfire.
pub async fn execute_release(ctx: AppContext) -> Result<()> {
    info!("ReleaseRebateJob: triggering mono");
    ctx.mono_callback.trigger_release_rebate().await;
    Ok(())
}
