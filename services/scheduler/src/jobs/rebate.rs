use std::time::Duration;

use anyhow::Result;
use tracing::{error, info, warn};

use crate::db::rebate_calc as db;
use crate::db::tenant;
use crate::jobs::{rebate_calc, row_lock};
use crate::AppContext;

/// Calculate rebates for all tenants.
/// Reads from trd.trade_rebate_k8s, writes to core.matter_k8s + trd."_Rebate_{year}".
/// Cron: every 2 minutes (*/2 * * * *).
pub async fn execute_calculate(ctx: AppContext) -> Result<()> {
    let tenant_ids = tenant::get_all_tenant_ids(&ctx.central_pool).await?;

    for tenant_id in tenant_ids {
        let pool = match ctx.tenant_pool(tenant_id).await {
            Ok(p) => p,
            Err(e) => {
                error!("CalculateRebate: failed to get pool for tenant {}: {:#}", tenant_id, e);
                continue;
            }
        };

        // Check RebateEnabled toggle
        match db::is_rebate_enabled(&pool).await {
            Ok(true) => {}
            Ok(false) => {
                info!("CalculateRebate: rebate disabled for tenant {}, skipping", tenant_id);
                continue;
            }
            Err(e) => {
                warn!("CalculateRebate: failed to check RebateEnabled for tenant {}: {:#}", tenant_id, e);
                continue;
            }
        }

        // Acquire distributed lock (70 minutes) — use a scheduler-specific key to avoid
        // conflicting with mono's lock key "calculate_rebate_lock_key_tid:{tenantId}"
        let lock_key = format!("scheduler_calculate_rebate_lock_tid:{}", tenant_id);
        let lock_value = uuid::Uuid::new_v4().to_string();
        match ctx.cache.set_nx(&lock_key, &lock_value, Duration::from_secs(70 * 60)).await {
            Ok(true) => {}
            Ok(false) => {
                info!("CalculateRebate: lock held for tenant {}, skipping", tenant_id);
                continue;
            }
            Err(e) => {
                error!("CalculateRebate: failed to acquire lock for tenant {}: {:#}", tenant_id, e);
                continue;
            }
        }

        let result = run_for_tenant(&ctx, &pool, tenant_id).await;

        // Always release lock
        if let Err(e) = ctx.cache.delete(&lock_key).await {
            warn!("CalculateRebate: failed to release lock for tenant {}: {:#}", tenant_id, e);
        }

        if let Err(e) = result {
            error!("CalculateRebate: error for tenant {}: {:#}", tenant_id, e);
        }
    }

    Ok(())
}

async fn run_for_tenant(
    ctx: &AppContext,
    pool: &sqlx::PgPool,
    tenant_id: i64,
) -> Result<()> {
    let table = db::TRADE_REBATE_TABLE;
    let mut total_processed = 0usize;

    let (min_id, max_id) = match db::get_min_max_id(pool, table).await {
        Ok(ids) => ids,
        Err(e) => {
            warn!("CalculateRebate: get_min_max_id failed for tenant {}: {:#}", tenant_id, e);
            return Ok(());
        }
    };
    if min_id == 0 && max_id == 0 {
        info!("CalculateRebate: no pending records for tenant {}", tenant_id);
        return Ok(());
    }

    let mut page = 1i64;
    let size = 30i64;

    loop {
        let ids = db::get_pending_ids(pool, table, min_id, max_id, page, size).await?;
        let count = ids.len();

        for id in &ids {
            // Take the same per-row lock the event handler uses, so a sweep that
            // overlaps an in-flight event cannot insert a second set of rebates.
            let lock_key = row_lock::calc_key(tenant_id, *id);
            let outcome = row_lock::with_row_lock(
                &ctx.cache,
                &lock_key,
                rebate_calc::generate_rebates(ctx, pool, tenant_id, *id, table),
            )
            .await;

            match outcome {
                Ok(Some(())) => total_processed += 1,
                Ok(None) => info!(
                    "CalculateRebate: trade_rebate_id={} locked by another worker, skipping",
                    id
                ),
                Err(e) => error!(
                    "CalculateRebate: failed for trade_rebate_id={} tenant={}: {:#}",
                    id, tenant_id, e
                ),
            }
        }

        if count < size as usize {
            break;
        }
        page += 1;
    }

    if total_processed > 0 {
        info!(
            "CalculateRebate: tenant={} processed={}",
            tenant_id, total_processed
        );
    } else {
        info!("CalculateRebate: no pending records for tenant {}", tenant_id);
    }
    Ok(())
}

/// Execute ReleaseRebate directly in scheduler (no gRPC to mono).
/// Cron: every 2 minutes (*/2 * * * *).
pub async fn execute_release(ctx: AppContext) -> Result<()> {
    info!("ReleaseRebateJob: executing directly");
    super::release_rebate::execute(ctx).await
}
