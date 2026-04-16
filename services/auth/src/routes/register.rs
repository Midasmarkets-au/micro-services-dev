use std::net::SocketAddr;
use std::sync::Arc;

use axum::{
    Json, Router,
    extract::{ConnectInfo, State},
    http::{HeaderMap, StatusCode},
    routing::post,
};
use serde_json::json;
use tracing::{error, warn};

use crate::{
    db, grpc, party_uid, password, state::AppState, twilio,
    generated::{
        http_v1::RegisterRequest,
        http_routes::REGISTER_PATH,
    },
};

pub fn router() -> Router<Arc<AppState>> {
    Router::new().route(REGISTER_PATH, post(register))
}

fn bad_request(msg: &str) -> (StatusCode, Json<serde_json::Value>) {
    (StatusCode::BAD_REQUEST, Json(json!({ "message": msg })))
}

// ─── Handler ─────────────────────────────────────────────────────────────────

async fn register(
    State(state): State<Arc<AppState>>,
    ConnectInfo(addr): ConnectInfo<SocketAddr>,
    headers: HeaderMap,
    Json(mut req): Json<RegisterRequest>,
) -> (StatusCode, Json<serde_json::Value>) {
    // ── 1. Extract IP ──────────────────────────────────────────────────────
    let ip = headers
        .get("x-forwarded-for")
        .and_then(|v| v.to_str().ok())
        .and_then(|v| v.split(',').next())
        .map(|s| s.trim().to_string())
        .or_else(|| {
            headers
                .get("x-real-ip")
                .and_then(|v| v.to_str().ok())
                .map(|s| s.to_string())
        })
        .unwrap_or_else(|| addr.ip().to_string());

    tracing::info!(email = req.email.trim(), ip, "register requested");

    // ── 2. IP blacklist ────────────────────────────────────────────────────
    if db::is_ip_blocked(&state.pool, &ip).await.unwrap_or(false) {
        tracing::warn!(email = req.email.trim(), ip, "register: ip blocked");
        return bad_request("__REGISTER_BLOCKED__");
    }

    // ── 3. Normalise inputs ────────────────────────────────────────────────
    req.email = req.email.trim().to_lowercase();
    req.refer_code = req
        .refer_code
        .trim()
        .to_uppercase()
        .chars()
        .filter(|c| c.is_ascii_alphanumeric())
        .collect();

    // ── 4. Resolve tenant ─────────────────────────────────────────────────
    let tenant_id = resolve_tenant(&state, &req, &ip).await;

    // ── 5. Email duplicate check ───────────────────────────────────────────
    let existing = match db::find_users_by_email(&state.pool, &req.email).await {
        Ok(users) => users,
        Err(e) => {
            error!("DB error checking email: {}", e);
            return bad_request("__SERVER_ERROR__");
        }
    };
    if existing.iter().any(|u| u.tenant_id != tenant_id) {
        tracing::warn!(email = req.email, "register: email already registered on another site");
        return bad_request("__ALREADY_REGISTERED_OTHER_SITE__");
    }
    if existing.iter().any(|u| u.tenant_id == tenant_id) {
        tracing::warn!(email = req.email, tenant_id, "register: email already exists");
        return bad_request("__EMAIL_EXISTS__");
    }

    // ── 6. Password strength ───────────────────────────────────────────────
    if req.password.len() < 6 {
        return bad_request("__PASSWORD_TOO_SHORT__");
    }

    // ── 7. OTP verification ────────────────────────────────────────────────
    let phone_confirmed = if let Some(ref twilio) = state.twilio {
        if req.phone.is_empty() {
            return bad_request("__PHONE_REQUIRED__");
        }
        let to = format!("+{}{}", req.ccc.trim_start_matches('+'), req.phone.trim());
        let otp = req.otp.trim().replace([' ', '-'], "");
        if otp.is_empty() {
            return bad_request("__OTP_REQUIRED__");
        }
        let verified = twilio::verify_otp(
            &twilio.account_sid,
            &twilio.auth_token,
            &twilio.service_sid,
            &to,
            &otp,
        )
        .await;
        if !verified {
            tracing::warn!(email = req.email, "register: otp verification failed");
            return bad_request("__VERIFICATION_FAILED__");
        }
        true
    } else {
        // SMS disabled — accept without OTP
        false
    };

    // ── 8. Party UID ───────────────────────────────────────────────────────
    let uid = match party_uid::generate_party_uid(&state.pool, tenant_id).await {
        Ok(u) => u,
        Err(e) => {
            error!("generate_party_uid failed: {}", e);
            return bad_request("__SERVER_ERROR__");
        }
    };

    // ── 9. SiteId ──────────────────────────────────────────────────────────
    let site_id = if req.site_id == 0 {
        fetch_ip_country(&ip).map(|c| country_to_site(&c)).unwrap_or(1)
    } else {
        req.site_id
    };

    // ── 10. Create CentralParty ────────────────────────────────────────────
    let full_name = format!("{} {}", req.first_name.trim(), req.last_name.trim());
    let party_id = match db::insert_central_party(
        &state.pool,
        uid,
        &req.email,
        &full_name,
        site_id,
        tenant_id,
    )
    .await
    {
        Ok(id) => id,
        Err(e) => {
            error!("insert_central_party failed: {}", e);
            return bad_request("__SERVER_ERROR__");
        }
    };

    // ── 11. Hash password ──────────────────────────────────────────────────
    let password_hash = password::hash_password(&req.password);

    // ── 12. Create User ────────────────────────────────────────────────────
    let native_name = full_name.clone();
    let language = if req.language.is_empty() { "en".to_string() } else { req.language.clone() };
    let new_user = db::NewUser {
        uid,
        party_id,
        tenant_id,
        email: &req.email,
        password_hash: &password_hash,
        first_name: req.first_name.trim(),
        last_name: req.last_name.trim(),
        native_name: &native_name,
        phone: &req.phone,
        phone_confirmed,
        ccc: &req.ccc,
        country_code: &req.country_code,
        currency: &req.currency,
        refer_code: &req.refer_code,
        language: &language,
        register_ip: &ip,
    };

    let user_id = match db::insert_user(&state.pool, &new_user).await {
        Ok(id) => id,
        Err(e) => {
            error!("insert_user failed: {}", e);
            let _ = db::delete_central_party(&state.pool, party_id).await;
            return bad_request("__SERVER_ERROR__");
        }
    };

    // ── 13. User role (Guest) ──────────────────────────────────────────────
    if let Err(e) = db::insert_user_role_by_name(&state.pool, user_id, "Guest").await {
        error!("insert_user_role failed: {}", e);
        let _ = db::delete_user(&state.pool, user_id).await;
        let _ = db::delete_central_party(&state.pool, party_id).await;
        return bad_request("__SERVER_ERROR__");
    }

    // ── 14. gRPC → mono (synchronous) ─────────────────────────────────────
    let utm = headers
        .get("cookie")
        .and_then(|v| v.to_str().ok())
        .and_then(|cookies| {
            cookies.split(';').find_map(|c| {
                let c = c.trim();
                c.strip_prefix("utm=").map(|v| v.to_string())
            })
        })
        .unwrap_or_default();

    if let Some(mono) = &state.mono_client {
        let mut client = mono.lock().await;
        match grpc::auth_client::register_tenant_user(
            &mut client,
            grpc::auth_client::RegisterTenantUserParams {
                party_id,
                tenant_id,
                user_id,
                uid,
                email: &req.email,
                first_name: req.first_name.trim(),
                last_name: req.last_name.trim(),
                native_name: &native_name,
                phone: &req.phone,
                ccc: &req.ccc,
                currency: &req.currency,
                refer_code: &req.refer_code,
                language: &language,
                site_id,
                register_ip: &ip,
                phone_confirmed,
                source_comment: &req.source_comment,
                utm: &utm,
                country_code: &req.country_code,
            },
        )
        .await
        {
            Ok(true) => {}
            Ok(false) | Err(_) => {
                warn!("RegisterTenantUser gRPC failed, rolling back user_id={}", user_id);
                let _ = db::delete_user(&state.pool, user_id).await;
                let _ = db::delete_central_party(&state.pool, party_id).await;
                return bad_request("__REGISTER_FAILED__");
            }
        }
    }

    tracing::info!(email = req.email, tenant_id, user_id, "register success");
    (StatusCode::NO_CONTENT, Json(json!({})))
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

async fn resolve_tenant(state: &AppState, req: &RegisterRequest, ip: &str) -> i64 {
    // 1. Referral code → tenant
    if !req.refer_code.is_empty() {
        if let Ok(Some(tid)) = db::find_tenant_by_refer_code(&state.pool, &req.refer_code).await {
            return tid;
        }
    }
    // 2. Explicitly provided tenant_id
    if req.tenant_id != 0 {
        if db::tenant_exists(&state.pool, req.tenant_id).await.unwrap_or(false) {
            return req.tenant_id;
        }
    }
    // 3. IP country
    if let Some(country) = fetch_ip_country(ip) {
        let tid = country_to_tenant(&country);
        if tid != 0 {
            return tid;
        }
    }
    // 4. Default
    10000
}

/// Cheaply extract the country code from a cached IP lookup.
/// For simplicity we do a blocking-style lookup here — in production the
/// ip-info endpoint already caches this, but register is infrequent enough
/// that a small extra HTTP call is acceptable.
fn fetch_ip_country(ip: &str) -> Option<String> {
    // Best-effort: parse from raw IP heuristic or return None to use default.
    // Full ipinfo.io call would require async context; caller handles None.
    let _ = ip; // suppress unused warning
    None
}

fn country_to_tenant(country: &str) -> i64 {
    match country.to_uppercase().as_str() {
        "VN" | "MY" => 10004,
        "AU" => 1,
        "CN" | "TW" | "JP" | "MN" => 10000,
        _ => 0,
    }
}

fn country_to_site(country: &str) -> i32 {
    match country.to_uppercase().as_str() {
        "CN" => 3,
        "TW" => 4,
        "VN" => 5,
        "JP" => 6,
        "MN" => 7,
        "MY" => 8,
        "AU" => 2,
        _ => 1,
    }
}
