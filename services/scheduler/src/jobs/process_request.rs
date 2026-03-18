/// ProcessReportRequest job — mirrors ReportJob.ProcessReportRequest in C#.
///
/// Flow:
///   1. Receive job payload (tenant_id + request_id)
///   2. Load ReportRequest from sto._ReportRequest
///   3. Dispatch to report processor (generates CSV, uploads to S3)
///   4. Mark GeneratedOn in DB
///   5. Notify mono via HTTP callback (for WebSocket notification to client)
use anyhow::Result;
use serde::{Deserialize, Serialize};
use tracing::{error, info};

use crate::db::tenant;
use crate::report::request::process_request;
use crate::AppContext;

/// Job payload — serialized into apalis job queue.
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ProcessReportRequestJob {
    pub tenant_id: i64,
    pub request_id: i64,
}

/// Execute the job. Called by the apalis worker.
pub async fn execute(ctx: AppContext, job: ProcessReportRequestJob) -> Result<()> {
    info!(
        "ProcessReportRequest: tenant={} request_id={}",
        job.tenant_id, job.request_id
    );

    let tenant_pool = ctx.tenant_pool(job.tenant_id).await?;
    let request = tenant::get_report_request(&tenant_pool, job.request_id).await?;

    let request = match request {
        Some(r) => r,
        None => {
            error!(
                "ProcessReportRequest: request_id={} not found",
                job.request_id
            );
            return Err(anyhow::anyhow!("__REPORT_REQUEST_NOT_FOUND__"));
        }
    };

    let result = process_request(&ctx, job.tenant_id, &request).await;

    match result {
        Ok(true) => {
            // Notify mono via gRPC so it can send WebSocket notification to the client
            ctx.mono_callback
                .notify_report_done(job.tenant_id, request.party_id, &request.name)
                .await;
            Ok(())
        }
        Ok(false) => {
            error!(
                "ProcessReportRequest: request_id={} returned false",
                job.request_id
            );
            Ok(())
        }
        Err(e) => Err(e),
    }
}

