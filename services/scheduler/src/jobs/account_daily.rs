/// GenerateAccountDailyConfirmationReport — mirrors ReportJob in C#.
///
/// Scheduled: cron "29 21 * * 1-5" (DST) or "29 22 * * 1-5" (non-DST)
///
/// Three-stage pipeline per tenant, coordinated via Redis:
///   Stage 1 (GenerateAccountTask): find accounts with trades → insert AccountReport rows
///   Stage 2 (ProcessAccountReportTask): generate HTML → upload to S3
///   Stage 3 (SendMailTask): send emails
use anyhow::Result;
use chrono::{Duration, Utc};
use std::time::Duration as StdDuration;
use tracing::{error, info};

use crate::cache::redis::{
    generate_account_task_key, process_account_report_task_key, send_mail_task_key, TODAY_KEY,
};
use crate::db::tenant;
use crate::report::account;
use crate::AppContext;

pub async fn execute(ctx: AppContext) -> Result<()> {
    info!("GenerateAccountDailyConfirmationReport: starting");

    let central_pool = ctx.central_pool.clone();
    let tenant_ids = tenant::get_all_tenant_ids(&central_pool).await?;

    let tasks: Vec<_> = tenant_ids
        .into_iter()
        .map(|tenant_id| {
            let ctx = ctx.clone();
            tokio::spawn(async move {
                if let Err(e) = process_tenant(ctx, tenant_id).await {
                    error!(
                        "AccountDailyConfirmation error tenant={}: {:#}",
                        tenant_id, e
                    );
                }
            })
        })
        .collect();

    for t in tasks {
        let _ = t.await;
    }

    info!("GenerateAccountDailyConfirmationReport: done");
    Ok(())
}

async fn process_tenant(ctx: AppContext, tenant_id: i64) -> Result<()> {
    // Determine the date for today's report
    let date_str = match ctx.cache.get_string(TODAY_KEY).await? {
        Some(d) => d,
        None => {
            let today = Utc::now().date_naive().to_string();
            ctx.cache
                .set_string(TODAY_KEY, &today, StdDuration::from_secs(10 * 3600))
                .await?;
            today
        }
    };

    let date = chrono::NaiveDate::parse_from_str(&date_str, "%Y-%m-%d")
        .map(|d| d.and_hms_opt(0, 0, 0).unwrap())
        .map(|dt| chrono::DateTime::<Utc>::from_naive_utc_and_offset(dt, Utc))
        .unwrap_or_else(|_| Utc::now());

    let dst_offset = if is_dst_los_angeles(date) { 20i64 } else { 21 };
    let to = date + Duration::hours(dst_offset) + Duration::minutes(59) + Duration::seconds(59);

    // Skip if the report window hasn't closed yet
    if to > Utc::now() {
        info!("AccountDailyConfirmation: window not closed yet for tenant {}", tenant_id);
        return Ok(());
    }

    // Run all three stages concurrently (they self-coordinate via Redis)
    let (r1, r2, r3) = tokio::join!(
        generate_account_task(ctx.clone(), tenant_id, date, to),
        process_account_report_task(ctx.clone(), tenant_id, date),
        send_mail_task(ctx.clone(), tenant_id, date),
    );
    r1?;
    r2?;
    r3?;
    Ok(())
}

/// Stage 1: Identify accounts with trades → insert AccountReport rows.
async fn generate_account_task(
    ctx: AppContext,
    tenant_id: i64,
    from: chrono::DateTime<Utc>,
    to: chrono::DateTime<Utc>,
) -> Result<()> {
    let key = generate_account_task_key(tenant_id, &from.format("%Y-%m-%d").to_string());
    ctx.cache
        .set_string(&key, "Started", StdDuration::from_secs(10 * 3600))
        .await?;

    let tenant_pool = ctx.tenant_pool(tenant_id).await?;
    let auth_pool = ctx.auth_pool.clone();

    account::generate_account_to_send_confirmation_report(
        &tenant_pool,
        &auth_pool,
        tenant_id,
        from,
        to,
    )
    .await?;

    ctx.cache
        .set_string(&key, "Ended", StdDuration::from_secs(10 * 3600))
        .await?;
    Ok(())
}

/// Stage 2: Generate HTML for each AccountReport → upload to S3.
async fn process_account_report_task(
    ctx: AppContext,
    tenant_id: i64,
    date: chrono::DateTime<Utc>,
) -> Result<()> {
    let date_str = date.format("%Y-%m-%d").to_string();
    let process_key = process_account_report_task_key(tenant_id, &date_str);
    let generate_key = generate_account_task_key(tenant_id, &date_str);

    ctx.cache
        .set_string(&process_key, "Started", StdDuration::from_secs(10 * 3600))
        .await?;

    let tenant_pool = ctx.tenant_pool(tenant_id).await?;

    loop {
        let remains = account::process_account_report_model(
            &tenant_pool,
            &ctx.s3,
            tenant_id,
            date,
        )
        .await?;

        let generate_done = ctx.cache.get_string(&generate_key).await?.as_deref() == Some("Ended");

        if generate_done && remains == 0 {
            break;
        }

        if !generate_done {
            tokio::time::sleep(StdDuration::from_secs(2)).await;
        }
    }

    ctx.cache
        .set_string(&process_key, "Ended", StdDuration::from_secs(10 * 3600))
        .await?;
    Ok(())
}

/// Stage 3: Send emails for processed AccountReports.
async fn send_mail_task(
    ctx: AppContext,
    tenant_id: i64,
    date: chrono::DateTime<Utc>,
) -> Result<()> {
    let date_str = date.format("%Y-%m-%d").to_string();
    let mail_key = send_mail_task_key(tenant_id, &date_str);
    let generate_key = generate_account_task_key(tenant_id, &date_str);
    let process_key = process_account_report_task_key(tenant_id, &date_str);

    ctx.cache
        .set_string(&mail_key, "Started", StdDuration::from_secs(10 * 3600))
        .await?;

    let tenant_pool = ctx.tenant_pool(tenant_id).await?;
    let auth_pool = ctx.auth_pool.clone();

    loop {
        tokio::time::sleep(StdDuration::from_secs(10)).await;

        let remains = account::process_send_account_daily_report_email(
            &tenant_pool,
            &auth_pool,
            &ctx.s3,
            &ctx.mail,
            date,
        )
        .await?;

        let generate_done = ctx.cache.get_string(&generate_key).await?.as_deref() == Some("Ended");
        let process_done = ctx.cache.get_string(&process_key).await?.as_deref() == Some("Ended");

        if generate_done && process_done && remains == 0 {
            break;
        }
    }

    ctx.cache
        .set_string(&mail_key, "Ended", StdDuration::from_secs(10 * 3600))
        .await?;
    Ok(())
}

fn is_dst_los_angeles(dt: chrono::DateTime<Utc>) -> bool {
    let month = dt.month();
    (3..=11).contains(&month)
}

use chrono::Datelike;
