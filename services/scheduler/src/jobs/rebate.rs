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

/// Execute ReleaseRebate directly in scheduler (no gRPC to mono).
/// Cron: every 2 minutes (*/2 * * * *).
pub async fn execute_release(ctx: AppContext) -> Result<()> {
    info!("ReleaseRebateJob: executing directly");
    super::release_rebate::execute(ctx).await
}
