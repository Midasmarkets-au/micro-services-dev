/// ASP.NET Identity V3 password hash verification.
///
/// Format (header byte 0x01):
///   [0]      = 0x01   (format marker)
///   [1..5]   = PRF    (network-byte-order u32: 1=HMACSHA256, 2=HMACSHA512)
///   [5..9]   = iter   (network-byte-order u32)
///   [9..13]  = salt_len (network-byte-order u32)
///   [13..13+salt_len] = salt
///   [13+salt_len..]   = sub-key
use base64::Engine as _;
use base64::engine::general_purpose::STANDARD as BASE64;
use pbkdf2::pbkdf2_hmac;
use sha2::{Sha256, Sha512};

fn read_network_u32(buf: &[u8], offset: usize) -> u32 {
    ((buf[offset] as u32) << 24)
        | ((buf[offset + 1] as u32) << 16)
        | ((buf[offset + 2] as u32) << 8)
        | (buf[offset + 3] as u32)
}

pub fn verify_hashed_password_v3(hashed_base64: &str, password: &str) -> bool {
    let hashed = match BASE64.decode(hashed_base64) {
        Ok(v) => v,
        Err(_) => return false,
    };

    if hashed.is_empty() || hashed[0] != 0x01 {
        return false;
    }
    if hashed.len() < 13 {
        return false;
    }

    let prf = read_network_u32(&hashed, 1);
    let iter_count = read_network_u32(&hashed, 5);
    let salt_len = read_network_u32(&hashed, 9) as usize;

    if salt_len < 16 {
        return false;
    }
    if hashed.len() < 13 + salt_len {
        return false;
    }

    let salt = &hashed[13..13 + salt_len];
    let expected_sub_key = &hashed[13 + salt_len..];
    let sub_key_len = expected_sub_key.len();

    if sub_key_len < 16 {
        return false;
    }

    let mut actual_sub_key = vec![0u8; sub_key_len];

    match prf {
        1 => pbkdf2_hmac::<Sha256>(password.as_bytes(), salt, iter_count, &mut actual_sub_key),
        2 => pbkdf2_hmac::<Sha512>(password.as_bytes(), salt, iter_count, &mut actual_sub_key),
        _ => return false,
    }

    constant_time_eq(&actual_sub_key, expected_sub_key)
}

/// Generate an ASP.NET Identity V3 password hash (PBKDF2-HMAC-SHA256, 10000 iter, 16-byte salt).
pub fn hash_password(password: &str) -> String {
    use rsa::rand_core::{OsRng, RngCore};
    let mut salt = [0u8; 16];
    OsRng.fill_bytes(&mut salt);

    let sub_key_len = 32usize;
    let mut sub_key = vec![0u8; sub_key_len];
    pbkdf2_hmac::<Sha256>(password.as_bytes(), &salt, 10_000, &mut sub_key);

    let salt_len = salt.len() as u32;
    let prf: u32 = 1; // HMAC-SHA256
    let iterations: u32 = 10_000;

    let mut buf = Vec::with_capacity(13 + salt.len() + sub_key_len);
    buf.push(0x01u8);
    buf.extend_from_slice(&prf.to_be_bytes());
    buf.extend_from_slice(&iterations.to_be_bytes());
    buf.extend_from_slice(&salt_len.to_be_bytes());
    buf.extend_from_slice(&salt);
    buf.extend_from_slice(&sub_key);
    BASE64.encode(&buf)
}

fn constant_time_eq(a: &[u8], b: &[u8]) -> bool {
    if a.len() != b.len() {
        return false;
    }
    let mut diff = 0u8;
    for (x, y) in a.iter().zip(b.iter()) {
        diff |= x ^ y;
    }
    diff == 0
}

#[cfg(test)]
mod tests {
    use super::*;
    use base64::engine::general_purpose::STANDARD as BASE64;

    /// Build a valid ASP.NET Identity V3 hash for testing.
    fn make_v3_hash(password: &str, salt: &[u8], prf: u32, iterations: u32) -> String {
        let sub_key_len = 32usize;
        let mut sub_key = vec![0u8; sub_key_len];
        match prf {
            1 => pbkdf2_hmac::<Sha256>(password.as_bytes(), salt, iterations, &mut sub_key),
            2 => pbkdf2_hmac::<Sha512>(password.as_bytes(), salt, iterations, &mut sub_key),
            _ => panic!("unsupported prf"),
        }

        let salt_len = salt.len() as u32;
        let mut buf = Vec::with_capacity(13 + salt.len() + sub_key_len);
        buf.push(0x01u8); // format marker
        buf.extend_from_slice(&prf.to_be_bytes());
        buf.extend_from_slice(&iterations.to_be_bytes());
        buf.extend_from_slice(&salt_len.to_be_bytes());
        buf.extend_from_slice(salt);
        buf.extend_from_slice(&sub_key);
        BASE64.encode(&buf)
    }

    #[test]
    fn test_verify_empty_returns_false() {
        assert!(!verify_hashed_password_v3("", "password"));
    }

    #[test]
    fn test_verify_correct_sha256() {
        let salt = b"1234567890abcdef"; // 16 bytes
        let hash = make_v3_hash("secret", salt, 1, 10_000);
        assert!(verify_hashed_password_v3(&hash, "secret"));
    }

    #[test]
    fn test_verify_wrong_password() {
        let salt = b"1234567890abcdef";
        let hash = make_v3_hash("secret", salt, 1, 10_000);
        assert!(!verify_hashed_password_v3(&hash, "wrong"));
    }

    #[test]
    fn test_verify_sha512() {
        let salt = b"abcdef1234567890"; // 16 bytes
        let hash = make_v3_hash("pass512", salt, 2, 10_000);
        assert!(verify_hashed_password_v3(&hash, "pass512"));
        assert!(!verify_hashed_password_v3(&hash, "wrong512"));
    }

    #[test]
    fn test_verify_bad_format_marker() {
        // Build a buffer with marker 0x00 instead of 0x01
        let mut buf = vec![0x00u8; 45];
        buf[0] = 0x00;
        let encoded = BASE64.encode(&buf);
        assert!(!verify_hashed_password_v3(&encoded, "any"));
    }

    #[test]
    fn test_verify_short_data() {
        // Only 10 bytes — shorter than minimum 13
        let encoded = BASE64.encode(&[0x01u8; 10]);
        assert!(!verify_hashed_password_v3(&encoded, "any"));
    }
}
