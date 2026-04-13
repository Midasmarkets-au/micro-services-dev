use std::sync::Arc;

use axum::{Json, Router, extract::State, http::StatusCode, routing};
use serde::Deserialize;
use serde_json::{Value, json};

use crate::{db, extractors::AuthUser, password, state::AppState, totp};

pub fn router() -> Router<Arc<AppState>> {
    Router::new()
        .route("/api/v1/2fa/authenticator/setup", routing::get(setup_authenticator))
        .route("/api/v1/2fa/authenticator/verify", routing::post(verify_authenticator))
        .route("/api/v1/2fa/enable", routing::put(enable_2fa))
        .route("/api/v1/2fa/disable", routing::put(disable_2fa))
}

// ─── GET /api/v1/2fa/authenticator/setup ──────────────────────────────────

async fn setup_authenticator(
    State(state): State<Arc<AppState>>,
    auth_user: AuthUser,
) -> (StatusCode, Json<Value>) {
    let key = match db::get_authenticator_key(&state.pool, auth_user.user_id).await {
        Ok(Some(k)) => k,
        Ok(None) => {
            // Generate and persist a new secret
            let new_key = totp::generate_secret();
            if let Err(e) = db::upsert_authenticator_key(&state.pool, auth_user.user_id, &new_key).await {
                tracing::error!("upsert_authenticator_key failed: {}", e);
                return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
            }
            new_key
        }
        Err(e) => {
            tracing::error!("get_authenticator_key failed: {}", e);
            return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
        }
    };

    let qr_uri = totp::totp_uri(&key, &auth_user.email);
    (StatusCode::OK, Json(json!({ "key": key, "qr_uri": qr_uri })))
}

// ─── POST /api/v1/2fa/authenticator/verify ────────────────────────────────

#[derive(Deserialize)]
struct VerifyTotpRequest {
    code: String,
}

async fn verify_authenticator(
    State(state): State<Arc<AppState>>,
    auth_user: AuthUser,
    Json(req): Json<VerifyTotpRequest>,
) -> (StatusCode, Json<Value>) {
    let key = match db::get_authenticator_key(&state.pool, auth_user.user_id).await {
        Ok(Some(k)) => k,
        Ok(None) => return (StatusCode::BAD_REQUEST, Json(json!({ "error": "authenticator_not_setup" }))),
        Err(e) => {
            tracing::error!("get_authenticator_key failed: {}", e);
            return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
        }
    };

    let code = req.code.trim().replace([' ', '-'], "");
    if !totp::verify_totp(&key, &code) {
        return (StatusCode::BAD_REQUEST, Json(json!({ "error": "invalid_code" })));
    }

    if let Err(e) = db::update_two_factor_enabled(&state.pool, auth_user.user_id, true).await {
        tracing::error!("update_two_factor_enabled failed: {}", e);
        return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
    }

    (StatusCode::OK, Json(json!({ "success": true })))
}

// ─── PUT /api/v1/2fa/enable ───────────────────────────────────────────────

async fn enable_2fa(
    State(state): State<Arc<AppState>>,
    auth_user: AuthUser,
) -> (StatusCode, Json<Value>) {
    // Must have a key set up first
    match db::get_authenticator_key(&state.pool, auth_user.user_id).await {
        Ok(Some(_)) => {}
        Ok(None) => return (StatusCode::BAD_REQUEST, Json(json!({ "error": "authenticator_not_setup" }))),
        Err(e) => {
            tracing::error!("get_authenticator_key failed: {}", e);
            return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
        }
    }

    if let Err(e) = db::update_two_factor_enabled(&state.pool, auth_user.user_id, true).await {
        tracing::error!("update_two_factor_enabled failed: {}", e);
        return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
    }

    (StatusCode::NO_CONTENT, Json(json!(null)))
}

// ─── PUT /api/v1/2fa/disable ──────────────────────────────────────────────

#[derive(Deserialize)]
struct DisableTwoFaRequest {
    password: String,
}

async fn disable_2fa(
    State(state): State<Arc<AppState>>,
    auth_user: AuthUser,
    Json(req): Json<DisableTwoFaRequest>,
) -> (StatusCode, Json<Value>) {
    let user = match db::find_user_by_id(&state.pool, auth_user.user_id).await {
        Ok(Some(u)) => u,
        Ok(None) => return (StatusCode::UNAUTHORIZED, Json(json!({ "error": "not_found" }))),
        Err(_) => return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" }))),
    };

    let hash = match user.password_hash.as_deref() {
        Some(h) if !h.is_empty() => h,
        _ => return (StatusCode::BAD_REQUEST, Json(json!({ "error": "no_password_set" }))),
    };

    if !password::verify_hashed_password_v3(hash, &req.password) {
        return (StatusCode::BAD_REQUEST, Json(json!({ "error": "invalid_password" })));
    }

    if let Err(e) = db::update_two_factor_enabled(&state.pool, auth_user.user_id, false).await {
        tracing::error!("update_two_factor_enabled failed: {}", e);
        return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
    }

    (StatusCode::NO_CONTENT, Json(json!(null)))
}
