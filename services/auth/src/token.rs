use chrono::Utc;
use jsonwebtoken::{Algorithm, EncodingKey, Header, encode};
use serde::{Deserialize, Serialize};
use uuid::Uuid;

/// Full claims mirroring mono's PasswordGrantHandler.BuildPrincipal.
///
/// Standard OIDC claims use lowercase keys; custom mono claims use PascalCase
/// to match what mono's ClaimsPrincipal extensions expect.
#[derive(Debug, Serialize, Deserialize, Clone)]
pub struct Claims {
    // Standard
    pub sub: String,
    pub iat: i64,
    pub exp: i64,
    pub nbf: i64,
    pub jti: String,
    pub name: String,
    pub email: String,
    pub auth_time: i64,
    pub idp: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub amr: Option<String>,

    // .NET ClaimTypes URI variants (required by mono middleware)
    #[serde(rename = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")]
    pub name_identifier: String,
    #[serde(rename = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")]
    pub claims_name: String,
    #[serde(rename = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")]
    pub claims_email: String,

    // Custom mono claims
    #[serde(rename = "TenantId")]
    pub tenant_id: String,
    #[serde(rename = "PartyId")]
    pub party_id: String,
    #[serde(rename = "GodPartyId")]
    pub god_party_id: String,
    #[serde(rename = "Version")]
    pub version: String,
    #[serde(rename = "UserAgent", skip_serializing_if = "Option::is_none")]
    pub user_agent: Option<String>,
    #[serde(rename = "SalesAccount", skip_serializing_if = "Option::is_none")]
    pub sales_account: Option<String>,
    #[serde(rename = "AgentAccount", skip_serializing_if = "Option::is_none")]
    pub agent_account: Option<String>,
    #[serde(rename = "RepAccount", skip_serializing_if = "Option::is_none")]
    pub rep_account: Option<String>,

    // Roles — serialized as array to match OpenIddict multi-value claim behaviour
    pub role: Vec<String>,
}

pub struct TokenResult {
    pub access_token: String,
    pub expires_in: i64,
}

const VERSION: &str = "08-23-24";
/// TenantAdmin gets a very long-lived token (matches mono's 3650-day behaviour).
const TENANT_ADMIN_LIFETIME_SECS: i64 = 3650 * 24 * 3600;

pub struct TokenParams<'a> {
    pub user_id: i64,
    pub tenant_id: i64,
    pub party_id_hashed: &'a str,
    pub god_party_id: i64,
    pub display_name: &'a str,
    pub email: &'a str,
    pub roles: &'a [String],
    pub two_factor_enabled: bool,
    pub lifetime_secs: i64,
    /// MD5(raw_user_agent + ".thebcr.com") — computed by caller
    pub user_agent_hash: Option<String>,
    pub sales_account: Option<i64>,
    pub agent_account: Option<i64>,
    pub rep_account: Option<i64>,
}

pub fn generate_access_token(
    params: &TokenParams<'_>,
    private_der: &[u8],
    kid: &str,
) -> Result<TokenResult, jsonwebtoken::errors::Error> {
    let now = Utc::now().timestamp();
    let is_tenant_admin = params.roles.iter().any(|r| r == "TenantAdmin");
    let effective_lifetime = if is_tenant_admin {
        TENANT_ADMIN_LIFETIME_SECS
    } else {
        params.lifetime_secs
    };

    let amr = if params.two_factor_enabled {
        Some("mfa".to_string())
    } else {
        Some("pwd".to_string())
    };

    let god_party_id_hashed = crate::hashids::encode_party_id(params.god_party_id);

    let claims = Claims {
        sub: params.user_id.to_string(),
        iat: now,
        exp: now + effective_lifetime,
        nbf: now,
        jti: Uuid::new_v4().to_string(),
        name: params.display_name.to_string(),
        email: params.email.to_string(),
        auth_time: now,
        idp: "local".to_string(),
        amr,
        name_identifier: params.user_id.to_string(),
        claims_name: format!("{}_{}", params.tenant_id, params.email),
        claims_email: params.email.to_string(),
        tenant_id: params.tenant_id.to_string(),
        party_id: params.party_id_hashed.to_string(),
        god_party_id: god_party_id_hashed,
        version: VERSION.to_string(),
        user_agent: params.user_agent_hash.clone(),
        sales_account: params.sales_account.map(|v| v.to_string()),
        agent_account: params.agent_account.map(|v| v.to_string()),
        rep_account: params.rep_account.map(|v| v.to_string()),
        role: params.roles.to_vec(),
    };

    let mut header = Header::new(Algorithm::RS256);
    header.kid = Some(kid.to_string());
    header.typ = Some("at+jwt".to_string());

    let token = encode(&header, &claims, &EncodingKey::from_rsa_der(private_der))?;

    Ok(TokenResult {
        access_token: token,
        expires_in: effective_lifetime,
    })
}

pub fn generate_refresh_token() -> String {
    Uuid::new_v4().to_string()
}

/// Compute the UserAgent claim value: MD5(raw_ua + ".thebcr.com")
pub fn hash_user_agent(raw_ua: &str) -> String {
    let input = format!("{}.thebcr.com", raw_ua);
    format!("{:x}", md5::compute(input.as_bytes()))
}
