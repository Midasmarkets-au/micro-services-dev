use jsonwebtoken::{Algorithm, DecodingKey, Validation, decode};
use tonic::{Request, Response, Status};

use crate::generated::api_v1::{
    ValidateTokenRequest, ValidateTokenResponse,
    auth_validation_service_server::{AuthValidationService, AuthValidationServiceServer},
};
use crate::token::Claims;

pub struct AuthValidationServer {
    pub public_der: Vec<u8>,
}

pub fn new_server(public_der: Vec<u8>) -> AuthValidationServiceServer<AuthValidationServer> {
    AuthValidationServiceServer::new(AuthValidationServer { public_der })
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
}
