use anyhow::Result;
use chrono::{DateTime, Utc};
use serde::{Deserialize, Serialize};
use sqlx::PgPool;
use tracing::{error, info};

use crate::db::tenant::{self, ReportRequest};
use crate::report::ReportRequestType;
use crate::storage::s3::S3Storage;
use crate::AppContext;

use super::{daily_equity, deposit, rebate, sales, trade};

/// Common query criteria parsed from ReportRequest.Query JSON
#[derive(Debug, Clone, Deserialize, Serialize, Default)]
pub struct DateRangeCriteria {
    pub from: Option<DateTime<Utc>>,
    pub to: Option<DateTime<Utc>>,
    pub emails: Option<Vec<String>>,
}

/// Process a single ReportRequest: dispatch to the correct handler,
/// generate CSV, upload to S3, and mark GeneratedOn.
pub async fn process_request(ctx: &AppContext, tenant_id: i64, request: &ReportRequest) -> Result<bool> {
    let report_type = ReportRequestType::try_from(request.r#type)?;
    let tenant_pool = ctx.tenant_pool(tenant_id).await?;

    info!(
        "Processing report request id={} type={:?} name={}",
        request.id, report_type, request.name
    );

    let result = dispatch(ctx, &tenant_pool, tenant_id, request, report_type).await;

    match result {
        Ok(file_name) => {
            tenant::mark_report_generated(&tenant_pool, request.id, &file_name).await?;
            info!("Report request id={} completed, file={}", request.id, file_name);
            Ok(true)
        }
        Err(e) => {
            error!("Report request id={} failed: {:#}", request.id, e);
            Err(e)
        }
    }
}

async fn dispatch(
    ctx: &AppContext,
    tenant_pool: &PgPool,
    tenant_id: i64,
    request: &ReportRequest,
    report_type: ReportRequestType,
) -> Result<String> {
    let criteria: DateRangeCriteria = serde_json::from_str(&request.query).unwrap_or_default();

    match report_type {
        ReportRequestType::DepositForTenant => {
            let csv_bytes = deposit::generate_deposit_csv(tenant_pool, &criteria).await?;
            upload_csv(ctx, tenant_id, request, csv_bytes).await
        }
        ReportRequestType::WithdrawForTenant
        | ReportRequestType::WithdrawPendingForTenant
        | ReportRequestType::WithdrawUnionPayPendingForTenant
        | ReportRequestType::WithdrawUSDTPendingForTenant => {
            let csv_bytes = deposit::generate_withdrawal_csv(tenant_pool, &criteria, report_type).await?;
            upload_csv(ctx, tenant_id, request, csv_bytes).await
        }
        ReportRequestType::Rebate => {
            let csv_bytes = rebate::generate_rebate_csv(
                tenant_pool,
                &criteria,
                request.is_from_api == 1,
            ).await?;
            upload_csv(ctx, tenant_id, request, csv_bytes).await
        }
        ReportRequestType::SalesRebateForTenant => {
            let csv_bytes = sales::generate_sales_rebate_csv(tenant_pool, &criteria).await?;
            upload_csv(ctx, tenant_id, request, csv_bytes).await
        }
        ReportRequestType::SalesRebateSumByAccountForTenant => {
            let csv_bytes = sales::generate_sales_rebate_sum_csv(tenant_pool, &criteria).await?;
            upload_csv(ctx, tenant_id, request, csv_bytes).await
        }
        ReportRequestType::WalletTransactionForTenant => {
            let csv_bytes = deposit::generate_wallet_transaction_csv(
                tenant_pool,
                &criteria,
                request.is_from_api == 1,
            ).await?;
            upload_csv(ctx, tenant_id, request, csv_bytes).await
        }
        ReportRequestType::WalletDailySnapshot => {
            let csv_bytes = deposit::generate_wallet_snapshot_csv(tenant_pool, &request.query).await?;
            upload_csv(ctx, tenant_id, request, csv_bytes).await
        }
        ReportRequestType::WalletOverviewForTenant => {
            let csv_bytes = deposit::generate_wallet_overview_csv(tenant_pool, &criteria).await?;
            upload_csv(ctx, tenant_id, request, csv_bytes).await
        }
        ReportRequestType::TransactionForTenant => {
            let csv_bytes = deposit::generate_transaction_csv(tenant_pool, &criteria).await?;
            upload_csv(ctx, tenant_id, request, csv_bytes).await
        }
        ReportRequestType::TradeForTenant
        | ReportRequestType::TradeForClient
        | ReportRequestType::TradeForAgent
        | ReportRequestType::TradeForSales => {
            let csv_bytes = trade::generate_trade_csv(
                ctx,
                tenant_pool,
                tenant_id,
                &criteria,
                report_type,
            ).await?;
            upload_csv(ctx, tenant_id, request, csv_bytes).await
        }
        ReportRequestType::DailyEquity => {
            let emails = criteria.emails.clone().unwrap_or_default();
            let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(1));
            let to = criteria.to.unwrap_or_else(Utc::now);
            let use_closing_time = request.is_from_api == 1;
            let csv_bytes = daily_equity::generate_daily_equity_csv(
                ctx,
                tenant_pool,
                from,
                to,
                use_closing_time,
                from,
                to,
            ).await?;
            let guid = upload_csv(ctx, tenant_id, request, csv_bytes.clone()).await?;
            if !emails.is_empty() {
                let subject = format!("Daily Equity Report {}", to.format("%Y-%m-%d"));
                let attachment_name = format!("{}.csv", request.name.replace(['/', '\\'], "_"));
                send_equity_email(ctx, &emails, &subject, &attachment_name, csv_bytes).await;
            }
            Ok(guid)
        }
        ReportRequestType::AccountSearchForTenant => {
            let csv_bytes = sales::generate_account_search_csv(tenant_pool, &criteria).await?;
            upload_csv(ctx, tenant_id, request, csv_bytes).await
        }
        ReportRequestType::SalesReportForTenant
        | ReportRequestType::SalesWeeklyReportForTenant => {
            let csv_bytes = sales::generate_sales_report_csv(tenant_pool, &criteria, report_type).await?;
            upload_csv(ctx, tenant_id, request, csv_bytes).await
        }
        ReportRequestType::IbReportForTenant
        | ReportRequestType::IbMonthlyReportForClient => {
            let csv_bytes = sales::generate_ib_report_csv(tenant_pool, &criteria, report_type).await?;
            upload_csv(ctx, tenant_id, request, csv_bytes).await
        }
        ReportRequestType::DemoAccount => {
            let csv_bytes = trade::generate_demo_account_csv(tenant_pool, &request.query).await?;
            upload_csv(ctx, tenant_id, request, csv_bytes).await
        }
    }
}

async fn send_equity_email(
    ctx: &AppContext,
    emails: &[String],
    subject: &str,
    attachment_name: &str,
    csv_bytes: Vec<u8>,
) {
    if let Err(e) = ctx
        .mail
        .send_with_attachment(emails, subject, attachment_name, csv_bytes)
        .await
    {
        error!("Failed to send equity report email: {:#}", e);
    }
}

async fn upload_csv(
    ctx: &AppContext,
    tenant_id: i64,
    request: &ReportRequest,
    csv_bytes: Vec<u8>,
) -> Result<String> {
    let key = S3Storage::report_csv_key(tenant_id, request.r#type, &request.name);
    let length = csv_bytes.len() as i64;
    ctx.s3.upload_csv(&key, csv_bytes).await?;

    // Build the S3 URL and insert a sto._Medium record so the mono
    // /api/v1/tenant/media/{guid} endpoint can serve this file.
    let url = format!("https://{}.s3.amazonaws.com/{}", ctx.s3.bucket(), key);
    let guid = uuid::Uuid::new_v4().to_string();
    let tenant_pool = ctx.tenant_pool(tenant_id).await?;
    let safe_name = format!("{}.csv", request.name.replace(['/', '\\'], "_"));
    tenant::insert_medium(&tenant_pool, &tenant::NewMedium {
        tenant_id,
        party_id: request.party_id,
        row_id: request.id,
        guid: &guid,
        file_name: &safe_name,
        url: &url,
        length,
    })
    .await?;

    Ok(guid)
}
