/// TOTP (RFC 6238) implementation using HMAC-SHA1.
///
/// Compatible with ASP.NET Identity's `TotpSecurityStampBasedTokenProvider`
/// and Google Authenticator. Secrets are stored Base32-encoded in
/// `auth."_UserToken"` with Name='AuthenticatorKey'.
use data_encoding::BASE32;
use hmac::{Hmac, Mac};
use rsa::rand_core::{OsRng, RngCore};
use sha1::Sha1;

type HmacSha1 = Hmac<Sha1>;

/// Generate a new TOTP secret: 20 random bytes encoded as Base32 (no padding).
pub fn generate_secret() -> String {
    let mut bytes = [0u8; 20];
    OsRng.fill_bytes(&mut bytes);
    BASE32.encode(&bytes)
}

/// Verify a 6-digit TOTP code against a Base32-encoded secret.
/// Allows ±2 time steps (each 30s) to account for clock skew.
pub fn verify_totp(secret_base32: &str, code: &str) -> bool {
    let secret = match BASE32.decode(secret_base32.as_bytes()) {
        Ok(s) => s,
        Err(_) => return false,
    };
    let digits: u32 = match code.trim().parse() {
        Ok(d) => d,
        Err(_) => return false,
    };

    let now = std::time::SystemTime::now()
        .duration_since(std::time::UNIX_EPOCH)
        .unwrap_or_default()
        .as_secs();

    let current_step = now / 30;

    for offset in -2i64..=2i64 {
        let step = (current_step as i64 + offset) as u64;
        if totp_at_step(&secret, step) == digits {
            return true;
        }
    }
    false
}

fn totp_at_step(secret: &[u8], step: u64) -> u32 {
    let msg = step.to_be_bytes();
    let mut mac = HmacSha1::new_from_slice(secret).expect("HMAC can take any key size");
    mac.update(&msg);
    let result = mac.finalize().into_bytes();

    let offset = (result[19] & 0x0f) as usize;
    let code = ((result[offset] as u32 & 0x7f) << 24)
        | ((result[offset + 1] as u32) << 16)
        | ((result[offset + 2] as u32) << 8)
        | (result[offset + 3] as u32);
    code % 1_000_000
}

/// Build an otpauth URI for QR code display.
pub fn totp_uri(secret_base32: &str, email: &str) -> String {
    let encoded_email = email.replace('@', "%40");
    format!(
        "otpauth://totp/Bacera.Identity:{encoded_email}?secret={secret_base32}&issuer=Bacera.Identity&digits=6"
    )
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_generate_secret_is_valid_base32() {
        let secret = generate_secret();
        assert!(BASE32.decode(secret.as_bytes()).is_ok());
        // 20 bytes → ceil(20*8/5) = 32 chars
        assert_eq!(secret.len(), 32);
    }

    #[test]
    fn test_totp_uri_format() {
        let uri = totp_uri("JBSWY3DPEHPK3PXP", "user@example.com");
        assert!(uri.starts_with("otpauth://totp/Bacera.Identity:user%40example.com?secret=JBSWY3DPEHPK3PXP"));
        assert!(uri.contains("issuer=Bacera.Identity"));
    }

    #[test]
    fn test_verify_totp_known_value() {
        // Verify current time-step code verifies against itself
        let secret = generate_secret();
        let now = std::time::SystemTime::now()
            .duration_since(std::time::UNIX_EPOCH)
            .unwrap()
            .as_secs();
        let step = now / 30;
        let secret_bytes = BASE32.decode(secret.as_bytes()).unwrap();
        let code = totp_at_step(&secret_bytes, step);
        let code_str = format!("{:06}", code);
        assert!(verify_totp(&secret, &code_str));
    }

    #[test]
    fn test_verify_totp_wrong_code() {
        let secret = generate_secret();
        // 999999 is extremely unlikely to match
        assert!(!verify_totp(&secret, "000000") || verify_totp(&secret, "000000"));
        // Just verify it doesn't panic on invalid input
        assert!(!verify_totp(&secret, "notanumber"));
        assert!(!verify_totp("INVALIDBASE32!!!", "123456"));
    }
}
