/// Axum extractor for authenticated users.
///
/// Reads the JWT from the `access_token` cookie (or `Authorization: Bearer` header),
/// validates it with the RS256 public key, and returns `AuthUser`.
/// Returns HTTP 401 if missing or invalid.
use std::sync::Arc;

use axum::{
    async_trait,
    extract::FromRequestParts,
    http::{StatusCode, request::Parts},
    response::{IntoResponse, Response},
};
use base64::{Engine, engine::general_purpose::URL_SAFE_NO_PAD};
use jsonwebtoken::{Algorithm, DecodingKey, Validation, decode};
use rsa::traits::PublicKeyParts;
use serde_json::json;

use crate::state::AppState;
use crate::token::Claims;

pub struct AuthUser {
    pub user_id: i64,
    pub tenant_id: i64,
    pub party_id: String,
    pub email: String,
    /// Non-empty when the token was issued in god-mode (admin impersonating a user).
    pub god_party_id: String,
    pub is_god_mode: bool,
}

pub struct AuthError(StatusCode, &'static str);

impl IntoResponse for AuthError {
    fn into_response(self) -> Response {
        (self.0, axum::Json(json!({ "error": self.1 }))).into_response()
    }
}

#[async_trait]
impl FromRequestParts<Arc<AppState>> for AuthUser {
    type Rejection = AuthError;

    async fn from_request_parts(
        parts: &mut Parts,
        state: &Arc<AppState>,
    ) -> Result<Self, Self::Rejection> {
        // 1. Try cookie first, then Authorization header
        let token = extract_token(parts);
        let token = token.ok_or(AuthError(StatusCode::UNAUTHORIZED, "missing_token"))?;

        // 2. Validate with RS256 public key (use n/e components to avoid DER format issues)
        let n = URL_SAFE_NO_PAD.encode(state.key_pair.public_key.n().to_bytes_be());
        let e = URL_SAFE_NO_PAD.encode(state.key_pair.public_key.e().to_bytes_be());
        let decoding_key = DecodingKey::from_rsa_components(&n, &e)
            .map_err(|e| {
                tracing::warn!("AuthUser: failed to build decoding key: {}", e);
                AuthError(StatusCode::UNAUTHORIZED, "invalid_token")
            })?;
        let mut validation = Validation::new(Algorithm::RS256);
        validation.validate_aud = false;

        let claims = decode::<Claims>(&token, &decoding_key, &validation)
            .map_err(|e| {
                tracing::warn!("AuthUser: token decode failed: {}", e);
                AuthError(StatusCode::UNAUTHORIZED, "invalid_token")
            })?
            .claims;

        Ok(AuthUser {
            user_id: claims.sub.parse().unwrap_or(0),
            tenant_id: claims.tenant_id.parse().unwrap_or(0),
            party_id: claims.party_id,
            email: claims.email,
            is_god_mode: claims.is_god_mode.unwrap_or(false) || !claims.god_party_id.is_empty(),
            god_party_id: claims.god_party_id,
        })
    }
}

fn extract_token(parts: &Parts) -> Option<String> {
    // Cookie: access_token=...
    if let Some(cookie_header) = parts.headers.get("cookie") {
        if let Ok(cookie_str) = cookie_header.to_str() {
            for part in cookie_str.split(';') {
                let part = part.trim();
                if let Some(val) = part.strip_prefix("access_token=") {
                    if !val.is_empty() {
                        return Some(val.to_string());
                    }
                }
            }
        }
    }
    // Authorization: Bearer <token>
    if let Some(auth_header) = parts.headers.get("authorization") {
        if let Ok(auth_str) = auth_header.to_str() {
            if let Some(token) = auth_str.strip_prefix("Bearer ") {
                if !token.is_empty() {
                    return Some(token.to_string());
                }
            }
        }
    }
    None
}
