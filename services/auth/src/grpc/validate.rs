use deadpool_redis::Pool as RedisPool;
use jsonwebtoken::{Algorithm, DecodingKey, Validation, decode};
use tonic::{Request, Response, Status};

use crate::generated::api_v1::{
    IssueTokenRequest, IssueTokenResponse,
    RevokeUserRequest, RevokeUserResponse,
    ValidateTokenRequest, ValidateTokenResponse,
    auth_validation_service_server::{AuthValidationService, AuthValidationServiceServer},
};
use crate::token::{Claims, TokenParams, generate_access_token, generate_refresh_token};
use crate::redis_store::{
    RefreshTokenData, store_refresh_token,
    is_jti_blocked, get_party_revoke_ts, revoke_party,
};

pub struct AuthValidationServer {
    pub private_der: Vec<u8>,
    pub public_der: Vec<u8>,
    pub kid: String,
    pub redis: RedisPool,
}

pub fn new_server(
    private_der: Vec<u8>,
    public_der: Vec<u8>,
    kid: String,
    redis: RedisPool,
) -> AuthValidationServiceServer<AuthValidationServer> {
    AuthValidationServiceServer::new(AuthValidationServer { private_der, public_der, kid, redis })
}

#[tonic::async_trait]
impl AuthValidationService for AuthValidationServer {
    async fn validate_token(
        &self,
        request: Request<ValidateTokenRequest>,
    ) -> Result<Response<ValidateTokenResponse>, Status> {
        let token = request.into_inner().token;

        let decoding_key = DecodingKey::from_rsa_der(&self.public_der);
        let mut validation = Validation::new(Algorithm::RS256);
        // Accept both at+jwt (new) and JWT (legacy IS4 migration)
        validation.validate_aud = false;

        let claims = match decode::<Claims>(&token, &decoding_key, &validation) {
            Ok(data) => data.claims,
            Err(e) => {
                return Ok(Response::new(ValidateTokenResponse {
                    valid: false,
                    user_id: 0,
                    tenant_id: 0,
                    party_id: String::new(),
                    roles: vec![],
                    error: e.to_string(),
                    jti: String::new(),
                    exp: 0,
                }));
            }
        };

        // Check jti blocklist (logout)
        if is_jti_blocked(&self.redis, &claims.jti).await {
            return Ok(Response::new(ValidateTokenResponse {
                valid: false,
                error: "token_revoked".to_string(),
                jti: claims.jti,
                exp: claims.exp,
                ..Default::default()
            }));
        }

        // Check party-level revocation (ban/lockout)
        // Decode hashed party_id to raw i64 for the Redis key lookup
        if let Some(raw_party_id) = crate::hashids::decode_party_id(&claims.party_id) {
            if let Some(revoke_ts) = get_party_revoke_ts(&self.redis, raw_party_id).await {
                if claims.iat < revoke_ts {
                    return Ok(Response::new(ValidateTokenResponse {
                        valid: false,
                        error: "user_revoked".to_string(),
                        jti: claims.jti,
                        exp: claims.exp,
                        ..Default::default()
                    }));
                }
            }
        }

        Ok(Response::new(ValidateTokenResponse {
            valid: true,
            user_id: claims.sub.parse().unwrap_or(0),
            tenant_id: claims.tenant_id.parse().unwrap_or(0),
            party_id: claims.party_id,
            roles: claims.role,
            error: String::new(),
            jti: claims.jti,
            exp: claims.exp,
        }))
    }

    async fn revoke_user(
        &self,
        request: Request<RevokeUserRequest>,
    ) -> Result<Response<RevokeUserResponse>, Status> {
        let party_id = request.into_inner().party_id;
        tracing::info!("RevokeUser gRPC called: party_id={}", party_id);

        match revoke_party(&self.redis, party_id).await {
            Ok(()) => {
                tracing::info!("RevokeUser: party_id={} revoked successfully", party_id);
                Ok(Response::new(RevokeUserResponse { ok: true }))
            }
            Err(e) => {
                tracing::error!("RevokeUser: Redis error for party_id={}: {}", party_id, e);
                Err(Status::internal(format!("revoke failed: {}", e)))
            }
        }
    }

    async fn issue_token(
        &self,
        request: Request<IssueTokenRequest>,
    ) -> Result<Response<IssueTokenResponse>, Status> {
        let req = request.into_inner();
        tracing::info!("IssueToken gRPC called: user_id={}, tenant_id={}", req.user_id, req.tenant_id);

        let user_agent_hash = if req.user_agent.is_empty() {
            None
        } else {
            Some(crate::token::hash_user_agent(&req.user_agent))
        };

        let params = TokenParams {
            user_id: req.user_id,
            tenant_id: req.tenant_id,
            party_id_hashed: &req.party_id_hashed,
            god_party_id: req.god_party_id,
            display_name: &req.display_name,
            email: &req.email,
            roles: &req.roles,
            two_factor_enabled: req.two_factor_enabled,
            lifetime_secs: 3600,
            user_agent_hash,
            sales_account: if req.sales_account != 0 { Some(req.sales_account) } else { None },
            agent_account: if req.agent_account != 0 { Some(req.agent_account) } else { None },
            rep_account: if req.rep_account != 0 { Some(req.rep_account) } else { None },
            origin: None,
        };

        let token_result = match generate_access_token(&params, &self.private_der, &self.kid) {
            Ok(r) => r,
            Err(e) => return Err(Status::internal(format!("token generation failed: {}", e))),
        };

        let refresh_token = generate_refresh_token();
        let store_data = RefreshTokenData {
            user_id: req.user_id,
            tenant_id: req.tenant_id,
            party_id: req.party_id_hashed.clone(),
        };
        if let Err(e) = store_refresh_token(&self.redis, &refresh_token, &store_data).await {
            tracing::warn!("IssueToken: failed to store refresh token in Redis: {}", e);
        }

        Ok(Response::new(IssueTokenResponse {
            access_token: token_result.access_token,
            expires_in: token_result.expires_in,
            refresh_token,
        }))
    }
}
