use std::net::SocketAddr;
use std::sync::Arc;

use axum::extract::State;
use axum::http::{HeaderMap, StatusCode};
use axum::response::{IntoResponse, Response};
use axum::{Form, Json, Router, routing::get, routing::post};
use chrono::Utc;
use serde::Deserialize;
use serde_json::json;
use sqlx::PgPool;
use tower_http::cors::CorsLayer;
use tracing::{error, info};

use auth::{
    cookie, db, grpc, hashids,
    keys::{Jwks, RsaKeyPair},
    password, redis_store, routes, security,
    state::AppState,
    token,
};


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

fn error_response(error: &str, description: &str) -> Response {
    (
        StatusCode::BAD_REQUEST,
        Json(json!({ "error": error, "error_description": description })),
    ).into_response()
}

/// POST /connect/token — OAuth2 token endpoint.
///
/// On success, the access token is delivered via HttpOnly cookie only;
/// the JSON body contains token_type and expires_in but NOT the token itself
/// (mirrors mono's ApplyTokenResponseHandler behaviour).
async fn connect_token(
    State(state): State<Arc<AppState>>,
    axum::extract::ConnectInfo(addr): axum::extract::ConnectInfo<SocketAddr>,
    headers: axum::http::HeaderMap,
    Form(req): Form<TokenRequest>,
) -> Response {
    let grant_type = req.grant_type.as_deref().unwrap_or("");
    info!(
        grant_type,
        username = req.username.as_deref().unwrap_or(""),
        client_id = req.client_id.as_deref().unwrap_or(""),
        "POST /connect/token"
    );

    let user_agent = headers
        .get("user-agent")
        .and_then(|v| v.to_str().ok())
        .unwrap_or("")
        .to_string();
    let referer = headers
        .get("referer")
        .and_then(|v| v.to_str().ok())
        .unwrap_or("")
        .to_string();
    let ip = headers
        .get("x-forwarded-for")
        .and_then(|v| v.to_str().ok())
        .and_then(|v| v.split(',').next())
        .map(|s| s.trim().to_string())
        .unwrap_or_else(|| addr.ip().to_string());

    match grant_type {
        "password" => handle_password_grant(&state, &req, &ip, &user_agent, &referer).await,
        "client_credentials" => handle_client_credentials(&state, &req),
        "refresh_token" => handle_refresh_grant(&state, &req).await,
        _ => error_response(
            "unsupported_grant_type",
            &format!("grant_type '{}' is not supported", grant_type),
        ),
    }
}

fn handle_client_credentials(state: &AppState, req: &TokenRequest) -> Response {
    let client_id = req.client_id.clone().unwrap_or_default();
    let params = token::TokenParams {
        user_id: 0,
        tenant_id: 0,
        party_id_hashed: &client_id,
        god_party_id: 0,
        display_name: &client_id,
        email: "",
        roles: &["service".to_string()],
        two_factor_enabled: false,
        lifetime_secs: state.access_token_lifetime,
        user_agent_hash: None,
        sales_account: None,
        agent_account: None,
        rep_account: None,
    };
    match token::generate_access_token(&params, &state.key_pair.private_der, &state.key_pair.kid) {
        Ok(t) => {
            let mut headers = HeaderMap::new();
            cookie::set_token_cookie(&mut headers, &t.access_token, t.expires_in, state.secure_cookie);
            (
                headers,
                Json(json!({
                    "token_type": "Bearer",
                    "expires_in": t.expires_in,
                })),
            ).into_response()
        }
        Err(e) => {
            tracing::error!("generate_access_token failed: {}", e);
            error_response("server_error", &format!("RSA key invalid: {}", e))
        }
    }
}

async fn handle_refresh_grant(state: &AppState, req: &TokenRequest) -> Response {
    let refresh_token = match req.refresh_token.as_deref() {
        Some(t) if !t.is_empty() => t,
        _ => return error_response("invalid_request", "refresh_token is required"),
    };

    let data = match redis_store::consume_refresh_token(&state.redis, refresh_token).await {
        Ok(Some(d)) => d,
        Ok(None) => return error_response("invalid_grant", "refresh token is invalid or expired"),
        Err(e) => {
            error!("Redis error on refresh: {}", e);
            return error_response("server_error", "internal error");
        }
    };

    // Reload user to re-check status
    let user = match db::find_user_by_id(&state.pool, data.user_id).await {
        Ok(Some(u)) => u,
        Ok(None) => return error_response("invalid_grant", "user not found"),
        Err(e) => {
            error!("db error on refresh: {}", e);
            return error_response("server_error", "internal error");
        }
    };

    if user.status == 1 {
        return error_response("__USER_UNDER_MAINTENANCE__", "Our system is under maintenance.");
    }
    if user.lockout_enabled && user.lockout_end.is_some_and(|end| end > Utc::now()) {
        return error_response("__USER_IS_LOCKED_OUT__", "Please contact customer service.");
    }

    let roles = db::get_user_roles(&state.pool, user.id).await.unwrap_or_default();
    let party_id_hashed = hashids::encode_party_id(user.party_id);
    let display_name = user.guess_display_name();

    let params = token::TokenParams {
        user_id: user.id,
        tenant_id: user.tenant_id,
        party_id_hashed: &party_id_hashed,
        god_party_id: 0,
        display_name: &display_name,
        email: user.email.as_deref().unwrap_or(""),
        roles: &roles,
        two_factor_enabled: user.two_factor_enabled,
        lifetime_secs: state.access_token_lifetime,
        user_agent_hash: None,
        sales_account: None,
        agent_account: None,
        rep_account: None,
    };

    let token_result = match token::generate_access_token(
        &params,
        &state.key_pair.private_der,
        &state.key_pair.kid,
    ) {
        Ok(t) => t,
        Err(e) => return error_response("server_error", &e.to_string()),
    };

    let new_refresh = token::generate_refresh_token();
    let store_data = redis_store::RefreshTokenData {
        user_id: user.id,
        tenant_id: user.tenant_id,
        party_id: party_id_hashed.clone(),
    };
    if let Err(e) = redis_store::store_refresh_token(&state.redis, &new_refresh, &store_data).await {
        redis_store::log_redis_error("store_refresh_token", e);
    }

    let mut headers = HeaderMap::new();
    cookie::set_token_cookie(&mut headers, &token_result.access_token, token_result.expires_in, state.secure_cookie);

    (
        headers,
        Json(json!({
            "token_type": "Bearer",
            "expires_in": token_result.expires_in,
            "refresh_token": new_refresh,
        })),
    ).into_response()
}

async fn handle_password_grant(
    state: &AppState,
    req: &TokenRequest,
    ip: &str,
    user_agent: &str,
    referer: &str,
) -> Response {
    let email = match req.username.as_deref() {
        Some(e) if !e.is_empty() => e.trim().to_lowercase(),
        _ => return error_response("invalid_request", "username is required"),
    };
    let pwd = match req.password.as_deref() {
        Some(p) if !p.is_empty() => p,
        _ => return error_response("invalid_request", "password is required"),
    };

    // IP blacklist check
    if db::is_ip_blocked(&state.pool, ip).await.unwrap_or(false) {
        return error_response("access_denied", "__LOGIN_FAILED__");
    }

    // Redis-based lockout check
    if security::is_locked_out(&state.redis, &email).await {
        return error_response("__USER_IS_LOCKED_OUT__", "Please contact customer service.");
    }

    let users = match db::find_users_by_email(&state.pool, &email).await {
        Ok(u) => u,
        Err(e) => {
            error!("db error finding users: {}", e);
            return error_response("server_error", "internal error");
        }
    };

    if users.is_empty() {
        return error_response("__LOGIN_FAILED__", "Login failed.");
    }

    let tenant_id: Option<i64> = req.tenant_id.as_deref().and_then(|s| s.parse::<i64>().ok());

    // Multiple tenants — return picker response
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
        return (
            HeaderMap::new(),
            Json(json!({ "tenantIds": tenant_ids, "hasMultipleTenants": true })),
        ).into_response();
    }

    let user = match tenant_id {
        Some(tid) => users.iter().find(|u| u.tenant_id == tid),
        None => users.first(),
    };
    let user = match user {
        Some(u) => u,
        None => return error_response("__LOGIN_FAILED__", "Login failed."),
    };

    if user.status == 1 {
        return error_response("__USER_UNDER_MAINTENANCE__", "Our system is under maintenance.");
    }
    if !user.email_confirmed {
        return error_response("__EMAIL_UNCONFIRMED__", "Please confirm your email before login.");
    }

    let hash = match user.password_hash.as_deref() {
        Some(h) if !h.is_empty() => h,
        _ => return error_response("__LOGIN_FAILED__", "Login failed."),
    };
    if !password::verify_hashed_password_v3(hash, pwd) {
        let locked = security::record_failure(&state.redis, &email).await;
        if locked {
            return error_response("__USER_IS_LOCKED_OUT__", "Please contact customer service.");
        }
        return error_response("__LOGIN_FAILED__", "Login failed.");
    }

    if user.lockout_enabled && user.lockout_end.is_some_and(|end| end > Utc::now()) {
        return error_response("__USER_IS_LOCKED_OUT__", "Please contact customer service.");
    }

    let roles = db::get_user_roles(&state.pool, user.id)
        .await
        .unwrap_or_default();

    let is_admin = roles.iter().any(|r| {
        matches!(r.as_str(), "TenantAdmin" | "Admin" | "SuperAdmin")
    });
    let is_client = roles.contains(&"Client".to_string()) && !is_admin;

    if let Some(mono) = &state.mono_client {
        let mut client = mono.lock().await;

        let login_code_enabled = grpc::auth_client::get_two_factor_setting(
            &mut client, user.tenant_id, user.party_id,
        ).await;

        if login_code_enabled {
            let ua_hash_str = security::md5_hash(&format!("{}.thebcr.com", user_agent));
            let require_email_2fa = if is_client {
                true
            } else {
                let recent = grpc::auth_client::get_recent_user_agents(
                    &mut client, user.tenant_id, user.party_id, 3,
                ).await;
                !recent.iter().any(|ua| security::md5_hash(&format!("{}.thebcr.com", ua)) == ua_hash_str)
            };

            if require_email_2fa {
                let tf_code = req.tf_code.as_deref().unwrap_or("").replace([' ', '-'], "");
                if tf_code.is_empty() {
                    grpc::auth_client::send_auth_code(
                        &mut client, user.tenant_id, &email, "TwoFactor",
                    ).await;
                    return (HeaderMap::new(), Json(json!({ "twoFactorRequired": true, "emailSent": true }))).into_response();
                }
                let (valid, err_code) = grpc::auth_client::verify_auth_code(
                    &mut client, user.tenant_id, &email, &tf_code,
                ).await;
                if !valid {
                    return error_response("invalid_grant", &err_code);
                }
            }
        }

        if is_admin && user.two_factor_enabled {
            let tf_code = req.tf_code.as_deref().unwrap_or("").replace([' ', '-'], "");
            if tf_code.is_empty() {
                return (HeaderMap::new(), Json(json!({ "twoFactorRequired": true, "authenticator2FA": true }))).into_response();
            }
        }
    } else if user.two_factor_enabled {
        let tf_code = req.tf_code.as_deref().unwrap_or("");
        if tf_code.is_empty() {
            return (HeaderMap::new(), Json(json!({ "twoFactorRequired": true, "authenticator2FA": true }))).into_response();
        }
    }

    let party_id_hashed = hashids::encode_party_id(user.party_id);
    let display_name = user.guess_display_name();

    let ua_hash = Some(token::hash_user_agent(user_agent));

    let params = token::TokenParams {
        user_id: user.id,
        tenant_id: user.tenant_id,
        party_id_hashed: &party_id_hashed,
        god_party_id: 0,
        display_name: &display_name,
        email: user.email.as_deref().unwrap_or(""),
        roles: &roles,
        two_factor_enabled: user.two_factor_enabled,
        lifetime_secs: state.access_token_lifetime,
        user_agent_hash: ua_hash,
        sales_account: None,
        agent_account: None,
        rep_account: None,
    };

    let token_result = match token::generate_access_token(
        &params,
        &state.key_pair.private_der,
        &state.key_pair.kid,
    ) {
        Ok(t) => t,
        Err(e) => return error_response("server_error", &e.to_string()),
    };

    let refresh_token = token::generate_refresh_token();

    // Persist refresh token in Redis
    let store_data = redis_store::RefreshTokenData {
        user_id: user.id,
        tenant_id: user.tenant_id,
        party_id: party_id_hashed.clone(),
    };
    if let Err(e) = redis_store::store_refresh_token(&state.redis, &refresh_token, &store_data).await {
        redis_store::log_redis_error("store_refresh_token", e);
    }

    // Clear login failure counter on success
    security::clear_failures(&state.redis, &email).await;

    // Fire-and-forget: update last login + write login log
    let pool = state.pool.clone();
    let user_id = user.id;
    let ip_owned = ip.to_string();
    tokio::spawn(async move {
        let _ = db::update_last_login(&pool, user_id, &ip_owned).await;
    });
    if let Some(mono) = &state.mono_client {
        let mut client = mono.lock().await;
        grpc::auth_client::write_login_log(
            &mut client,
            user.tenant_id,
            user.party_id,
            ip,
            user_agent,
            referer,
        ).await;
    }

    let mut headers = HeaderMap::new();
    cookie::set_token_cookie(
        &mut headers,
        &token_result.access_token,
        token_result.expires_in,
        state.secure_cookie,
    );

    // Token is in cookie only; body intentionally omits access_token
    (
        headers,
        Json(json!({
            "token_type": "Bearer",
            "expires_in": token_result.expires_in,
            "refresh_token": refresh_token,
        })),
    ).into_response()
}

fn validate_password_any(users: &[db::User], password: &str) -> bool {
    users.iter().any(|u| {
        u.password_hash
            .as_deref()
            .is_some_and(|h| auth::password::verify_hashed_password_v3(h, password))
    })
}

/// GET /.well-known/jwks.json — public key for token verification
async fn jwks(State(state): State<Arc<AppState>>) -> Json<Jwks> {
    Json(Jwks::from_key_pair(&state.key_pair))
}

fn http_app(state: Arc<AppState>) -> Router {
    Router::new()
        .route("/connect/token", post(connect_token))
        .route("/.well-known/jwks.json", get(jwks))
        .merge(routes::auth::router())
        .layer(CorsLayer::permissive())
        .with_state(state)
}

fn env(key: &str, fallback: &str) -> String {
    std::env::var(key).unwrap_or_else(|_| fallback.to_string())
}

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
                unsafe { std::env::set_var(key, value) };
            }
        }
    }
}

fn build_database_url() -> String {
    let host = env("DB_HOST", "localhost");
    let port = env("DB_PORT", "5432");
    let user = env("DB_USERNAME", "postgres");
    let password = env("DB_PASSWORD", "");
    let database = env("DB_DATABASE", "portal_central");
    format!("postgres://{}:{}@{}:{}/{}", user, password, host, port, database)
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    load_env_file();
    let _tracing_guard = otel::init_tracing("auth");

    let database_url = build_database_url();
    let access_token_lifetime: i64 = env("ACCESS_TOKEN_LIFETIME", "86400")
        .parse()
        .unwrap_or(86400);
    let secure_cookie: bool = env("SECURE_COOKIE", "true")
        .parse()
        .unwrap_or(true);
    let jwt_secret = std::env::var("JWT_SECRET").ok();
    let rsa_key_path = std::env::var("RSA_PRIVATE_KEY_PATH").ok();
    let redis_url = env("REDIS_URL", "redis://redis:6379");
    let grpc_addr: SocketAddr = env("GRPC_ADDR", "[::]:50002").parse()?;
    let mono_grpc_addr = Some(env("MONO_GRPC_ADDR", "http://mono:50005"));
    // Strip optional http:// prefix for compatibility with .NET-style HTTP_ADDR values
    let http_addr_raw = env("HTTP_ADDR", "[::]:9002");
    let http_addr_str = http_addr_raw
        .trim_start_matches("http://")
        .trim_start_matches("https://");
    let http_addr: SocketAddr = http_addr_str.parse()?;

    info!("Loading RSA key pair...");
    let key_pair = RsaKeyPair::load_or_generate(jwt_secret.as_deref(), rsa_key_path.as_deref())?;
    info!("RSA key loaded, kid={}", key_pair.kid);

    info!(
        "Connecting to database at {}:{}",
        env("DB_HOST", "localhost"),
        env("DB_PORT", "5432")
    );
    let pool = PgPool::connect(&database_url).await?;
    info!("Connected to database");

    let redis = redis_store::create_pool(&redis_url)
        .map_err(|e| format!("Redis pool creation failed: {}", e))?;
    info!("Redis pool created for {}", redis_url);

    let mono_client = if let Some(addr) = mono_grpc_addr {
        match grpc::auth_client::connect(&addr).await {
            Ok(c) => {
                info!("Connected to mono gRPC at {}", addr);
                Some(tokio::sync::Mutex::new(c))
            }
            Err(e) => {
                tracing::warn!("Could not connect to mono gRPC at {}: {} (2FA will be skipped)", addr, e);
                None
            }
        }
    } else {
        None
    };

    let state = Arc::new(AppState {
        pool,
        redis,
        key_pair,
        access_token_lifetime,
        secure_cookie,
        mono_client,
    });

    let listener = tokio::net::TcpListener::bind(http_addr).await?;
    info!("Auth HTTP listening on http://{}", http_addr);
    info!("Auth gRPC listening on {}", grpc_addr);

    let grpc_server = grpc::validate::new_server(
        state.key_pair.private_der.clone(),
        state.key_pair.public_der.clone(),
        state.key_pair.kid.clone(),
    );

    tokio::select! {
        res = axum::serve(listener, http_app(state).into_make_service_with_connect_info::<SocketAddr>()) => {
            res?;
        }
        res = tonic::transport::Server::builder()
            .add_service(grpc_server)
            .serve(grpc_addr) => {
            res?;
        }
    }
    Ok(())
}
