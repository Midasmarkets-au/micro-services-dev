use std::sync::Arc;

use axum::{Json, Router, extract::State, http::StatusCode, routing};
use serde_json::{Value, json};

use crate::{
    db, extractors::AuthUser, password, state::AppState, totp,
    generated::{
        http_v1::{
            VerifyAuthenticatorRequest, Disable2FaRequest,
            SetupAuthenticatorResponse, VerifyAuthenticatorResponse,
        },
        http_routes::{
            SETUP_AUTHENTICATOR_PATH, VERIFY_AUTHENTICATOR_PATH,
            ENABLE_2FA_PATH, DISABLE_2FA_PATH,
        },
    },
};

pub fn router() -> Router<Arc<AppState>> {
    Router::new()
        .route(SETUP_AUTHENTICATOR_PATH, routing::get(setup_authenticator))
        .route(VERIFY_AUTHENTICATOR_PATH, routing::post(verify_authenticator))
        .route(ENABLE_2FA_PATH, routing::put(enable_2fa))
        .route(DISABLE_2FA_PATH, routing::put(disable_2fa))
}

// ─── GET /api/v1/2fa/authenticator/setup ──────────────────────────────────

async fn setup_authenticator(
    State(state): State<Arc<AppState>>,
    auth_user: AuthUser,
) -> (StatusCode, Json<Value>) {
    tracing::info!(user_id = auth_user.user_id, "setup_authenticator requested");
    let key = match db::get_authenticator_key(&state.pool, auth_user.user_id).await {
        Ok(Some(k)) => k,
        Ok(None) => {
            let new_key = totp::generate_secret();
            if let Err(e) = db::upsert_authenticator_key(&state.pool, auth_user.user_id, &new_key).await {
                tracing::error!(user_id = auth_user.user_id, error = %e, "setup_authenticator: upsert key failed");
                return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
            }
            new_key
        }
        Err(e) => {
            tracing::error!(user_id = auth_user.user_id, error = %e, "setup_authenticator: get key failed");
            return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
        }
    };

    tracing::info!(user_id = auth_user.user_id, "setup_authenticator: key ready");
    let qr_uri = totp::totp_uri(&key, &auth_user.email);
    let resp = SetupAuthenticatorResponse { key, qr_uri };
    (StatusCode::OK, Json(json!(resp)))
}

// ─── POST /api/v1/2fa/authenticator/verify ────────────────────────────────

async fn verify_authenticator(
    State(state): State<Arc<AppState>>,
    auth_user: AuthUser,
    Json(req): Json<VerifyAuthenticatorRequest>,
) -> (StatusCode, Json<Value>) {
    tracing::info!(user_id = auth_user.user_id, "verify_authenticator requested");
    let key = match db::get_authenticator_key(&state.pool, auth_user.user_id).await {
        Ok(Some(k)) => k,
        Ok(None) => {
            tracing::warn!(user_id = auth_user.user_id, "verify_authenticator: authenticator not set up");
            return (StatusCode::BAD_REQUEST, Json(json!({ "error": "authenticator_not_setup" })));
        }
        Err(e) => {
            tracing::error!(user_id = auth_user.user_id, error = %e, "verify_authenticator: get key failed");
            return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
        }
    };

    let code = req.code.trim().replace([' ', '-'], "");
    if !totp::verify_totp(&key, &code) {
        tracing::warn!(user_id = auth_user.user_id, "verify_authenticator: invalid code");
        return (StatusCode::BAD_REQUEST, Json(json!({ "error": "invalid_code" })));
    }

    if let Err(e) = db::update_two_factor_enabled(&state.pool, auth_user.user_id, true).await {
        tracing::error!(user_id = auth_user.user_id, error = %e, "verify_authenticator: update 2fa flag failed");
        return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
    }

    tracing::info!(user_id = auth_user.user_id, "verify_authenticator: 2fa enabled");
    let resp = VerifyAuthenticatorResponse { success: true };
    (StatusCode::OK, Json(json!(resp)))
}

// ─── PUT /api/v1/2fa/enable ───────────────────────────────────────────────

async fn enable_2fa(
    State(state): State<Arc<AppState>>,
    auth_user: AuthUser,
) -> (StatusCode, Json<Value>) {
    tracing::info!(user_id = auth_user.user_id, "enable_2fa requested");
    match db::get_authenticator_key(&state.pool, auth_user.user_id).await {
        Ok(Some(_)) => {}
        Ok(None) => {
            tracing::warn!(user_id = auth_user.user_id, "enable_2fa: authenticator not set up");
            return (StatusCode::BAD_REQUEST, Json(json!({ "error": "authenticator_not_setup" })));
        }
        Err(e) => {
            tracing::error!(user_id = auth_user.user_id, error = %e, "enable_2fa: get key failed");
            return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
        }
    }

    if let Err(e) = db::update_two_factor_enabled(&state.pool, auth_user.user_id, true).await {
        tracing::error!(user_id = auth_user.user_id, error = %e, "enable_2fa: update flag failed");
        return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
    }

    tracing::info!(user_id = auth_user.user_id, "enable_2fa: success");
    (StatusCode::NO_CONTENT, Json(json!(null)))
}

// ─── PUT /api/v1/2fa/disable ──────────────────────────────────────────────

async fn disable_2fa(
    State(state): State<Arc<AppState>>,
    auth_user: AuthUser,
    Json(req): Json<Disable2FaRequest>,
) -> (StatusCode, Json<Value>) {
    tracing::info!(user_id = auth_user.user_id, "disable_2fa requested");
    let user = match db::find_user_by_id(&state.pool, auth_user.user_id).await {
        Ok(Some(u)) => u,
        Ok(None) => {
            tracing::warn!(user_id = auth_user.user_id, "disable_2fa: user not found");
            return (StatusCode::UNAUTHORIZED, Json(json!({ "error": "not_found" })));
        }
        Err(e) => {
            tracing::error!(user_id = auth_user.user_id, error = %e, "disable_2fa: db error");
            return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
        }
    };

    let hash = match user.password_hash.as_deref() {
        Some(h) if !h.is_empty() => h,
        _ => {
            tracing::warn!(user_id = auth_user.user_id, "disable_2fa: no password set");
            return (StatusCode::BAD_REQUEST, Json(json!({ "error": "no_password_set" })));
        }
    };

    if !password::verify_hashed_password_v3(hash, &req.password) {
        tracing::warn!(user_id = auth_user.user_id, "disable_2fa: invalid password");
        return (StatusCode::BAD_REQUEST, Json(json!({ "error": "invalid_password" })));
    }

    if let Err(e) = db::update_two_factor_enabled(&state.pool, auth_user.user_id, false).await {
        tracing::error!(user_id = auth_user.user_id, error = %e, "disable_2fa: update flag failed");
        return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
    }

    tracing::info!(user_id = auth_user.user_id, "disable_2fa: success");
    (StatusCode::NO_CONTENT, Json(json!(null)))
}
