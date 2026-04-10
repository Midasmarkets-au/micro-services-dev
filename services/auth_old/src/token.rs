use chrono::Utc;
use jsonwebtoken::{EncodingKey, Header, encode};
use serde::{Deserialize, Serialize};
use uuid::Uuid;

#[derive(Debug, Serialize, Deserialize)]
pub struct Claims {
    pub sub: String,
    pub iat: i64,
    pub exp: i64,
    pub nbf: i64,
    pub jti: String,
    #[serde(rename = "TenantId")]
    pub tenant_id: String,
    #[serde(rename = "PartyId")]
    pub party_id: String,
    #[serde(rename = "Version")]
    pub version: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub role: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub amr: Option<String>,
}

pub struct TokenResult {
    pub access_token: String,
    pub expires_in: i64,
}

const VERSION: &str = "08-23-24";

pub fn generate_access_token(
    user_id: i64,
    tenant_id: i64,
    party_id_hashed: &str,
    roles: &[String],
    two_factor_enabled: bool,
    lifetime_secs: i64,
    jwt_secret: &str,
) -> Result<TokenResult, jsonwebtoken::errors::Error> {
    let now = Utc::now().timestamp();
    let is_tenant_admin = roles.iter().any(|r| r == "TenantAdmin");
    let effective_lifetime = if is_tenant_admin { 2_592_000 } else { lifetime_secs };

    let role = roles.first().cloned();
    let amr = if two_factor_enabled {
        Some("mfa".to_string())
    } else {
        Some("pwd".to_string())
    };

    let claims = Claims {
        sub: user_id.to_string(),
        iat: now,
        exp: now + effective_lifetime,
        nbf: now,
        jti: Uuid::new_v4().to_string(),
        tenant_id: tenant_id.to_string(),
        party_id: party_id_hashed.to_string(),
        version: VERSION.to_string(),
        role,
        amr,
    };

    let token = encode(
        &Header::default(),
        &claims,
        &EncodingKey::from_secret(jwt_secret.as_bytes()),
    )?;

    Ok(TokenResult {
        access_token: token,
        expires_in: effective_lifetime,
    })
}

pub fn generate_refresh_token() -> String {
    Uuid::new_v4().to_string()
}
