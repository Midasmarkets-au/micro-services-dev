use anyhow::Result;
use chrono::{Datelike, Utc};
use tracing::{error, info};

use crate::db::{partition, tenant};
use crate::AppContext;

/// Monthly partition maintenance job (runs on the 1st of each month at 02:00 UTC).
///
/// For each tenant DB:
///   1. Ensures all K8s partitioned parent tables exist.
///   2. Pre-creates year sub-partitions for the current year and next year.
pub async fn execute(ctx: AppContext) -> Result<()> {
    info!("PartitionMaintenanceJob: starting");

    let tenant_ids = tenant::get_all_tenant_ids(&ctx.central_pool).await?;
    let current_year = Utc::now().year();

    for tenant_id in tenant_ids {
        let pool = match ctx.tenant_pool(tenant_id).await {
            Ok(p) => p,
            Err(e) => {
                error!("PartitionMaintenance: failed to get pool for tenant {}: {:#}", tenant_id, e);
                continue;
            }
        };

        if let Err(e) = run_for_tenant(&pool, tenant_id, current_year).await {
            error!("PartitionMaintenance: error for tenant {}: {:#}", tenant_id, e);
        }
    }

    info!("PartitionMaintenanceJob: done");
    Ok(())
}

async fn run_for_tenant(pool: &sqlx::PgPool, tenant_id: i64, current_year: i32) -> Result<()> {
    // Ensure parent tables exist (idempotent)
    partition::ensure_parent_tables(pool).await?;

    // Pre-create year sub-partitions for current year and next year
    for year in [current_year, current_year + 1] {
        partition::ensure_year_partition(pool, "core", "_MatterK8s", year).await?;
        partition::ensure_year_partition_snake(pool, "core", "activity_k8s", year).await?;
        partition::ensure_year_partition_snake(pool, "acct", "wallet_transaction_k8s", year).await?;
        partition::ensure_year_partition_snake(pool, "trd", "trade_rebate_k8s", year).await?;
        partition::ensure_year_partition_snake(pool, "trd", "rebate_k8s", year).await?;
    }

    info!("PartitionMaintenance: tenant={} years=[{}, {}] OK", tenant_id, current_year, current_year + 1);
    Ok(())
}
