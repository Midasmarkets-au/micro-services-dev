use anyhow::Result;
use chrono::{DateTime, Datelike, Duration, NaiveDate, TimeZone, Utc};
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
/// USC (US-cent) account currency. Cent accounts trade cent-lots worth 1/100 of a
/// USD lot, so their rebate is scaled down by this factor.
const USC_CURRENCY_ID: i32 = 841;
const USC_CENT_FACTOR: i64 = 100;

// ── MT5-server (EET) day/month boundaries ────────────────────────────────────
// The MT5 server runs on EET: UTC+2 in winter, UTC+3 in summer (EEST). To keep
// SalesRebate settlement windows consistent with the rest of the platform — mono
// (`ReportJob` / `IsCurrentDSTLosAngeles`), the trade-rebate export/report views,
// and the front-end — DST is decided by US rules via `is_dst_los_angeles`: on US
// daylight time the server offset is +3h, otherwise +2h. The US-vs-EU DST gap is a
// known pre-existing quirk, mirrored here on purpose so these day boundaries line
// up with those views. (Same `is_dst_los_angeles` source as close_trade /
// account_daily — keep them unified.)

/// MT5-server UTC offset in hours (+2 or +3) for the given instant.
fn mt5_offset_hours(at: DateTime<Utc>) -> i64 {
    if crate::utils::is_dst_los_angeles(at) {
        3
    } else {
        2
    }
}

/// UTC instant of 00:00 MT5-server time for the server-local date `at` falls on.
fn server_day_start(at: DateTime<Utc>) -> DateTime<Utc> {
    // Locate the server calendar day `at` belongs to. Using the offset at `at` is
    // fine for *locating* the day because all callers pass instants (03:00 UTC,
    // ~21:00 UTC server midnight) well away from the DST-transition window.
    let off_at = mt5_offset_hours(at);
    let server_wall = at + Duration::hours(off_at);
    let midnight = server_wall.date_naive().and_hms_opt(0, 0, 0).unwrap();
    let midnight_utc = Utc.from_utc_datetime(&midnight);
    // Subtract the offset in effect *at that server midnight* (not at `at`). On DST
    // transition days the two differ; anchoring on the midnight makes consecutive
    // days tile seamlessly (no 1h overlap in spring / 1h gap in autumn).
    let off_mid = mt5_offset_hours(midnight_utc);
    midnight_utc - Duration::hours(off_mid)
}

/// UTC instant of 00:00 MT5-server time on the 1st of the given server month.
fn server_month_start(year: i32, month: u32) -> DateTime<Utc> {
    let first = NaiveDate::from_ymd_opt(year, month, 1)
        .unwrap()
        .and_hms_opt(0, 0, 0)
        .unwrap();
    let off = mt5_offset_hours(Utc.from_utc_datetime(&first));
    Utc.from_utc_datetime(&first) - Duration::hours(off)
}

// ── Per-summary detail CSV report (uploaded to S3; Download/Regenerate from the UI) ──

/// Deterministic S3 key. summary_id is a globally-unique snowflake, so no tenant
/// prefix is needed; regenerate writes the same key (overwrite).
fn report_s3_key(summary_id: i64) -> String {
    format!("sales-rebate-k8s-report/{}.csv", summary_id)
}

#[derive(sqlx::FromRow)]
struct ReportRow {
    ticket: i64,
    trade_account_number: i64,
    symbol: String,
    volume: i32,
    rebate_type: String,
    rebate_base: Decimal,
    amount: Decimal,
    closed_on: DateTime<Utc>,
    excluded: bool,
}

impl From<&NewSalesRebateItem> for ReportRow {
    fn from(i: &NewSalesRebateItem) -> Self {
        ReportRow {
            ticket: i.ticket,
            trade_account_number: i.trade_account_number,
            symbol: i.symbol.clone(),
            volume: i.volume,
            rebate_type: i.rebate_type.clone(),
            rebate_base: i.rebate_base,
            amount: i.amount,
            closed_on: i.closed_on,
            excluded: i.excluded,
        }
    }
}

fn csv_escape(s: &str) -> String {
    if s.contains([',', '"', '\n']) {
        format!("\"{}\"", s.replace('"', "\"\""))
    } else {
        s.to_string()
    }
}

/// header + one row per item + a final `Total` row = sum of non-excluded amount.
/// Columns match the Sales Rebate Details modal.
fn build_report_csv(rows: &[ReportRow]) -> Vec<u8> {
    let mut out = String::from("\u{FEFF}"); // BOM so Excel reads UTF-8
    out.push_str(
        "Status,Ticket,Account No.,Symbol,Volume (lots),Rebate Type,Rebate Base,Amount,Closed On\n",
    );
    let mut total = Decimal::ZERO;
    for r in rows {
        // Closed On in MT5-server time (EET, DST-aware) — matches the modal.
        let server_time = r.closed_on + Duration::hours(mt5_offset_hours(r.closed_on));
        let amount = if r.excluded {
            "0.0000".to_string()
        } else {
            total += r.amount;
            format!("{:.4}", r.amount)
        };
        out.push_str(&format!(
            "{},{},{},{},{:.2},{},{:.4},{},{}\n",
            if r.excluded { "Excluded" } else { "Included" },
            r.ticket,
            r.trade_account_number,
            csv_escape(&r.symbol),
            r.volume as f64 / 100.0,
            csv_escape(&r.rebate_type),
            r.rebate_base,
            amount,
            server_time.format("%Y-%m-%d %H:%M:%S"),
        ));
    }
    out.push_str(&format!("Total,,,,,,,{:.4},\n", total));
    out.into_bytes()
}

/// Regenerate one summary's report CSV and overwrite its S3 object.
/// Finds the tenant owning `summary_id`, re-reads its items, rebuilds, uploads.
pub async fn regenerate_report(ctx: AppContext, summary_id: i64) -> Result<()> {
    let tenant_ids = tenant::get_all_tenant_ids(&ctx.central_pool).await?;
    for tenant_id in tenant_ids {
        let pool = match ctx.tenant_pool(tenant_id).await {
            Ok(p) => p,
            Err(_) => continue,
        };
        let found: Option<i64> =
            sqlx::query_scalar("SELECT id FROM trd.sales_rebate_k8s WHERE id = $1")
                .bind(summary_id)
                .fetch_optional(&pool)
                .await
                .unwrap_or(None);
        if found.is_none() {
            continue;
        }

        let rows: Vec<ReportRow> = sqlx::query_as(
            r#"SELECT ticket, trade_account_number, symbol, volume, rebate_type,
                      rebate_base, amount, closed_on, excluded
               FROM trd.sales_rebate_item_k8s
               WHERE sales_rebate_id = $1
               ORDER BY closed_on"#,
        )
        .bind(summary_id)
        .fetch_all(&pool)
        .await?;

        let key = report_s3_key(summary_id);
        ctx.s3.upload_csv(&key, build_report_csv(&rows)).await?;
        info!(
            "SalesRebate: regenerated report summary_id={} rows={} key={}",
            summary_id,
            rows.len(),
            key
        );
        return Ok(());
    }
    warn!(
        "SalesRebate: regenerate_report — summary_id={} not found in any tenant",
        summary_id
    );
    Ok(())
}

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
    run_for_all_tenants(&ctx, schedule_type, period_start, period_end, false, None).await
}

/// Force-recalculate: deletes existing records for the period then regenerates.
/// For daily schedule (type=0), loops day-by-day to produce one record per day.
/// `sales_account_id` (optional) restricts the recalc to a single sales account —
/// used by the per-summary "Recalculate" button so only that record is regenerated.
pub async fn run_force(
    ctx: AppContext,
    schedule_type: i16,
    period_start: DateTime<Utc>,
    period_end: DateTime<Utc>,
    sales_account_id: Option<i64>,
) -> Result<()> {
    info!(
        "SalesRebateForce: schedule_type={} period [{}, {}) sales_account={:?}",
        schedule_type, period_start, period_end, sales_account_id
    );
    if schedule_type == 0 {
        // Walk MT5-server days so backfill windows match the daily job exactly.
        let mut cursor = server_day_start(period_start);
        while cursor < period_end {
            let next = server_day_start(cursor + Duration::hours(25)); // next server midnight
            let end = if next < period_end { next } else { period_end };
            run_for_all_tenants(&ctx, schedule_type, cursor, end, true, sales_account_id).await?;
            cursor = next;
        }
        Ok(())
    } else {
        run_for_all_tenants(&ctx, schedule_type, period_start, period_end, true, sales_account_id)
            .await
    }
}

/// Daily SalesRebate settlement — settles the most recently completed MT5-server
/// (EET) day. Cron: 03:00 UTC daily.
pub async fn run_daily(ctx: AppContext) -> Result<()> {
    let now = Utc::now();
    let today_start = server_day_start(now); // current server day 00:00 (in UTC)
    let yesterday_start = server_day_start(today_start - Duration::hours(1)); // prev server day 00:00

    info!(
        "SalesRebateDaily: settling MT5-server day [{}, {})",
        yesterday_start, today_start
    );

    run_for_all_tenants(&ctx, 0, yesterday_start, today_start, false, None).await
}

/// Monthly SalesRebate settlement — settles the previous MT5-server (EET) month.
/// Cron: 1st of month 03:00 UTC.
pub async fn run_monthly(ctx: AppContext) -> Result<()> {
    let now = Utc::now();
    let server_now = now + Duration::hours(mt5_offset_hours(now));
    let (year, month) = (server_now.year(), server_now.month());
    let this_month_start = server_month_start(year, month);

    let (prev_year, prev_month) = if month == 1 {
        (year - 1, 12u32)
    } else {
        (year, month - 1)
    };
    let last_month_start = server_month_start(prev_year, prev_month);

    info!(
        "SalesRebateMonthly: settling MT5-server month [{}, {})",
        last_month_start, this_month_start
    );

    run_for_all_tenants(&ctx, 3, last_month_start, this_month_start, false, None).await
}

async fn run_for_all_tenants(
    ctx: &AppContext,
    schedule_type: i16,
    period_start: DateTime<Utc>,
    period_end: DateTime<Utc>,
    force: bool,
    sales_account_id: Option<i64>,
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

        if let Err(e) = settle_for_schedule(
            ctx,
            &pool,
            schedule_type,
            period_start,
            period_end,
            force,
            sales_account_id,
        )
        .await
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
    sales_account_id: Option<i64>,
) -> Result<()> {
    let mut schemas = db::get_active_schemas_by_schedule(pool, schedule_type).await?;
    // Restrict to a single sales account when recalculating one summary record.
    if let Some(said) = sales_account_id {
        schemas.retain(|s| s.sales_account_id == said);
    }
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
            db::delete_period_summary(pool, schema.sales_account_id, schema.rebate_account_id, period_start).await?;
            info!(
                "SalesRebate: force-cleared sales_account={} rebate_account={} period_start={}",
                schema.sales_account_id, schema.rebate_account_id, period_start
            );
        } else if db::summary_exists(pool, schema.sales_account_id, schema.rebate_account_id, period_start).await? {
            info!(
                "SalesRebate: skipping sales_account={} rebate_account={} period_start={} — already settled",
                schema.sales_account_id, schema.rebate_account_id, period_start
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
            let mut rebate_decimal = Decimal::from(rebate_value) / Decimal::from(10000);
            // USC (cent) accounts: value/volume is 1/100 of USD, so scale the rebate down.
            if trade.currency_id == USC_CURRENCY_ID {
                rebate_decimal /= Decimal::from(USC_CENT_FACTOR);
            }
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
            rebate_account_id: schema.rebate_account_id,
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
                // Generate + upload the detail CSV report to S3 (best-effort).
                let report_rows: Vec<ReportRow> = items.iter().map(ReportRow::from).collect();
                if let Err(e) = ctx
                    .s3
                    .upload_csv(&report_s3_key(rebate_id), build_report_csv(&report_rows))
                    .await
                {
                    warn!(
                        "SalesRebate: report upload failed rebate_id={}: {:#}",
                        rebate_id, e
                    );
                }
                if schema.auto_release && total_amount > Decimal::ZERO {
                    match db::release_summary(pool, rebate_id, created_on, schema.rebate_account_id, total_amount, Some(matter_id)).await {
                        Ok(wt_id) => info!(
                            "SalesRebate: auto-released rebate_account={} rebate_id={} wt_id={}",
                            schema.rebate_account_id, rebate_id, wt_id
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

#[cfg(test)]
mod tests {
    use super::*;

    fn utc(y: i32, m: u32, d: u32, h: u32) -> DateTime<Utc> {
        Utc.with_ymd_and_hms(y, m, d, h, 0, 0).unwrap()
    }

    // Daily run at 03:00 UTC settles [yesterday_start, today_start) (see run_daily).
    fn today_start(run_at: DateTime<Utc>) -> DateTime<Utc> {
        server_day_start(run_at)
    }
    fn yesterday_start(run_at: DateTime<Utc>) -> DateTime<Utc> {
        server_day_start(server_day_start(run_at) - Duration::hours(1))
    }

    #[test]
    fn normal_day_matches_old_behavior() {
        // Summer (US DST, +3): server midnight of Jun 9 = Jun 8 21:00 UTC.
        assert_eq!(server_day_start(utc(2026, 6, 9, 3)), utc(2026, 6, 8, 21));
        // Winter (no DST, +2): server midnight of Jan 15 = Jan 14 22:00 UTC.
        assert_eq!(server_day_start(utc(2026, 1, 15, 3)), utc(2026, 1, 14, 22));
    }

    #[test]
    fn spring_forward_tiles_no_overlap() {
        // US spring-forward 2026-03-08 (LA DST starts 10:00 UTC).
        // today_start of day N's run must equal yesterday_start of day N+1's run.
        assert_eq!(
            today_start(utc(2026, 3, 8, 3)),
            yesterday_start(utc(2026, 3, 9, 3)),
            "adjacent daily runs must share the boundary (no 1h overlap)"
        );
        // The server day settled by the Mar 9 run is the 23h short day.
        let prev = yesterday_start(utc(2026, 3, 9, 3));
        let today = today_start(utc(2026, 3, 9, 3));
        assert_eq!(today - prev, Duration::hours(23));
    }

    #[test]
    fn fall_back_tiles_no_gap() {
        // US fall-back 2026-11-01 (LA DST ends 09:00 UTC).
        assert_eq!(
            today_start(utc(2026, 11, 1, 3)),
            yesterday_start(utc(2026, 11, 2, 3)),
            "adjacent daily runs must share the boundary (no 1h gap)"
        );
        // The server day settled by the Nov 2 run is the 25h long day.
        let prev = yesterday_start(utc(2026, 11, 2, 3));
        let today = today_start(utc(2026, 11, 2, 3));
        assert_eq!(today - prev, Duration::hours(25));
    }
}
