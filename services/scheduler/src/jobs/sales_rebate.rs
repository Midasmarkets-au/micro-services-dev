use anyhow::Result;
use chrono::{DateTime, Datelike, TimeZone, Utc};
use rust_decimal::Decimal;
use sqlx::PgPool;
use tracing::{error, info, warn};

use crate::db::rebate_calc as rebate_db;
use crate::db::sales_rebate::{self as db, NewSalesRebateItem, NewSalesRebateSummary};
use crate::db::tenant;
use crate::jobs::rebate_calc::get_trimmed_symbol;
use crate::AppContext;

const ALPHA_TYPES: &[i16] = &[6, 9, 10, 14, 18, 19, 20];
const PRO_TYPES: &[i16] = &[5, 8, 21];

/// Manual trigger with explicit period — for backfill / testing.
pub async fn run_custom(
    ctx: AppContext,
    schedule_type: i16,
    period_start: DateTime<Utc>,
    period_end: DateTime<Utc>,
) -> Result<()> {
    info!(
        "SalesRebateCustom: schedule_type={} period [{}, {})",
        schedule_type, period_start, period_end
    );
    run_for_all_tenants(&ctx, schedule_type, period_start, period_end, false).await
}

/// Force-recalculate: deletes existing records for the period then regenerates.
/// For daily schedule (type=0), loops day-by-day to produce one record per day.
pub async fn run_force(
    ctx: AppContext,
    schedule_type: i16,
    period_start: DateTime<Utc>,
    period_end: DateTime<Utc>,
) -> Result<()> {
    info!(
        "SalesRebateForce: schedule_type={} period [{}, {})",
        schedule_type, period_start, period_end
    );
    if schedule_type == 0 {
        let mut day = period_start;
        while day < period_end {
            let next_day = day + chrono::Duration::days(1);
            let end = if next_day < period_end { next_day } else { period_end };
            run_for_all_tenants(&ctx, schedule_type, day, end, true).await?;
            day = next_day;
        }
        Ok(())
    } else {
        run_for_all_tenants(&ctx, schedule_type, period_start, period_end, true).await
    }
}

/// Daily SalesRebate settlement — settles previous day [yesterday 00:00 UTC, today 00:00 UTC).
/// Cron: 23:00 UTC daily.
pub async fn run_daily(ctx: AppContext) -> Result<()> {
    let now = Utc::now();
    let today_start = Utc
        .with_ymd_and_hms(now.year(), now.month(), now.day(), 0, 0, 0)
        .single()
        .ok_or_else(|| anyhow::anyhow!("Failed to compute today_start"))?;
    let yesterday_start = today_start - chrono::Duration::days(1);

    info!(
        "SalesRebateDaily: settling period [{}, {})",
        yesterday_start, today_start
    );

    run_for_all_tenants(&ctx, 0, yesterday_start, today_start, false).await
}

/// Monthly SalesRebate settlement — settles previous month [first of last month, first of this month).
/// Cron: 1st of month 00:05 UTC.
pub async fn run_monthly(ctx: AppContext) -> Result<()> {
    let now = Utc::now();
    let this_month_start = Utc
        .with_ymd_and_hms(now.year(), now.month(), 1, 0, 0, 0)
        .single()
        .ok_or_else(|| anyhow::anyhow!("Failed to compute this_month_start"))?;

    let (prev_year, prev_month) = if now.month() == 1 {
        (now.year() - 1, 12u32)
    } else {
        (now.year(), now.month() - 1)
    };
    let last_month_start = Utc
        .with_ymd_and_hms(prev_year, prev_month, 1, 0, 0, 0)
        .single()
        .ok_or_else(|| anyhow::anyhow!("Failed to compute last_month_start"))?;

    info!(
        "SalesRebateMonthly: settling period [{}, {})",
        last_month_start, this_month_start
    );

    run_for_all_tenants(&ctx, 3, last_month_start, this_month_start, false).await
}

async fn run_for_all_tenants(
    ctx: &AppContext,
    schedule_type: i16,
    period_start: DateTime<Utc>,
    period_end: DateTime<Utc>,
    force: bool,
) -> Result<()> {
    let tenant_ids = tenant::get_all_tenant_ids(&ctx.central_pool).await?;

    for tenant_id in tenant_ids {
        let pool = match ctx.tenant_pool(tenant_id).await {
            Ok(p) => p,
            Err(e) => {
                error!(
                    "SalesRebate: failed to get pool for tenant {}: {:#}",
                    tenant_id, e
                );
                continue;
            }
        };

        match rebate_db::is_rebate_enabled(&pool).await {
            Ok(true) => {
                info!("SalesRebate: rebate enabled for tenant {}", tenant_id);
            }
            Ok(false) => {
                info!(
                    "SalesRebate: rebate disabled for tenant {}, skipping",
                    tenant_id
                );
                continue;
            }
            Err(e) => {
                warn!(
                    "SalesRebate: failed to check RebateEnabled for tenant {}: {:#}",
                    tenant_id, e
                );
                continue;
            }
        }

        if let Err(e) =
            settle_for_schedule(ctx, &pool, schedule_type, period_start, period_end, force).await
        {
            error!(
                "SalesRebate: settle_for_schedule failed for tenant {}: {:#}",
                tenant_id, e
            );
        }
    }

    Ok(())
}

async fn settle_for_schedule(
    ctx: &AppContext,
    pool: &PgPool,
    schedule_type: i16,
    period_start: DateTime<Utc>,
    period_end: DateTime<Utc>,
    force: bool,
) -> Result<()> {
    let schemas = db::get_active_schemas_by_schedule(pool, schedule_type).await?;
    info!(
        "SalesRebate: found {} schema(s) for schedule_type={}",
        schemas.len(),
        schedule_type
    );
    if schemas.is_empty() {
        return Ok(());
    }

    for schema in &schemas {
        if force {
            db::delete_period_summary(pool, schema.sales_account_id, period_start).await?;
            info!(
                "SalesRebate: force-cleared sales_account={} period_start={}",
                schema.sales_account_id, period_start
            );
        } else if db::summary_exists(pool, schema.sales_account_id, period_start).await? {
            info!(
                "SalesRebate: skipping sales_account={} period_start={} — already settled",
                schema.sales_account_id, period_start
            );
            continue;
        }

        let trades =
            db::get_period_trades(pool, schema.sales_account_id, period_start, period_end)
                .await?;
        info!(
            "SalesRebate: sales_account={} found {} trade(s) in period",
            schema.sales_account_id,
            trades.len()
        );

        let ex_syms: Vec<String> =
            serde_json::from_str(&schema.exclude_symbol).unwrap_or_default();
        let ex_accs: Vec<String> =
            serde_json::from_str(&schema.exclude_account).unwrap_or_default();

        let mut items: Vec<NewSalesRebateItem> = Vec::new();
        let mut total_amount = Decimal::ZERO;

        for trade in &trades {
            let trimmed = get_trimmed_symbol(&trade.symbol);
            let excluded =
                ex_syms.iter().any(|s| s == &trimmed) ||
                ex_accs.iter().any(|a| a == &trade.uid.to_string());

            let (rebate_value, rebate_type) = if ALPHA_TYPES.contains(&trade.account_type) {
                (schema.alpha_rebate, "alpha")
            } else if PRO_TYPES.contains(&trade.account_type) {
                (schema.pro_rebate, "pro")
            } else {
                (schema.rebate, "default")
            };

            // _SalesRebateSchema.Rebate is old-table int×10000; divide to get real decimal rate.
            // amount = (rebate_value / 10000) * volume  (volume is raw units)
            let rebate_decimal = Decimal::from(rebate_value) / Decimal::from(10000);
            let amount = rebate_decimal * Decimal::from(trade.volume);

            if !excluded {
                if amount <= Decimal::ZERO {
                    warn!(
                        "SalesRebate: skipping ticket={} sales_account={} — zero amount (rebate={} volume={})",
                        trade.ticket, schema.sales_account_id, rebate_value, trade.volume
                    );
                    continue;
                }
                total_amount += amount;
            }

            items.push(NewSalesRebateItem {
                sales_account_id: schema.sales_account_id,
                trade_rebate_id: trade.trade_rebate_id,
                ticket: trade.ticket,
                trade_account_id: trade.trade_account_id,
                trade_account_number: trade.account_number,
                trade_account_type: trade.account_type,
                trade_account_fund_type: trade.fund_type,
                trade_account_currency_id: trade.currency_id,
                symbol: trade.symbol.clone(),
                volume: trade.volume,
                rebate_base: rebate_decimal,
                rebate_type: rebate_type.to_string(),
                amount: if excluded { Decimal::ZERO } else { amount },
                closed_on: trade.closed_on,
                excluded,
            });
        }

        if items.is_empty() {
            info!(
                "SalesRebate: no trades for sales_account={} period [{}, {}), skipping",
                schema.sales_account_id, period_start, period_end
            );
            continue;
        }

        let non_excluded_count = items.iter().filter(|i| !i.excluded).count() as i32;
        let matter_id = match ctx.idgen.generate_id().await {
            Ok(id) => id,
            Err(e) => {
                error!("SalesRebate: failed to generate matter_id: {:#}", e);
                continue;
            }
        };
        let summary = NewSalesRebateSummary {
            sales_account_id: schema.sales_account_id,
            period_start,
            period_end,
            schedule_type,
            total_amount,
            trade_count: non_excluded_count,
            matter_id,
        };

        match db::insert_batch(pool, &summary, &items).await {
            Ok((rebate_id, created_on)) => {
                info!(
                    "SalesRebate: inserted sales_account={} period_start={} rebate_id={} matter_id={} total_amount={} trade_count={} excluded={}",
                    schema.sales_account_id, period_start, rebate_id, matter_id, total_amount,
                    non_excluded_count,
                    items.iter().filter(|i| i.excluded).count()
                );
                if schema.auto_release && total_amount > Decimal::ZERO {
                    match db::release_summary(pool, rebate_id, created_on, schema.sales_account_id, total_amount, Some(matter_id)).await {
                        Ok(wt_id) => info!(
                            "SalesRebate: auto-released sales_account={} rebate_id={} wt_id={}",
                            schema.sales_account_id, rebate_id, wt_id
                        ),
                        Err(e) => error!(
                            "SalesRebate: auto-release failed rebate_id={}: {:#}",
                            rebate_id, e
                        ),
                    }
                }
            }
            Err(e) => {
                error!(
                    "SalesRebate: insert_batch failed for sales_account={} period_start={}: {:#}",
                    schema.sales_account_id, period_start, e
                );
            }
        }
    }

    Ok(())
}
