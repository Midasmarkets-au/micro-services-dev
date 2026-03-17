/// tonic gRPC client — calls mono's MonoCallbackService.NotifyReportDone
/// after a report is generated, so mono can send the WebSocket popup.
use tonic::transport::Channel;
use tracing::error;

use crate::generated::api::v1::{
    mono_callback_service_client::MonoCallbackServiceClient,
    NotifyReportDoneRequest,
};

#[derive(Clone)]
pub struct MonoCallbackClient {
    endpoint: String,
}

impl MonoCallbackClient {
    pub fn new(mono_grpc_url: &str) -> Self {
        Self {
            endpoint: mono_grpc_url.to_string(),
        }
    }

    /// Notify mono that a report has been generated.
    /// Fire-and-forget: errors are logged but not propagated.
    pub async fn notify_report_done(&self, tenant_id: i64, party_id: i64, report_name: &str) {
        let result = self.try_notify(tenant_id, party_id, report_name).await;
        if let Err(e) = result {
            error!(
                "Failed to notify mono of report completion (tenant={} party={}): {}",
                tenant_id, party_id, e
            );
        }
    }

    async fn try_notify(
        &self,
        tenant_id: i64,
        party_id: i64,
        report_name: &str,
    ) -> anyhow::Result<()> {
        let channel = Channel::from_shared(self.endpoint.clone())?
            .connect()
            .await?;

        let mut client = MonoCallbackServiceClient::new(channel);
        client
            .notify_report_done(NotifyReportDoneRequest {
                tenant_id,
                party_id,
                report_name: report_name.to_string(),
            })
            .await?;

        Ok(())
    }
}
