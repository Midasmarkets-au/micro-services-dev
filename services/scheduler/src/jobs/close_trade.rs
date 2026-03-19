/// ExecuteCloseTradeJobAsync — mirrors ReportJob.GenerateCloseTradeReportAsync in C#.
///
/// Scheduled: cron "30 22 * * *" (22:30 UTC daily)
///
/// For tenants [10000, 10004]:
///   1. Generate WalletDailySnapshot (2 versions: UTC-based + MT5-based)
///   2. Generate WalletTransactionForTenant (2 versions: ClosingTime + ReleasedTime)
///   3. Generate Rebate (2 versions)
///   4. Generate DailyEquity (2 versions)
///   5. On Tuesday: add 3-day DailyEquity (Sat-Sun-Mon)
///   6. On Friday: add DemoAccount report
///   7. On last day of month: add monthly WalletTransaction + SalesRebate + Rebate
use anyhow::Result;
use chrono::{Datelike, Duration, TimeZone, Utc, Weekday};
use serde_json::json;
use tracing::{error, info};

use crate::db::tenant::{self, NewReportRequest};
use crate::report::request::process_request;
use crate::AppContext;

const REPORT_TENANTS: &[i64] = &[10000, 10004];

pub async fn execute(ctx: AppContext) -> Result<()> {
    info!("ExecuteCloseTradeJob: starting");

    let tasks: Vec<_> = REPORT_TENANTS
        .iter()
        .map(|&tenant_id| {
            let ctx = ctx.clone();
            tokio::spawn(async move {
                if let Err(e) = process_tenant(&ctx, tenant_id).await {
                    error!("ExecuteCloseTradeJob error tenant={}: {:#}", tenant_id, e);
                }
            })
        })
        .collect();

    for t in tasks {
        let _ = t.await;
    }

    info!("ExecuteCloseTradeJob: done");
    Ok(())
}

async fn process_tenant(ctx: &AppContext, tenant_id: i64) -> Result<()> {
    let tenant_pool = ctx.tenant_pool(tenant_id).await?;

    // Determine date (mirrors C# logic: UTC midnight + DST offset + MT5 GMT+2)
    // IMPORTANT: Use date_naive() to get midnight UTC, matching C# `DateTime.UtcNow.Date`.
    // Using Utc::now() directly shifts the computed report date by ~22 hours into the future.
    let utc_now = Utc::now();
    let midnight_utc = utc_now
        .date_naive()
        .and_hms_opt(0, 0, 0)
        .unwrap()
        .and_utc();
    let hours_gap = get_hours_gap(&tenant_pool).await.unwrap_or(2.0);
    let dst_offset = if crate::utils::is_dst_los_angeles(utc_now) { 20.0 } else { 21.0 };
    let date = midnight_utc
        + Duration::hours((dst_offset + hours_gap) as i64)
        + Duration::minutes(59)
        + Duration::seconds(59);

    // Find a valid active party ID
    let valid_party_id: Option<(i64,)> = sqlx::query_as(
        r#"SELECT "Id" FROM core."_Party" WHERE "Status" = 0 LIMIT 1"#,
    )
    .fetch_optional(&tenant_pool)
    .await?;

    let valid_party_id = match valid_party_id {
        Some((id,)) => id,
        None => {
            error!("No active party found for tenant {}", tenant_id);
            return Ok(());
        }
    };

    let from_utc = Utc.from_utc_datetime(&(date - Duration::days(1)).naive_utc());
    let to_utc = Utc.from_utc_datetime(&date.naive_utc());

    let mut requests = vec![
        // WalletDailySnapshot v1: UTC time based
        NewReportRequest {
            r#type: 19,
            party_id: valid_party_id,
            name: format!("Wallet Daily Snapshot (UTC Time Based) {}", date.format("%Y-%m-%d")),
            query: json!({ "snapshotDate": date, "useMT5Time": false }).to_string(),
            is_from_api: 0,
        },
        // WalletDailySnapshot v2: MT5 time based
        NewReportRequest {
            r#type: 19,
            party_id: valid_party_id,
            name: format!("Wallet Daily Snapshot (MT5 Time Based) {}", date.format("%Y-%m-%d")),
            query: json!({ "snapshotDate": date, "useMT5Time": true }).to_string(),
            is_from_api: 0,
        },
        // WalletTransactionForTenant v1: ClosingTime based
        NewReportRequest {
            r#type: 10,
            party_id: valid_party_id,
            name: format!("Wallet Daily Transaction (MT5 ClosingTime Based) {}", date.format("%Y-%m-%d")),
            query: json!({ "from": from_utc, "to": to_utc }).to_string(),
            is_from_api: 0,
        },
        // WalletTransactionForTenant v2: ReleasedTime based
        NewReportRequest {
            r#type: 10,
            party_id: valid_party_id,
            name: format!("Wallet Daily Transaction (ReleasedTime Based) {}", date.format("%Y-%m-%d")),
            query: json!({ "from": from_utc, "to": to_utc }).to_string(),
            is_from_api: 1,
        },
        // Rebate v1: ClosingTime based
        NewReportRequest {
            r#type: 5,
            party_id: valid_party_id,
            name: format!("Rebate Daily Record (MT5 ClosingTime Based) {}", date.format("%Y-%m-%d")),
            query: json!({ "from": from_utc, "to": to_utc }).to_string(),
            is_from_api: 0,
        },
        // Rebate v2: ReleasedTime based
        NewReportRequest {
            r#type: 5,
            party_id: valid_party_id,
            name: format!("Rebate Daily Record (ReleasedTime Based) {}", date.format("%Y-%m-%d")),
            query: json!({ "from": from_utc, "to": to_utc }).to_string(),
            is_from_api: 1,
        },
        // DailyEquity v1: ClosingTime based
        NewReportRequest {
            r#type: 23,
            party_id: valid_party_id,
            name: format!("Daily Equity Report (MT5 ClosingTime Based) {}", date.format("%Y-%m-%d")),
            query: json!({ "from": from_utc, "to": to_utc }).to_string(),
            is_from_api: 0,
        },
        // DailyEquity v2: ReleasedTime based
        NewReportRequest {
            r#type: 23,
            party_id: valid_party_id,
            name: format!("Daily Equity Report (ReleasedTime Based) {}", date.format("%Y-%m-%d")),
            query: json!({ "from": from_utc, "to": to_utc }).to_string(),
            is_from_api: 1,
        },
    ];

    // Tuesday: add 3-day equity (Sat + Sun + Mon)
    if date.weekday() == Weekday::Tue {
        let from_3day = Utc.from_utc_datetime(&(date - Duration::days(4)).naive_utc());
        let to_3day = Utc.from_utc_datetime(&(date - Duration::days(1)).naive_utc());
        requests.push(NewReportRequest {
            r#type: 23,
            party_id: valid_party_id,
            name: format!("Daily Equity Report (Sat-Mon) (MT5 ClosingTime Based) {}", date.format("%Y-%m-%d")),
            query: json!({ "from": from_3day, "to": to_3day }).to_string(),
            is_from_api: 0,
        });
        requests.push(NewReportRequest {
            r#type: 23,
            party_id: valid_party_id,
            name: format!("Daily Equity Report (Sat-Mon) (ReleasedTime Based) {}", date.format("%Y-%m-%d")),
            query: json!({ "from": from_3day, "to": to_3day }).to_string(),
            is_from_api: 1,
        });
    }

    // Friday: add DemoAccount report
    if date.weekday() == Weekday::Fri {
        requests.push(NewReportRequest {
            r#type: 20,
            party_id: valid_party_id,
            name: format!("Demo Account Report {}", date.format("%Y-%m-%d")),
            query: json!({ "Date": date }).to_string(),
            is_from_api: 0,
        });
    }

    // Last day of month: add monthly reports
    let next_day = date + Duration::days(1);
    if next_day.day() == 1 {
        let last_month_end = Utc.from_utc_datetime(
            &(date.with_day(1).unwrap() - Duration::days(1)).naive_utc(),
        );
        requests.push(NewReportRequest {
            r#type: 10,
            party_id: valid_party_id,
            name: format!("Wallet Monthly Transaction {}", date.format("%Y-%m")),
            query: json!({ "from": last_month_end, "to": date }).to_string(),
            is_from_api: 0,
        });
        requests.push(NewReportRequest {
            r#type: 12,
            party_id: valid_party_id,
            name: format!("Sales Rebate Monthly Record {}", date.format("%Y-%m")),
            query: json!({ "from": last_month_end, "to": date, "IsFromDirectClient": true }).to_string(),
            is_from_api: 0,
        });
        requests.push(NewReportRequest {
            r#type: 5,
            party_id: valid_party_id,
            name: format!("Rebate Monthly Record {}", date.format("%Y-%m")),
            query: json!({ "from": last_month_end, "to": date }).to_string(),
            is_from_api: 0,
        });
    }

    // Insert all requests and process concurrently
    let mut inserted = Vec::new();
    for req in &requests {
        let id = tenant::insert_report_request(&tenant_pool, req).await?;
        inserted.push(id);
    }

    let tasks: Vec<_> = inserted
        .into_iter()
        .zip(requests.iter())
        .map(|(id, req)| {
            let ctx = ctx.clone();
            let tenant_id = tenant_id;
            let req_type = req.r#type;
            let req_name = req.name.clone();
            tokio::spawn(async move {
                let tenant_pool = match ctx.tenant_pool(tenant_id).await {
                    Ok(p) => p,
                    Err(e) => {
                        error!("Failed to get tenant pool: {}", e);
                        return;
                    }
                };
                let request = match tenant::get_report_request(&tenant_pool, id).await {
                    Ok(Some(r)) => r,
                    _ => return,
                };
                if let Err(e) = process_request(&ctx, tenant_id, &request).await {
                    error!(
                        "CloseTradeJob error: tenant={} type={} name={}: {:#}",
                        tenant_id, req_type, req_name, e
                    );
                }
            })
        })
        .collect();

    for t in tasks {
        let _ = t.await;
    }

    Ok(())
}

async fn get_hours_gap(pool: &sqlx::PgPool) -> Result<f64> {
    // Read from tenant configuration (mirrors ConfigService.GetHoursGapForMT5Async)
    let row: Option<(String,)> = sqlx::query_as(
        r#"SELECT "Value" FROM cfg."_Configuration" WHERE "Key" = 'MT5HoursGap' LIMIT 1"#,
    )
    .fetch_optional(pool)
    .await?;

    Ok(row
        .and_then(|(v,)| v.parse::<f64>().ok())
        .unwrap_or(2.0))
}

