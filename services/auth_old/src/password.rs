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
    let iter_count = read_network_u32(&hashed, 5) as u32;
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

    #[test]
    fn test_verify_empty_returns_false() {
        assert!(!verify_hashed_password_v3("", "password"));
    }
}
