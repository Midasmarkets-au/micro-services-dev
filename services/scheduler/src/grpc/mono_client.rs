/// tonic gRPC client — calls mono's MonoCallbackService.
/// Used to notify report completion and to trigger recurring jobs whose cron
/// schedule is now managed by the Rust scheduler.
use tonic::transport::Channel;
use tracing::error;

use crate::generated::api::v1::{
    mono_callback_service_client::MonoCallbackServiceClient,
    NotifyReportDoneRequest,
    TriggerCalculateRebateRequest,
    TriggerReleaseRebateRequest,
    TriggerCryptoMonitorRequest,
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

    /// Trigger CalculateRebate job in mono.
    /// Fire-and-forget: errors are logged but not propagated.
    pub async fn trigger_calculate_rebate(&self) {
        if let Err(e) = self.try_trigger_calculate_rebate().await {
            error!("Failed to trigger CalculateRebate in mono: {}", e);
        }
    }

    /// Trigger ReleaseRebate job in mono.
    /// Fire-and-forget: errors are logged but not propagated.
    pub async fn trigger_release_rebate(&self) {
        if let Err(e) = self.try_trigger_release_rebate().await {
            error!("Failed to trigger ReleaseRebate in mono: {}", e);
        }
    }

    /// Trigger CryptoMonitor job in mono.
    /// Fire-and-forget: errors are logged but not propagated.
    pub async fn trigger_crypto_monitor(&self) {
        if let Err(e) = self.try_trigger_crypto_monitor().await {
            error!("Failed to trigger CryptoMonitor in mono: {}", e);
        }
    }

    async fn try_trigger_calculate_rebate(&self) -> anyhow::Result<()> {
        let channel = Channel::from_shared(self.endpoint.clone())?.connect().await?;
        let mut c = MonoCallbackServiceClient::new(channel);
        c.trigger_calculate_rebate(TriggerCalculateRebateRequest { tenant_id: 0 }).await?;
        Ok(())
    }

    async fn try_trigger_release_rebate(&self) -> anyhow::Result<()> {
        let channel = Channel::from_shared(self.endpoint.clone())?.connect().await?;
        let mut c = MonoCallbackServiceClient::new(channel);
        c.trigger_release_rebate(TriggerReleaseRebateRequest { tenant_id: 0 }).await?;
        Ok(())
    }

    async fn try_trigger_crypto_monitor(&self) -> anyhow::Result<()> {
        let channel = Channel::from_shared(self.endpoint.clone())?.connect().await?;
        let mut c = MonoCallbackServiceClient::new(channel);
        c.trigger_crypto_monitor(TriggerCryptoMonitorRequest { tenant_id: 0 }).await?;
        Ok(())
    }
}
