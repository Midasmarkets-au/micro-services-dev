use std::net::SocketAddr;
use std::sync::Arc;

use axum::{
    Json, Router,
    extract::{ConnectInfo, Query, State},
    http::HeaderMap,
    routing::{get, post},
};
use serde::Deserialize;
use serde_json::{Value, json};

use crate::{
    cookie, redis_store, security, state::AppState,
    generated::{
        http_v1::{LogoutRequest, SendLoginCodeRequest, ConfirmLoginCodeRequest,
            SendLoginCodeResponse, ConfirmLoginCodeResponse,
            GodModeExchangeRequest},
        http_routes::{
            LOGOUT_PATH, SEND_LOGIN_CODE_PATH, CONFIRM_LOGIN_CODE_PATH,
            IP_INFO_PATH, SITE_CONFIG_PATH, GOD_MODE_EXCHANGE_PATH,
        },
    },
};

/// Decode JWT claims without signature verification (for logout jti extraction).
/// Safety: we only use this after the token was already verified by the auth middleware.
fn decode_jti_exp_unsafe(token: &str) -> Option<(String, i64)> {
    use jsonwebtoken::{Algorithm, DecodingKey, Validation, decode};
    use crate::token::Claims;
    // Use an all-zeros key — we skip signature validation intentionally
    let mut validation = Validation::new(Algorithm::RS256);
    validation.insecure_disable_signature_validation();
    validation.validate_aud = false;
    validation.validate_exp = false;
    let dummy_key = DecodingKey::from_secret(b"");
    decode::<Claims>(token, &dummy_key, &validation)
        .ok()
        .map(|d| (d.claims.jti, d.claims.exp))
}

pub fn router() -> Router<Arc<AppState>> {
    Router::new()
        .route(LOGOUT_PATH, post(logout))
        .route(SEND_LOGIN_CODE_PATH, post(send_login_code))
        .route(CONFIRM_LOGIN_CODE_PATH, post(confirm_login_code))
        .route(IP_INFO_PATH, get(ip_info))
        .route(SITE_CONFIG_PATH, get(site_config))
        .route(GOD_MODE_EXCHANGE_PATH, post(god_mode_exchange))
}

// ─── Query params ─────────────────────────────────────────────────────────────

#[derive(Deserialize)]
struct SiteConfigQuery {
    open_at: Option<String>,
}

// ─── IP helpers ───────────────────────────────────────────────────────────────

/// Extract the best client IP and raw header values from the request.
/// Returns (lookup_ip, remote_addr_str, x_forwarded_for_str, x_real_ip_str).
fn extract_ip(addr: SocketAddr, headers: &HeaderMap) -> (String, String, String, String) {
    let remote_ip = addr.ip().to_string();
    let x_forwarded_for = headers
        .get("x-forwarded-for")
        .and_then(|v| v.to_str().ok())
        .unwrap_or("NONE")
        .to_string();
    let x_real_ip = headers
        .get("x-real-ip")
        .and_then(|v| v.to_str().ok())
        .unwrap_or("NONE")
        .to_string();
    let lookup_ip = if x_forwarded_for != "NONE" {
        x_forwarded_for
            .split(',')
            .next()
            .map(|s| s.trim().to_string())
            .unwrap_or_else(|| remote_ip.clone())
    } else if x_real_ip != "NONE" {
        x_real_ip.clone()
    } else {
        remote_ip.clone()
    };
    (lookup_ip, remote_ip, x_forwarded_for, x_real_ip)
}

/// Call ipinfo.io for the given IP and return the parsed JSON.
/// Appends an `ips` field with local header info. Never fails — returns `{}` on error.
async fn fetch_ip_info(
    state: &AppState,
    lookup_ip: &str,
    remote_ip: &str,
    x_forwarded_for: &str,
    x_real_ip: &str,
) -> Value {
    let endpoint = state.ipinfo_endpoint.trim_end_matches('/');
    let url = if state.ipinfo_token.is_empty() {
        format!("{}/{}", endpoint, lookup_ip)
    } else {
        format!("{}/{}?token={}", endpoint, lookup_ip, state.ipinfo_token)
    };

    let mut result: Value = if let Ok(client) = reqwest::Client::builder()
        .timeout(std::time::Duration::from_secs(10))
        .build()
    {
        match client.get(&url).send().await {
            Ok(resp) => resp.json::<Value>().await.unwrap_or_else(|_| json!({})),
            Err(_) => json!({}),
        }
    } else {
        json!({})
    };

    result["ips"] = json!({
        "RemoteIpAddress": remote_ip,
        "X-Forwarded-For": x_forwarded_for,
        "X-Real-IP": x_real_ip,
    });
    result
}

// ─── GET /api/v1/auth/ip-info ─────────────────────────────────────────────────

async fn ip_info(
    State(state): State<Arc<AppState>>,
    ConnectInfo(addr): ConnectInfo<SocketAddr>,
    headers: HeaderMap,
) -> Json<Value> {
    let (lookup_ip, remote_ip, xff, xri) = extract_ip(addr, &headers);
    Json(fetch_ip_info(&state, &lookup_ip, &remote_ip, &xff, &xri).await)
}

// ─── GET /api/v1/auth/c ───────────────────────────────────────────────────────

async fn site_config(
    State(state): State<Arc<AppState>>,
    ConnectInfo(addr): ConnectInfo<SocketAddr>,
    headers: HeaderMap,
    Query(params): Query<SiteConfigQuery>,
) -> Json<Value> {
    let site_id = match params.open_at.as_deref() {
        Some(s) => open_at_to_site_id(s),
        None => {
            let (lookup_ip, remote_ip, xff, xri) = extract_ip(addr, &headers);
            let info = fetch_ip_info(&state, &lookup_ip, &remote_ip, &xff, &xri).await;
            let country = info["country"].as_str().unwrap_or("").to_uppercase();
            country_to_site_id(&country)
        }
    };
    Json(json!([site_id]))
}

fn open_at_to_site_id(open_at: &str) -> i32 {
    match open_at.to_uppercase().as_str() {
        "BVI" => 1,
        "BA"  => 2,
        "CN"  => 3,
        "TW"  => 4,
        "VN" | "SEA" => 5,
        "JP"  => 6,
        "MY"  => 8,
        _     => 1,
    }
}

fn country_to_site_id(country: &str) -> i32 {
    match country {
        "CN" => 3,
        "TW" => 4,
        "VN" => 5,
        "JP" => 6,
        "MN" => 7,
        "MY" => 8,
        "AU" => 2,
        "VG" => 1,
        _    => 0,
    }
}

// ─── God Mode Exchange ────────────────────────────────────────────────────────

/// POST /api/v2/auth/god-mode/exchange
///
/// Exchanges a one-time god-mode key (written by mono's EnableGodMode gRPC handler
/// into Redis as `godmode:key:{uuid}`) for an HttpOnly `access_token` cookie.
/// The key is consumed atomically (single-use, 60-second TTL set by mono).
async fn god_mode_exchange(
    State(state): State<Arc<AppState>>,
    Json(req): Json<GodModeExchangeRequest>,
) -> (HeaderMap, Json<Value>) {
    tracing::info!("god_mode_exchange requested");
    if req.key.is_empty() {
        tracing::warn!("god_mode_exchange: key is empty");
        return (HeaderMap::new(), Json(json!({ "error": "invalid_request", "error_description": "key is required" })));
    }

    let access_token = match redis_store::consume_godmode_key(&state.redis, &req.key).await {
        Ok(Some(t)) => t,
        Ok(None) => {
            tracing::warn!("god_mode_exchange: key invalid or expired");
            return (HeaderMap::new(), Json(json!({ "error": "invalid_grant", "error_description": "God-mode key is invalid or expired" })));
        }
        Err(e) => {
            tracing::error!(error = %e, "god_mode_exchange: redis error");
            return (HeaderMap::new(), Json(json!({ "error": "server_error" })));
        }
    };

    tracing::info!("god_mode_exchange: success");
    let mut headers = HeaderMap::new();
    cookie::set_token_cookie(&mut headers, &access_token, state.access_token_lifetime, state.secure_cookie);
    (headers, Json(json!(null)))
}

/// POST /api/v2/auth/logout
/// Clears the access_token cookie, deletes the refresh token from Redis,
/// and adds the current access token's jti to the blocklist so it cannot
/// be used even if someone still holds the cookie value.
async fn logout(
    State(state): State<Arc<AppState>>,
    headers: HeaderMap,
    body: Option<Json<LogoutRequest>>,
) -> (HeaderMap, Json<Value>) {
    tracing::info!("logout");

    // Extract access token from cookie and blocklist its jti
    let cookie_header = headers
        .get(axum::http::header::COOKIE)
        .and_then(|v| v.to_str().ok())
        .unwrap_or("");
    let access_token = cookie_header
        .split(';')
        .find_map(|part| {
            let part = part.trim();
            part.strip_prefix("access_token=")
        });

    if let Some(token) = access_token {
        if let Some((jti, exp)) = decode_jti_exp_unsafe(token) {
            let now = chrono::Utc::now().timestamp();
            let ttl = (exp - now).max(0) as u64;
            if ttl > 0 {
                if let Err(e) = redis_store::blocklist_jti(&state.redis, &jti, ttl).await {
                    redis_store::log_redis_error("blocklist_jti(logout)", e);
                }
            }
        }
    }

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
    tracing::info!(email, "send_login_code requested");

    // Lockout check (shared with password grant)
    if security::is_locked_out(&state.redis, &email).await {
        tracing::warn!(email, "send_login_code: user locked out");
        return Json(SendLoginCodeResponse { success: false, message: "Please contact customer service.".into() });
    }

    // Verify user exists in our DB
    let users = match crate::db::find_users_by_email(&state.pool, &email).await {
        Ok(u) => u,
        Err(e) => {
            tracing::error!(email, error = %e, "send_login_code: db error");
            return Json(SendLoginCodeResponse { success: false, message: "server_error".into() });
        }
    };
    if users.is_empty() {
        // Don't reveal user existence; return success anyway (security best practice)
        tracing::warn!(email, "send_login_code: user not found (returning success to avoid enumeration)");
        return Json(SendLoginCodeResponse { success: true, message: "Login code sent.".into() });
    }
    if users.len() > 1 {
        tracing::warn!(email, "send_login_code: multiple tenants, email login not allowed");
        return Json(SendLoginCodeResponse { success: false, message: "Email login not allowed for multi-tenant accounts".into() });
    }

    let user = &users[0];

    // Call mono gRPC to send the code
    if let Some(mono) = &state.mono_client {
        let mut client = mono.lock().await;
        crate::grpc::auth_client::send_auth_code(&mut client, user.tenant_id, &email, "Login").await;
    }

    tracing::info!(email, user_id = user.id, "send_login_code: code sent");
    Json(SendLoginCodeResponse { success: true, message: "Login code sent.".into() })
}

/// POST /api/v2/auth/token/code/confirm
/// Verifies the email code via mono gRPC, then issues JWT + cookie.
async fn confirm_login_code(
    State(state): State<Arc<AppState>>,
    Json(req): Json<ConfirmLoginCodeRequest>,
) -> (HeaderMap, Json<Value>) {
    let email = req.email.trim().to_lowercase();
    tracing::info!(email, "confirm_login_code requested");

    // Lockout check before any DB work
    if security::is_locked_out(&state.redis, &email).await {
        tracing::warn!(email, "confirm_login_code: user locked out");
        return (HeaderMap::new(), Json(json!({ "error": "__USER_IS_LOCKED_OUT__", "error_description": "Please contact customer service." })));
    }

    let users = match crate::db::find_users_by_email(&state.pool, &email).await {
        Ok(u) => u,
        Err(e) => {
            tracing::error!(email, error = %e, "confirm_login_code: db error");
            return (HeaderMap::new(), Json(json!({ "error": "server_error" })));
        }
    };
    if users.is_empty() {
        tracing::warn!(email, "confirm_login_code: user not found");
        return (HeaderMap::new(), Json(json!({ "error": "not_found", "error_description": "User not found." })));
    }
    if users.len() > 1 {
        tracing::warn!(email, "confirm_login_code: multiple tenants, email login not allowed");
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
        tracing::error!("confirm_login_code: MONO_GRPC_ADDR not configured");
        return (
            HeaderMap::new(),
            Json(json!({ "error": "server_error", "error_description": "MONO_GRPC_ADDR not configured" })),
        );
    };

    if !code_valid {
        let locked = security::record_failure(&state.redis, &email).await;
        if locked {
            tracing::warn!(email, user_id = user.id, "confirm_login_code: account locked after repeated failures");
            return (HeaderMap::new(), Json(json!({ "error": "__USER_IS_LOCKED_OUT__", "error_description": "Please contact customer service." })));
        }
        tracing::warn!(email, user_id = user.id, "confirm_login_code: invalid code");
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
        origin: None,
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

    tracing::info!(user_id = user.id, email, "email-code login success");
    let resp = ConfirmLoginCodeResponse {
        token_type: "Bearer".into(),
        expires_in: token_result.expires_in,
        refresh_token,
    };
    (headers, Json(json!(resp)))
}
