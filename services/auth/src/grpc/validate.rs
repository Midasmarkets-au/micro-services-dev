use jsonwebtoken::{Algorithm, DecodingKey, Validation, decode};
use tonic::{Request, Response, Status};

use crate::generated::api_v1::{
    IssueTokenRequest, IssueTokenResponse,
    ValidateTokenRequest, ValidateTokenResponse,
    auth_validation_service_server::{AuthValidationService, AuthValidationServiceServer},
};
use crate::token::{Claims, TokenParams, generate_access_token};

pub struct AuthValidationServer {
    pub private_der: Vec<u8>,
    pub public_der: Vec<u8>,
    pub kid: String,
}

pub fn new_server(
    private_der: Vec<u8>,
    public_der: Vec<u8>,
    kid: String,
) -> AuthValidationServiceServer<AuthValidationServer> {
    AuthValidationServiceServer::new(AuthValidationServer { private_der, public_der, kid })
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

        match decode::<Claims>(&token, &decoding_key, &validation) {
            Ok(data) => {
                let c = data.claims;
                Ok(Response::new(ValidateTokenResponse {
                    valid: true,
                    user_id: c.sub.parse().unwrap_or(0),
                    tenant_id: c.tenant_id.parse().unwrap_or(0),
                    party_id: c.party_id,
                    roles: c.role,
                    error: String::new(),
                }))
            }
            Err(e) => Ok(Response::new(ValidateTokenResponse {
                valid: false,
                user_id: 0,
                tenant_id: 0,
                party_id: String::new(),
                roles: vec![],
                error: e.to_string(),
            })),
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
            lifetime_secs: 86400,
            user_agent_hash,
            sales_account: if req.sales_account != 0 { Some(req.sales_account) } else { None },
            agent_account: if req.agent_account != 0 { Some(req.agent_account) } else { None },
            rep_account: if req.rep_account != 0 { Some(req.rep_account) } else { None },
        };

        match generate_access_token(&params, &self.private_der, &self.kid) {
            Ok(result) => Ok(Response::new(IssueTokenResponse {
                access_token: result.access_token,
                expires_in: result.expires_in,
            })),
            Err(e) => Err(Status::internal(format!("token generation failed: {}", e))),
        }
    }
}
