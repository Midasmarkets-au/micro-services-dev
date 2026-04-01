use anyhow::Result;
use tracing::{error, info};

use crate::{db, AppContext};

pub async fn execute(ctx: AppContext) -> Result<()> {
    // Check Redis disable flag (mirrors mono's CacheKeys.GetReleaseDisabledKey())
    if ctx.cache.get_string("ReleaseRebateDisabledKey").await?.is_some() {
        info!("ReleaseRebate disabled via cache key, skipping");
        return Ok(());
    }

    let tenant_ids = db::tenant::get_all_tenant_ids(&ctx.central_pool).await?;

    let tasks: Vec<_> = tenant_ids
        .into_iter()
        .map(|tid| {
            let ctx = ctx.clone();
            tokio::spawn(async move {
                if let Err(e) = process_tenant(ctx, tid).await {
                    error!(tenant_id = tid, error = %e, "ReleaseRebate: tenant processing failed");
                }
            })
        })
        .collect();

    futures::future::join_all(tasks).await;
    Ok(())
}

async fn process_tenant(ctx: AppContext, tenant_id: i64) -> Result<()> {
    let pool = ctx.tenant_pool(tenant_id).await?;

    if !db::rebate_calc::is_rebate_enabled(&pool).await? {
        return Ok(());
    }

    let items = db::rebate::get_pending_rebate_ids(&pool).await?;
    if items.is_empty() {
        return Ok(());
    }

    info!(tenant_id, count = items.len(), "ReleaseRebate: processing rebates");

    for (year, rebate_id) in items {
        if let Err(e) = db::rebate::process_rebate(&pool, year, rebate_id).await {
            error!(tenant_id, rebate_id, year, error = %e, "ReleaseRebate: failed to process rebate");
        }
    }

    Ok(())
}
