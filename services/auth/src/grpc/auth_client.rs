use tonic::transport::Channel;
use tracing::error;

use crate::generated::api_v1::{
    GetRecentUserAgentsRequest, GetTwoFactorSettingRequest, SendAuthCodeRequest,
    VerifyAuthCodeRequest, WriteLoginLogRequest,
    auth_service_client::AuthServiceClient,
};

pub type MonoAuthClient = AuthServiceClient<Channel>;

pub async fn connect(mono_grpc_addr: &str) -> Result<MonoAuthClient, tonic::transport::Error> {
    AuthServiceClient::connect(mono_grpc_addr.to_string()).await
}

/// Check if email 2FA is enabled for this tenant/party.
pub async fn get_two_factor_setting(
    client: &mut MonoAuthClient,
    tenant_id: i64,
    party_id: i64,
) -> bool {
    let req = GetTwoFactorSettingRequest { tenant_id, party_id };
    match client.get_two_factor_setting(req).await {
        Ok(resp) => resp.into_inner().login_code_enabled,
        Err(e) => {
            error!("GetTwoFactorSetting gRPC error: {}", e);
            false
        }
    }
}

/// Fetch recent user-agent strings for new-device detection.
pub async fn get_recent_user_agents(
    client: &mut MonoAuthClient,
    tenant_id: i64,
    party_id: i64,
    limit: i32,
) -> Vec<String> {
    let req = GetRecentUserAgentsRequest { tenant_id, party_id, limit };
    match client.get_recent_user_agents(req).await {
        Ok(resp) => resp.into_inner().user_agents,
        Err(e) => {
            error!("GetRecentUserAgents gRPC error: {}", e);
            vec![]
        }
    }
}

/// Send a 2FA email code (fire-and-forget; errors are logged only).
pub async fn send_auth_code(
    client: &mut MonoAuthClient,
    tenant_id: i64,
    email: &str,
    event_label: &str,
) {
    let req = SendAuthCodeRequest {
        tenant_id,
        email: email.to_string(),
        event_label: event_label.to_string(),
    };
    if let Err(e) = client.send_auth_code(req).await {
        error!("SendAuthCode gRPC error: {}", e);
    }
}

/// Verify a 2FA email code. Returns (valid, error_code).
pub async fn verify_auth_code(
    client: &mut MonoAuthClient,
    tenant_id: i64,
    email: &str,
    code: &str,
) -> (bool, String) {
    let req = VerifyAuthCodeRequest {
        tenant_id,
        email: email.to_string(),
        code: code.to_string(),
    };
    match client.verify_auth_code(req).await {
        Ok(resp) => {
            let r = resp.into_inner();
            (r.valid, r.error_code)
        }
        Err(e) => {
            error!("VerifyAuthCode gRPC error: {}", e);
            (false, "server_error".to_string())
        }
    }
}

/// Write login log (fire-and-forget).
pub async fn write_login_log(
    client: &mut MonoAuthClient,
    tenant_id: i64,
    party_id: i64,
    ip: &str,
    user_agent: &str,
    referer: &str,
) {
    let req = WriteLoginLogRequest {
        tenant_id,
        party_id,
        ip: ip.to_string(),
        user_agent: user_agent.to_string(),
        referer: referer.to_string(),
    };
    if let Err(e) = client.write_login_log(req).await {
        error!("WriteLoginLog gRPC error: {}", e);
    }
}
