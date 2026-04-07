use std::time::Duration;

use anyhow::Result;
use chrono::Datelike;
use tracing::{error, info, warn};

use crate::db::rebate_calc as db;
use crate::db::tenant;
use crate::jobs::rebate_calc;
use crate::AppContext;

/// Calculate rebates for all tenants.
/// Reads from trd."_TradeRebate_{year}", writes to core."_Matter_{year}" + trd."_Rebate_{year}".
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
    let current_year = chrono::Utc::now().year();
    let mut total_processed = 0usize;

    // Process previous year and current year — mirrors get_pending_rebate_ids behaviour.
    // Previous-year records can remain pending if they were not fully processed before rollover.
    for year in [current_year - 1, current_year] {
        let table = db::trade_rebate_table(year);

        // Ensure _Matter_{year} and _Rebate_{year} tables exist for this year
        db::ensure_rebate_tables(pool, year).await?;

        let (min_id, max_id) = match db::get_min_max_id(pool, &table).await {
            Ok(ids) => ids,
            // Table may not exist for previous year — skip silently
            Err(_) => continue,
        };
        if min_id == 0 && max_id == 0 {
            continue;
        }

        let mut page = 1i64;
        let size = 30i64;

        loop {
            let ids = db::get_pending_ids(pool, &table, min_id, max_id, page, size).await?;
            let count = ids.len();

            for id in &ids {
                if let Err(e) = rebate_calc::generate_rebates(ctx, pool, *id, &table, year).await {
                    error!(
                        "CalculateRebate: failed for trade_rebate_id={} year={} tenant={}: {:#}",
                        id, year, tenant_id, e
                    );
                } else {
                    total_processed += 1;
                }
            }

            if count < size as usize {
                break;
            }
            page += 1;
        }
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
