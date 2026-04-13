use std::sync::Arc;

use axum::{
    Json, Router,
    extract::State,
    http::HeaderMap,
    routing::post,
};
use serde_json::{Value, json};

use crate::{
    cookie, redis_store, security, state::AppState,
    generated::{
        http_v1::{LogoutRequest, SendLoginCodeRequest, ConfirmLoginCodeRequest,
            SendLoginCodeResponse, ConfirmLoginCodeResponse},
        http_routes::{LOGOUT_PATH, SEND_LOGIN_CODE_PATH, CONFIRM_LOGIN_CODE_PATH},
    },
};

pub fn router() -> Router<Arc<AppState>> {
    Router::new()
        .route(LOGOUT_PATH, post(logout))
        .route(SEND_LOGIN_CODE_PATH, post(send_login_code))
        .route(CONFIRM_LOGIN_CODE_PATH, post(confirm_login_code))
}

/// POST /api/v2/auth/logout
/// Clears the access_token cookie and deletes the refresh token from Redis if provided.
async fn logout(
    State(state): State<Arc<AppState>>,
    body: Option<Json<LogoutRequest>>,
) -> (HeaderMap, Json<Value>) {
    if let Some(Json(req)) = body {
        if !req.refresh_token.is_empty() {
            redis_store::delete_refresh_token(&state.redis, &req.refresh_token).await;
        }
    }

    let mut resp_headers = HeaderMap::new();
    cookie::clear_token_cookie(&mut resp_headers);
    (resp_headers, Json(json!(null)))
}

/// POST /api/v2/auth/token/code
/// Delegates to mono via gRPC: SendAuthCode(tenant_id, email, "Login")
/// Rate limiting (60/hour) is enforced by mono's IsEmailValidToSendAsync.
async fn send_login_code(
    State(state): State<Arc<AppState>>,
    Json(req): Json<SendLoginCodeRequest>,
) -> Json<SendLoginCodeResponse> {
    let email = req.email.trim().to_lowercase();

    // Lockout check (shared with password grant)
    if security::is_locked_out(&state.redis, &email).await {
        return Json(SendLoginCodeResponse { success: false, message: "Please contact customer service.".into() });
    }

    // Verify user exists in our DB
    let users = match crate::db::find_users_by_email(&state.pool, &email).await {
        Ok(u) => u,
        Err(_) => return Json(SendLoginCodeResponse { success: false, message: "server_error".into() }),
    };
    if users.is_empty() {
        // Don't reveal user existence; return success anyway (security best practice)
        return Json(SendLoginCodeResponse { success: true, message: "Login code sent.".into() });
    }
    if users.len() > 1 {
        return Json(SendLoginCodeResponse { success: false, message: "Email login not allowed for multi-tenant accounts".into() });
    }

    let user = &users[0];

    // Call mono gRPC to send the code
    if let Some(mono) = &state.mono_client {
        let mut client = mono.lock().await;
        crate::grpc::auth_client::send_auth_code(&mut client, user.tenant_id, &email, "Login").await;
    }

    Json(SendLoginCodeResponse { success: true, message: "Login code sent.".into() })
}

/// POST /api/v2/auth/token/code/confirm
/// Verifies the email code via mono gRPC, then issues JWT + cookie.
async fn confirm_login_code(
    State(state): State<Arc<AppState>>,
    Json(req): Json<ConfirmLoginCodeRequest>,
) -> (HeaderMap, Json<Value>) {
    let email = req.email.trim().to_lowercase();

    // Lockout check before any DB work
    if security::is_locked_out(&state.redis, &email).await {
        return (HeaderMap::new(), Json(json!({ "error": "__USER_IS_LOCKED_OUT__", "error_description": "Please contact customer service." })));
    }

    let users = match crate::db::find_users_by_email(&state.pool, &email).await {
        Ok(u) => u,
        Err(_) => {
            return (HeaderMap::new(), Json(json!({ "error": "server_error" })));
        }
    };
    if users.is_empty() {
        return (HeaderMap::new(), Json(json!({ "error": "not_found", "error_description": "User not found." })));
    }
    if users.len() > 1 {
        return (HeaderMap::new(), Json(json!({ "error": "invalid_request", "error_description": "Email login not allowed" })));
    }

    let user = &users[0];

    // Verify code via mono gRPC
    let code_valid = if let Some(mono) = &state.mono_client {
        let mut client = mono.lock().await;
        let (valid, _err) = crate::grpc::auth_client::verify_auth_code(
            &mut client, user.tenant_id, &email, &req.code,
        ).await;
        valid
    } else {
        return (
            HeaderMap::new(),
            Json(json!({ "error": "server_error", "error_description": "MONO_GRPC_ADDR not configured" })),
        );
    };

    if !code_valid {
        let locked = security::record_failure(&state.redis, &email).await;
        if locked {
            return (HeaderMap::new(), Json(json!({ "error": "__USER_IS_LOCKED_OUT__", "error_description": "Please contact customer service." })));
        }
        return (HeaderMap::new(), Json(json!({ "error": "invalid_grant", "error_description": "Invalid Code" })));
    }

    // Clear lockout counter on successful code verification
    security::clear_failures(&state.redis, &email).await;

    // Issue token
    let roles = crate::db::get_user_roles(&state.pool, user.id)
        .await
        .unwrap_or_default();
    let party_id_hashed = crate::hashids::encode_party_id(user.party_id);
    let display_name = user.guess_display_name();

    let params = crate::token::TokenParams {
        user_id: user.id,
        tenant_id: user.tenant_id,
        party_id_hashed: &party_id_hashed,
        god_party_id: 0,
        display_name: &display_name,
        email: user.email.as_deref().unwrap_or(""),
        roles: &roles,
        two_factor_enabled: false,
        lifetime_secs: state.access_token_lifetime,
        user_agent_hash: None,
        sales_account: None,
        agent_account: None,
        rep_account: None,
    };

    let token_result = match crate::token::generate_access_token(
        &params,
        &state.key_pair.private_der,
        &state.key_pair.kid,
    ) {
        Ok(t) => t,
        Err(e) => {
            return (HeaderMap::new(), Json(json!({ "error": "server_error", "error_description": e.to_string() })));
        }
    };

    let refresh_token = crate::token::generate_refresh_token();
    let store_data = redis_store::RefreshTokenData {
        user_id: user.id,
        tenant_id: user.tenant_id,
        party_id: party_id_hashed,
    };
    if let Err(e) = redis_store::store_refresh_token(&state.redis, &refresh_token, &store_data).await {
        redis_store::log_redis_error("store_refresh_token(code_confirm)", e);
    }

    let mut headers = HeaderMap::new();
    cookie::set_token_cookie(
        &mut headers,
        &token_result.access_token,
        token_result.expires_in,
        state.secure_cookie,
    );

    let resp = ConfirmLoginCodeResponse {
        token_type: "Bearer".into(),
        expires_in: token_result.expires_in,
        refresh_token,
    };
    (headers, Json(json!(resp)))
}
