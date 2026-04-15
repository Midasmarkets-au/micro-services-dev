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
    #[serde(rename = "GodPartyId", default, skip_serializing_if = "String::is_empty")]
    pub god_party_id: String,
    /// True when this token was issued in god-mode (admin impersonating a user).
    #[serde(rename = "IsGodMode", skip_serializing_if = "Option::is_none")]
    pub is_god_mode: Option<bool>,
    /// Origin header from the login request (e.g. "https://portal.trademdm.com").
    #[serde(rename = "Origin", skip_serializing_if = "Option::is_none")]
    pub origin: Option<String>,
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
    /// Origin header from the login request.
    pub origin: Option<String>,
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

    let is_god_mode_active = params.god_party_id != 0;
    let god_party_id_hashed = if is_god_mode_active {
        crate::hashids::encode_party_id(params.god_party_id)
    } else {
        String::new()
    };
    let is_god_mode = if is_god_mode_active { Some(true) } else { None };

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
        is_god_mode,
        origin: params.origin.clone(),
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

#[cfg(test)]
mod tests {
    use super::*;
    use std::sync::OnceLock;
    use crate::keys::RsaKeyPair;
    use jsonwebtoken::{Algorithm, DecodingKey, Validation, decode};
    use rsa::pkcs1::EncodeRsaPublicKey as _;

    fn test_key_pair() -> &'static RsaKeyPair {
        static KEY: OnceLock<RsaKeyPair> = OnceLock::new();
        KEY.get_or_init(|| RsaKeyPair::load_or_generate(None, None).unwrap())
    }

    fn base_params<'a>(_kp: &'a RsaKeyPair, roles: &'a [String]) -> TokenParams<'a> {
        TokenParams {
            user_id: 42,
            tenant_id: 7,
            party_id_hashed: "abc123",
            god_party_id: 0,
            display_name: "Test User",
            email: "test@example.com",
            roles,
            two_factor_enabled: false,
            lifetime_secs: 86400,
            user_agent_hash: None,
            sales_account: None,
            agent_account: None,
            rep_account: None,
        }
    }

    #[test]
    fn test_hash_user_agent() {
        let result = hash_user_agent("Mozilla");
        let expected = format!("{:x}", md5::compute("Mozilla.thebcr.com".as_bytes()));
        assert_eq!(result, expected);
    }

    #[test]
    fn test_refresh_token_is_uuid() {
        let token = generate_refresh_token();
        assert!(uuid::Uuid::parse_str(&token).is_ok());
    }

    #[test]
    fn test_access_token_claims() {
        let kp = test_key_pair();
        let roles = vec!["Client".to_string()];
        let params = base_params(kp, &roles);
        let result = generate_access_token(&params, &kp.private_der, &kp.kid).unwrap();

        // jsonwebtoken::DecodingKey::from_rsa_der expects PKCS#1 RSAPublicKey DER
        let pub_pkcs1_der = kp.public_key.to_pkcs1_der().unwrap();
        let decoding_key = DecodingKey::from_rsa_der(pub_pkcs1_der.as_bytes());
        let mut validation = Validation::new(Algorithm::RS256);
        validation.validate_aud = false;

        let decoded = decode::<Claims>(&result.access_token, &decoding_key, &validation).unwrap();
        let claims = decoded.claims;

        assert_eq!(claims.sub, "42");
        assert_eq!(claims.tenant_id, "7");
        assert_eq!(claims.email, "test@example.com");
        assert_eq!(claims.role, vec!["Client"]);
        assert_eq!(claims.amr, Some("pwd".to_string()));
        assert_eq!(claims.idp, "local");
    }

    #[test]
    fn test_tenant_admin_lifetime() {
        let kp = test_key_pair();
        let roles = vec!["TenantAdmin".to_string()];
        let params = base_params(kp, &roles);
        let result = generate_access_token(&params, &kp.private_der, &kp.kid).unwrap();
        assert_eq!(result.expires_in, TENANT_ADMIN_LIFETIME_SECS);
    }

    #[test]
    fn test_non_admin_lifetime() {
        let kp = test_key_pair();
        let roles = vec!["Client".to_string()];
        let params = base_params(kp, &roles);
        let result = generate_access_token(&params, &kp.private_der, &kp.kid).unwrap();
        assert_eq!(result.expires_in, 86400);
    }

    #[test]
    fn test_two_factor_amr() {
        let kp = test_key_pair();
        let roles = vec!["Client".to_string()];
        let mut params = base_params(kp, &roles);
        params.two_factor_enabled = true;
        let result = generate_access_token(&params, &kp.private_der, &kp.kid).unwrap();

        let pub_pkcs1_der = kp.public_key.to_pkcs1_der().unwrap();
        let decoding_key = DecodingKey::from_rsa_der(pub_pkcs1_der.as_bytes());
        let mut validation = Validation::new(Algorithm::RS256);
        validation.validate_aud = false;
        let decoded = decode::<Claims>(&result.access_token, &decoding_key, &validation).unwrap();
        assert_eq!(decoded.claims.amr, Some("mfa".to_string()));
    }
}
