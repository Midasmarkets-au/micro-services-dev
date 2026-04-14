use tonic::transport::Channel;
use tracing::error;

use crate::generated::api_v1::{
    GetRecentUserAgentsRequest, GetTwoFactorSettingRequest, RegisterTenantUserRequest,
    SendAuthCodeRequest, SendPasswordResetEmailRequest, VerifyAuthCodeRequest, WriteLoginLogRequest,
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

/// Send password reset email (fire-and-forget).
pub async fn send_password_reset_email(
    client: &mut MonoAuthClient,
    tenant_id: i64,
    email: &str,
    reset_token: &str,
    reset_url: &str,
) {
    let req = SendPasswordResetEmailRequest {
        tenant_id,
        email: email.to_string(),
        reset_token: reset_token.to_string(),
        reset_url: reset_url.to_string(),
    };
    if let Err(e) = client.send_password_reset_email(req).await {
        error!("SendPasswordResetEmail gRPC error: {}", e);
    }
}

/// Parameters for registering a user's tenant data.
pub struct RegisterTenantUserParams<'a> {
    pub party_id: i64,
    pub tenant_id: i64,
    pub user_id: i64,
    pub uid: i64,
    pub email: &'a str,
    pub first_name: &'a str,
    pub last_name: &'a str,
    pub native_name: &'a str,
    pub phone: &'a str,
    pub ccc: &'a str,
    pub currency: &'a str,
    pub refer_code: &'a str,
    pub language: &'a str,
    pub site_id: i32,
    pub register_ip: &'a str,
    pub phone_confirmed: bool,
    pub source_comment: &'a str,
    pub utm: &'a str,
    pub country_code: &'a str,
}

/// Call mono to create Party/PartyRole/Lead/email for a newly registered user.
/// Returns Ok(true) on success, Ok(false) if mono reported failure, Err on transport error.
pub async fn register_tenant_user(
    client: &mut MonoAuthClient,
    p: RegisterTenantUserParams<'_>,
) -> Result<bool, tonic::Status> {
    let req = RegisterTenantUserRequest {
        party_id: p.party_id,
        tenant_id: p.tenant_id,
        user_id: p.user_id,
        uid: p.uid,
        email: p.email.to_string(),
        first_name: p.first_name.to_string(),
        last_name: p.last_name.to_string(),
        native_name: p.native_name.to_string(),
        phone: p.phone.to_string(),
        ccc: p.ccc.to_string(),
        currency: p.currency.to_string(),
        refer_code: p.refer_code.to_string(),
        language: p.language.to_string(),
        site_id: p.site_id,
        register_ip: p.register_ip.to_string(),
        phone_confirmed: p.phone_confirmed,
        source_comment: p.source_comment.to_string(),
        utm: p.utm.to_string(),
        country_code: p.country_code.to_string(),
    };
    match client.register_tenant_user(req).await {
        Ok(resp) => Ok(resp.into_inner().success),
        Err(e) => {
            error!("RegisterTenantUser gRPC error: {}", e);
            Err(e)
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
