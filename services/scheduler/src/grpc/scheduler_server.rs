/// tonic gRPC server — implements SchedulerService defined in scheduler.proto.
///
/// mono calls:
///   EnqueueReportRequest  → push apalis job
///   TriggerCloseTrade     → spawn close_trade job
///   TriggerAccountDaily   → spawn account_daily job
use apalis::prelude::TaskSink;
use apalis_redis::RedisStorage;
use futures::stream;
use tonic::{Request, Response, Status};
use tracing::{error, info};

use crate::jobs::process_request::ProcessReportRequestJob;
use crate::AppContext;

// Import generated tonic types
use crate::generated::api::v1::{
    scheduler_service_server::SchedulerService,
    EnqueueReportRequestRequest, EnqueueReportRequestResponse,
    TriggerJobRequest, TriggerJobResponse,
};

pub struct SchedulerGrpcServer {
    pub ctx: AppContext,
}

#[tonic::async_trait]
impl SchedulerService for SchedulerGrpcServer {
    async fn enqueue_report_request(
        &self,
        request: Request<EnqueueReportRequestRequest>,
    ) -> Result<Response<EnqueueReportRequestResponse>, Status> {
        let req = request.into_inner();
        info!(
            "gRPC EnqueueReportRequest: tenant={} request_id={}",
            req.tenant_id, req.request_id
        );

        let job = ProcessReportRequestJob {
            tenant_id: req.tenant_id,
            request_id: req.request_id,
        };

        let mut storage: RedisStorage<ProcessReportRequestJob> =
            RedisStorage::new(self.ctx.apalis_conn.clone());
        let task = apalis::prelude::Task::builder(job).build();
        let mut tasks = stream::iter(vec![task]);

        storage.push_all(&mut tasks).await.map_err(|e| {
            error!("Failed to enqueue job: {}", e);
            Status::internal(format!("Failed to enqueue job: {}", e))
        })?;

        Ok(Response::new(EnqueueReportRequestResponse {
            success: true,
            message: format!("Job enqueued for request_id={}", req.request_id),
        }))
    }

    async fn trigger_close_trade(
        &self,
        _request: Request<TriggerJobRequest>,
    ) -> Result<Response<TriggerJobResponse>, Status> {
        let ctx = self.ctx.clone();
        tokio::spawn(async move {
            if let Err(e) = crate::jobs::close_trade::execute(ctx).await {
                error!("TriggerCloseTrade error: {:#}", e);
            }
        });

        Ok(Response::new(TriggerJobResponse {
            success: true,
            message: "CloseTradeJob triggered".to_string(),
        }))
    }

    async fn trigger_account_daily(
        &self,
        _request: Request<TriggerJobRequest>,
    ) -> Result<Response<TriggerJobResponse>, Status> {
        let ctx = self.ctx.clone();
        tokio::spawn(async move {
            if let Err(e) = crate::jobs::account_daily::execute(ctx).await {
                error!("TriggerAccountDaily error: {:#}", e);
            }
        });

        Ok(Response::new(TriggerJobResponse {
            success: true,
            message: "AccountDailyJob triggered".to_string(),
        }))
    }
}
