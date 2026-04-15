use base64::{Engine, engine::general_purpose::URL_SAFE_NO_PAD};
use rsa::{
    RsaPrivateKey, RsaPublicKey,
    pkcs8::{DecodePrivateKey, EncodePrivateKey, LineEnding},
    pkcs1::EncodeRsaPrivateKey,
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
    /// Load RSA key pair in priority order:
    ///   1. `JWT_SECRET` env var — PEM content directly (preferred for K8s secrets)
    ///   2. `RSA_PRIVATE_KEY_PATH` env var — path to a PEM file
    ///   3. Ephemeral — generate a new key (not persisted, tokens invalidated on restart)
    pub fn load_or_generate(pem_content: Option<&str>, pem_path: Option<&str>) -> Result<Self, Box<dyn std::error::Error>> {
        let private_key = if let Some(pem) = pem_content {
            info!("Loading RSA private key from JWT_SECRET env var");
            // Normalize: handle literal \n escape sequences, strip \r, then
            // rebuild a canonical PEM with exactly 64-char lines so that
            // from_pkcs8_pem succeeds regardless of how K8s injected the value.
            let normalized = pem.replace("\\n", "\n").replace('\r', "");
            let b64_raw: String = normalized
                .lines()
                .filter(|l| !l.trim().starts_with("-----") && !l.trim().is_empty())
                .flat_map(|l| l.trim().chars())
                .collect();
            // Re-chunk into 64-char lines
            let b64_chunked = b64_raw
                .as_bytes()
                .chunks(64)
                .map(|c| std::str::from_utf8(c).unwrap())
                .collect::<Vec<_>>()
                .join("\n");
            let canonical = format!(
                "-----BEGIN PRIVATE KEY-----\n{}\n-----END PRIVATE KEY-----\n",
                b64_chunked
            );
            RsaPrivateKey::from_pkcs8_pem(&canonical)?
        } else if let Some(path) = pem_path {
            if Path::new(path).exists() {
                info!("Loading RSA private key from {}", path);
                RsaPrivateKey::read_pkcs8_pem_file(path)?
            } else {
                Self::generate_and_save(path)?
            }
        } else {
            info!("No JWT_SECRET or RSA_PRIVATE_KEY_PATH set — generating ephemeral RSA-2048 key pair (not persisted across restarts)");
            RsaPrivateKey::new(&mut OsRng, 2048)?
        };

        let public_key = RsaPublicKey::from(&private_key);
        // jsonwebtoken::EncodingKey::from_rsa_der expects PKCS#1 DER (RSAPrivateKey),
        // NOT PKCS#8 DER (PrivateKeyInfo). Use to_pkcs1_der() accordingly.
        let private_der = private_key
            .to_pkcs1_der()?
            .as_bytes()
            .to_vec();
        // jsonwebtoken::DecodingKey::from_rsa_der expects PKCS#1 RSAPublicKey DER,
        // NOT PKCS#8 SubjectPublicKeyInfo. Use to_pkcs1_der() here.
        let public_der = {
            use rsa::pkcs1::EncodeRsaPublicKey;
            public_key.to_pkcs1_der()?.as_bytes().to_vec()
        };

        // kid = first 8 hex chars of SHA-256 of SPKI DER (stable across restarts,
        // computed from SubjectPublicKeyInfo to match the previously published JWKS kid).
        let kid = {
            use rsa::pkcs8::EncodePublicKey;
            use sha2::{Digest, Sha256};
            let spki_der = public_key.to_public_key_der()?.as_bytes().to_vec();
            let hash = Sha256::digest(&spki_der);
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

#[cfg(test)]
mod tests {
    use super::*;
    use std::sync::OnceLock;

    fn test_key_pair() -> &'static RsaKeyPair {
        static KEY: OnceLock<RsaKeyPair> = OnceLock::new();
        KEY.get_or_init(|| RsaKeyPair::load_or_generate(None, None).unwrap())
    }

    #[test]
    fn test_kid_is_8_hex_chars() {
        let kp = test_key_pair();
        assert_eq!(kp.kid.len(), 8);
        assert!(kp.kid.chars().all(|c| c.is_ascii_hexdigit()));
    }

    #[test]
    fn test_jwks_has_correct_fields() {
        let kp = test_key_pair();
        let jwks = Jwks::from_key_pair(kp);
        assert_eq!(jwks.keys.len(), 1);
        let key = &jwks.keys[0];
        assert_eq!(key.kty, "RSA");
        assert_eq!(key.alg, "RS256");
        assert_eq!(key.use_, "sig");
        assert_eq!(key.kid, kp.kid);
        assert!(!key.n.is_empty());
        assert!(!key.e.is_empty());
    }
}
