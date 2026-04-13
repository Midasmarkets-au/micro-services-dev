use std::sync::Arc;

use axum::{Json, Router, extract::State, http::StatusCode, routing::post};
use serde_json::{Value, json};

use crate::{
    db, extractors::AuthUser, password, redis_store, state::AppState,
    generated::{
        http_v1::{
            SendPasswordResetCodeRequest, SendPasswordResetCodeResponse,
            ConfirmPasswordResetCodeRequest, ConfirmPasswordResetCodeResponse,
            ForgotPasswordRequest, ResetPasswordRequest, ChangePasswordRequest,
        },
        http_routes::{
            SEND_PASSWORD_RESET_CODE_PATH, CONFIRM_PASSWORD_RESET_CODE_PATH,
            FORGOT_PASSWORD_PATH, RESET_PASSWORD_PATH, CHANGE_PASSWORD_PATH,
        },
    },
};

pub fn router() -> Router<Arc<AppState>> {
    Router::new()
        .route(SEND_PASSWORD_RESET_CODE_PATH, post(send_password_reset_code))
        .route(CONFIRM_PASSWORD_RESET_CODE_PATH, post(confirm_password_reset_code))
        .route(FORGOT_PASSWORD_PATH, post(forgot_password))
        .route(RESET_PASSWORD_PATH, post(reset_password))
        .route(CHANGE_PASSWORD_PATH, post(change_password))
}

// ─── POST /api/v2/auth/password-reset/code ────────────────────────────────

async fn send_password_reset_code(
    State(state): State<Arc<AppState>>,
    Json(req): Json<SendPasswordResetCodeRequest>,
) -> Json<SendPasswordResetCodeResponse> {
    let email = req.email.trim().to_lowercase();

    let users = match db::find_users_by_email(&state.pool, &email).await {
        Ok(u) => u,
        Err(_) => return Json(SendPasswordResetCodeResponse { success: true }), // don't reveal errors
    };
    if users.is_empty() {
        return Json(SendPasswordResetCodeResponse { success: true }); // don't reveal user existence
    }

    let user = &users[0];
    if let Some(mono) = &state.mono_client {
        let mut client = mono.lock().await;
        crate::grpc::auth_client::send_auth_code(
            &mut client, user.tenant_id, &email, "ResetPassword",
        ).await;
    }

    Json(SendPasswordResetCodeResponse { success: true })
}

// ─── POST /api/v2/auth/password-reset/code/confirm ────────────────────────

async fn confirm_password_reset_code(
    State(state): State<Arc<AppState>>,
    Json(req): Json<ConfirmPasswordResetCodeRequest>,
) -> (StatusCode, Json<Value>) {
    let email = req.email.trim().to_lowercase();

    let users = match db::find_users_by_email(&state.pool, &email).await {
        Ok(u) => u,
        Err(_) => return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" }))),
    };
    if users.is_empty() {
        return (StatusCode::BAD_REQUEST, Json(json!({ "error": "not_found" })));
    }

    let code_valid = if let Some(mono) = &state.mono_client {
        let mut client = mono.lock().await;
        let (valid, _) = crate::grpc::auth_client::verify_auth_code(
            &mut client, users[0].tenant_id, &email, &req.code,
        ).await;
        valid
    } else {
        return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
    };

    if !code_valid {
        return (StatusCode::BAD_REQUEST, Json(json!({ "error": "invalid_code" })));
    }

    let hash = password::hash_password(&req.new_password);
    if let Err(e) = db::update_password_hash_by_email(&state.pool, &email, &hash).await {
        tracing::error!("update_password_hash_by_email failed: {}", e);
        return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
    }

    let resp = ConfirmPasswordResetCodeResponse { success: true };
    (StatusCode::OK, Json(json!(resp)))
}

// ─── POST /api/v2/auth/password/forgot ────────────────────────────────────

async fn forgot_password(
    State(state): State<Arc<AppState>>,
    Json(req): Json<ForgotPasswordRequest>,
) -> StatusCode {
    let email = req.email.trim().to_lowercase();

    let users = match db::find_users_by_email(&state.pool, &email).await {
        Ok(u) => u,
        Err(_) => return StatusCode::NO_CONTENT, // don't reveal errors
    };
    if users.is_empty() {
        return StatusCode::NO_CONTENT; // don't reveal user existence
    }

    let reset_token = uuid::Uuid::new_v4().to_string();
    if let Err(e) = redis_store::store_password_reset_token(&state.redis, &reset_token, &email).await {
        tracing::error!("store_password_reset_token failed: {}", e);
        return StatusCode::NO_CONTENT;
    }

    if let Some(mono) = &state.mono_client {
        let mut client = mono.lock().await;
        crate::grpc::auth_client::send_password_reset_email(
            &mut client, users[0].tenant_id, &email, &reset_token, &req.reset_url,
        ).await;
    }

    StatusCode::NO_CONTENT
}

// ─── POST /api/v2/auth/password/reset ─────────────────────────────────────

async fn reset_password(
    State(state): State<Arc<AppState>>,
    Json(req): Json<ResetPasswordRequest>,
) -> (StatusCode, Json<Value>) {
    let email = req.email.trim().to_lowercase();

    let stored_email = match redis_store::consume_password_reset_token(&state.redis, &req.code).await {
        Ok(Some(e)) => e,
        Ok(None) => return (StatusCode::BAD_REQUEST, Json(json!({ "error": "invalid_or_expired_code" }))),
        Err(_) => return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" }))),
    };

    if stored_email != email {
        return (StatusCode::BAD_REQUEST, Json(json!({ "error": "invalid_or_expired_code" })));
    }

    let users = match db::find_users_by_email(&state.pool, &email).await {
        Ok(u) => u,
        Err(_) => return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" }))),
    };
    if users.is_empty() {
        return (StatusCode::BAD_REQUEST, Json(json!({ "error": "not_found" })));
    }

    let hash = password::hash_password(&req.new_password);
    if let Err(e) = db::update_password_hash_by_email(&state.pool, &email, &hash).await {
        tracing::error!("update_password_hash_by_email failed: {}", e);
        return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
    }

    (StatusCode::NO_CONTENT, Json(json!(null)))
}

// ─── POST /api/v2/auth/password/change ────────────────────────────────────

async fn change_password(
    State(state): State<Arc<AppState>>,
    auth_user: AuthUser,
    Json(req): Json<ChangePasswordRequest>,
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

    if !password::verify_hashed_password_v3(hash, &req.current_password) {
        return (StatusCode::BAD_REQUEST, Json(json!({ "error": "invalid_password" })));
    }

    let new_hash = password::hash_password(&req.new_password);
    let email = user.email.as_deref().unwrap_or("").to_lowercase();
    if let Err(e) = db::update_password_hash_by_email(&state.pool, &email, &new_hash).await {
        tracing::error!("update_password_hash_by_email failed: {}", e);
        return (StatusCode::INTERNAL_SERVER_ERROR, Json(json!({ "error": "server_error" })));
    }

    (StatusCode::NO_CONTENT, Json(json!(null)))
}
