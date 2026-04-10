use base64::{Engine, engine::general_purpose::URL_SAFE_NO_PAD};
use rsa::{
    RsaPrivateKey, RsaPublicKey,
    pkcs8::{DecodePrivateKey, EncodePrivateKey, LineEnding},
    traits::PublicKeyParts,
    rand_core::OsRng,
};
use serde::Serialize;
use std::path::Path;
use tracing::info;

pub struct RsaKeyPair {
    pub private_key: RsaPrivateKey,
    pub public_key: RsaPublicKey,
    /// DER-encoded private key bytes for jsonwebtoken EncodingKey
    pub private_der: Vec<u8>,
    /// DER-encoded public key bytes for jsonwebtoken DecodingKey
    pub public_der: Vec<u8>,
    /// Key ID used in JWT header and JWKS
    pub kid: String,
}

impl RsaKeyPair {
    /// Load from PEM file or generate a new 2048-bit key pair.
    pub fn load_or_generate(pem_path: Option<&str>) -> Result<Self, Box<dyn std::error::Error>> {
        let private_key = if let Some(path) = pem_path {
            if Path::new(path).exists() {
                info!("Loading RSA private key from {}", path);
                RsaPrivateKey::read_pkcs8_pem_file(path)?
            } else {
                let key = Self::generate_and_save(path)?;
                key
            }
        } else {
            info!("No RSA_PRIVATE_KEY_PATH set — generating ephemeral key pair");
            RsaPrivateKey::new(&mut OsRng, 2048)?
        };

        let public_key = RsaPublicKey::from(&private_key);
        let private_der = private_key
            .to_pkcs8_der()?
            .as_bytes()
            .to_vec();
        let public_der = {
            use rsa::pkcs8::EncodePublicKey;
            public_key.to_public_key_der()?.as_bytes().to_vec()
        };

        // kid = first 8 hex chars of SHA-256 of public DER
        let kid = {
            use sha2::{Digest, Sha256};
            let hash = Sha256::digest(&public_der);
            hex::encode(&hash[..4])
        };

        Ok(Self {
            private_key,
            public_key,
            private_der,
            public_der,
            kid,
        })
    }

    fn generate_and_save(path: &str) -> Result<RsaPrivateKey, Box<dyn std::error::Error>> {
        info!("Generating new RSA-2048 key pair, saving to {}", path);
        let key = RsaPrivateKey::new(&mut OsRng, 2048)?;
        key.write_pkcs8_pem_file(path, LineEnding::LF)?;
        Ok(key)
    }
}

/// JWKS response for GET /.well-known/jwks.json
#[derive(Serialize)]
pub struct Jwks {
    pub keys: Vec<JwkKey>,
}

#[derive(Serialize)]
pub struct JwkKey {
    pub kty: &'static str,
    pub alg: &'static str,
    #[serde(rename = "use")]
    pub use_: &'static str,
    pub kid: String,
    pub n: String,
    pub e: String,
}

impl Jwks {
    pub fn from_key_pair(kp: &RsaKeyPair) -> Self {
        let n = URL_SAFE_NO_PAD.encode(kp.public_key.n().to_bytes_be());
        let e = URL_SAFE_NO_PAD.encode(kp.public_key.e().to_bytes_be());
        Jwks {
            keys: vec![JwkKey {
                kty: "RSA",
                alg: "RS256",
                use_: "sig",
                kid: kp.kid.clone(),
                n,
                e,
            }],
        }
    }
}
