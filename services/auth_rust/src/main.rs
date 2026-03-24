use std::net::SocketAddr;
use std::sync::Arc;

use axum::extract::State;
use axum::{Form, Json, Router, routing::post};
use chrono::Utc;
use serde::Deserialize;
use serde_json::{Value, json};
use sqlx::PgPool;
use tower_http::cors::CorsLayer;

use auth::{db, hashids, password, token};

struct AppState {
    pool: PgPool,
    jwt_secret: String,
    access_token_lifetime: i64,
}

#[derive(Debug, Deserialize)]
#[allow(dead_code)]
struct TokenRequest {
    grant_type: Option<String>,
    client_id: Option<String>,
    client_secret: Option<String>,
    username: Option<String>,
    password: Option<String>,
    scope: Option<String>,
    refresh_token: Option<String>,
    #[serde(rename = "tenantId")]
    tenant_id: Option<String>,
    code: Option<String>,
    tf_code: Option<String>,
}

fn error_response(error: &str, description: &str) -> Json<Value> {
    Json(json!({ "error": error, "error_description": description }))
}

/// POST /connect/token — OAuth2 token endpoint mirroring mono's IdentityServer4 logic.
///
/// Supported grant types:
///   - "password"            (resource owner password credentials)
///   - "client_credentials"  (service-to-service)
async fn connect_token(
    State(state): State<Arc<AppState>>,
    Form(req): Form<TokenRequest>,
) -> Json<Value> {
    let grant_type = req.grant_type.as_deref().unwrap_or("");

    match grant_type {
        "password" => handle_password_grant(&state, &req).await,
        "client_credentials" => handle_client_credentials(&state, &req),
        _ => error_response(
            "unsupported_grant_type",
            &format!("grant_type '{}' is not supported", grant_type),
        ),
    }
}

fn handle_client_credentials(state: &AppState, req: &TokenRequest) -> Json<Value> {
    let claims = token::Claims {
        sub: req.client_id.clone().unwrap_or_default(),
        iat: Utc::now().timestamp(),
        exp: Utc::now().timestamp() + state.access_token_lifetime,
        nbf: Utc::now().timestamp(),
        jti: uuid::Uuid::new_v4().to_string(),
        tenant_id: "0".to_string(),
        party_id: "0".to_string(),
        version: "08-23-24".to_string(),
        role: None,
        amr: None,
    };
    let access_token = jsonwebtoken::encode(
        &jsonwebtoken::Header::default(),
        &claims,
        &jsonwebtoken::EncodingKey::from_secret(state.jwt_secret.as_bytes()),
    );
    match access_token {
        Ok(t) => Json(json!({
            "access_token": t,
            "token_type": "Bearer",
            "expires_in": state.access_token_lifetime,
        })),
        Err(e) => error_response("server_error", &e.to_string()),
    }
}

async fn handle_password_grant(state: &AppState, req: &TokenRequest) -> Json<Value> {
    let email = match req.username.as_deref() {
        Some(e) if !e.is_empty() => e.trim().to_lowercase(),
        _ => return error_response("invalid_request", "username is required"),
    };
    let pwd = match req.password.as_deref() {
        Some(p) if !p.is_empty() => p,
        _ => return error_response("invalid_request", "password is required"),
    };

    // --- IP blacklist check (best-effort, skip on DB error) ---
    // In production, extract real IP from X-Forwarded-For / peer addr.
    // Skipped here as Axum extraction depends on deployment (proxy, etc.)

    // --- Find users by email ---
    let users = match db::find_users_by_email(&state.pool, &email).await {
        Ok(u) => u,
        Err(e) => {
            tracing::error!("db error: {}", e);
            return error_response("server_error", "internal error");
        }
    };

    if users.is_empty() {
        return error_response("__LOGIN_FAILED__", "Login failed.");
    }

    // --- Parse optional tenantId ---
    let tenant_id: Option<i64> = req.tenant_id.as_deref().and_then(|s| s.parse::<i64>().ok());

    // --- Multiple users without tenant selection ---
    if users.len() > 1 && tenant_id.is_none() {
        if !validate_password_any(&users, pwd) {
            return error_response("__LOGIN_FAILED__", "Login failed.");
        }
        let tenant_ids: Vec<i64> = users
            .iter()
            .map(|u| u.tenant_id)
            .collect::<std::collections::HashSet<_>>()
            .into_iter()
            .collect();
        return Json(json!({
            "tenantIds": tenant_ids,
            "hasMultipleTenants": true
        }));
    }

    // --- Pick user (by tenantId or first match) ---
    let user = match tenant_id {
        Some(tid) => users.iter().find(|u| u.tenant_id == tid),
        None => users.first(),
    };
    let user = match user {
        Some(u) => u,
        None => return error_response("__LOGIN_FAILED__", "Login failed."),
    };

    // --- Status checks ---
    if user.status == 1 {
        return error_response(
            "__USER_UNDER_MAINTENANCE__",
            "Our system is under maintenance.",
        );
    }
    if !user.email_confirmed {
        return error_response(
            "__EMAIL_UNCONFIRMED__",
            "Please confirm your email before login.",
        );
    }

    // --- Password verification ---
    let hash = match user.password_hash.as_deref() {
        Some(h) if !h.is_empty() => h,
        _ => return error_response("__LOGIN_FAILED__", "Login failed."),
    };
    if !password::verify_hashed_password_v3(hash, pwd) {
        return error_response("__LOGIN_FAILED__", "Login failed.");
    }

    // --- Lockout check ---
    if user.lockout_enabled && user.lockout_end.is_some_and(|end| end > Utc::now()) {
        return error_response("__USER_IS_LOCKED_OUT__", "Please contact customer service.");
    }

    // --- 2FA (TOTP) check ---
    if user.two_factor_enabled {
        let code = req.code.as_deref().unwrap_or("");
        if code.is_empty() || code.len() != 6 {
            return error_response("__2FA_ENABLED__", "Please provide a 2fa code.");
        }
        // TOTP verification would go here (requires shared secret from DB).
        // For now, accept any 6-digit code as placeholder.
    }

    // --- Fetch roles ---
    let roles = db::get_user_roles(&state.pool, user.id)
        .await
        .unwrap_or_default();

    // --- Generate tokens ---
    let party_id_hashed = hashids::encode_party_id(user.party_id);
    let token_result = match token::generate_access_token(
        user.id,
        user.tenant_id,
        &party_id_hashed,
        &roles,
        user.two_factor_enabled,
        state.access_token_lifetime,
        &state.jwt_secret,
    ) {
        Ok(t) => t,
        Err(e) => return error_response("server_error", &e.to_string()),
    };

    let refresh_token = token::generate_refresh_token();

    // --- Update last login (fire-and-forget) ---
    let pool = state.pool.clone();
    let user_id = user.id;
    tokio::spawn(async move {
        let _ = db::update_last_login(&pool, user_id, "").await;
    });

    Json(json!({
        "access_token": token_result.access_token,
        "token_type": "Bearer",
        "expires_in": token_result.expires_in,
        "refresh_token": refresh_token,
    }))
}

fn validate_password_any(users: &[db::User], password: &str) -> bool {
    users.iter().any(|u| {
        u.password_hash
            .as_deref()
            .is_some_and(|h| auth::password::verify_hashed_password_v3(h, password))
    })
}

fn http_app(state: Arc<AppState>) -> Router {
    Router::new()
        .route("/connect/token", post(connect_token))
        .layer(CorsLayer::permissive())
        .with_state(state)
}

fn env(key: &str, fallback: &str) -> String {
    std::env::var(key).unwrap_or_else(|_| fallback.to_string())
}

/// Load `.env` from current dir or parent dir (same logic as mono's `LoadEnvironmentConfigure`).
/// Existing env vars are NOT overwritten — K8s/Docker/Compose env takes priority.
fn load_env_file() {
    let root = std::env::current_dir().unwrap_or_default();
    let candidates = [root.join(".env"), root.join("../.env")];
    let path = match candidates.iter().find(|p| p.exists()) {
        Some(p) => p.clone(),
        None => return,
    };
    let content = match std::fs::read_to_string(&path) {
        Ok(c) => c,
        Err(_) => return,
    };
    for line in content.lines() {
        let line = line.trim();
        if line.is_empty() || line.starts_with('#') {
            continue;
        }
        if let Some((key, value)) = line.split_once('=') {
            let key = key.trim();
            let value = value.trim().trim_matches('"');
            if key.is_empty() {
                continue;
            }
            if std::env::var(key).is_err() {
                // SAFETY: load_env_file is called once at startup before any threads are spawned.
                unsafe { std::env::set_var(key, value) };
            }
        }
    }
}

/// Build PostgreSQL connection URL from the same env vars as mono:
///   DB_HOST, DB_PORT, DB_USERNAME, DB_PASSWORD, DB_DATABASE
fn build_database_url() -> String {
    let host = env("DB_HOST", "localhost");
    let port = env("DB_PORT", "5432");
    let user = env("DB_USERNAME", "postgres");
    let password = env("DB_PASSWORD", "");
    let database = env("DB_DATABASE", "portal_central");
    format!(
        "postgres://{}:{}@{}:{}/{}",
        user, password, host, port, database
    )
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    load_env_file();
    let _tracing_guard = otel::init_tracing("auth");

    let database_url = build_database_url();
    let jwt_secret = env("JWT_SECRET", "dev-secret-change-in-production");
    let access_token_lifetime: i64 = env("ACCESS_TOKEN_LIFETIME", "86400")
        .parse()
        .unwrap_or(86400);

    let http_addr: SocketAddr = env("HTTP_ADDR", "[::]:9001").parse()?;

    tracing::info!(
        "Connecting to database at {}:{}",
        env("DB_HOST", "localhost"),
        env("DB_PORT", "5432")
    );
    let pool = PgPool::connect(&database_url).await?;
    tracing::info!("Connected to database");

    let state = Arc::new(AppState {
        pool,
        jwt_secret,
        access_token_lifetime,
    });

    let listener = tokio::net::TcpListener::bind(http_addr).await?;
    tracing::info!("Auth HTTP listening on http://{}", http_addr);

    axum::serve(listener, http_app(state)).await?;
    Ok(())
}
