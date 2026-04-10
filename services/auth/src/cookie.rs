use axum::http::{HeaderMap, HeaderValue, header};

const COOKIE_NAME: &str = "access_token";

/// Build Set-Cookie header for the access token.
/// - HttpOnly: prevents JS access
/// - SameSite=None + Secure: required for cross-origin (frontend on different domain)
/// - Path=/: available for all routes
pub fn build_set_cookie(token: &str, expires_in_secs: i64, is_secure: bool) -> HeaderValue {
    let same_site = if is_secure { "None" } else { "Lax" };
    let secure_attr = if is_secure { "; Secure" } else { "" };
    let cookie = format!(
        "{}={}; Max-Age={}; Path=/; HttpOnly{}; SameSite={}",
        COOKIE_NAME, token, expires_in_secs, secure_attr, same_site
    );
    HeaderValue::from_str(&cookie).expect("cookie header value is always valid ASCII")
}

/// Build Set-Cookie header that clears the access token (logout).
pub fn build_clear_cookie() -> HeaderValue {
    let cookie = format!(
        "{}=; Max-Age=0; Path=/; HttpOnly; SameSite=Lax",
        COOKIE_NAME
    );
    HeaderValue::from_str(&cookie).expect("cookie header value is always valid ASCII")
}

/// Inject Set-Cookie into a HeaderMap.
pub fn set_token_cookie(headers: &mut HeaderMap, token: &str, expires_in_secs: i64, is_secure: bool) {
    headers.insert(
        header::SET_COOKIE,
        build_set_cookie(token, expires_in_secs, is_secure),
    );
}

/// Inject a cookie-clearing Set-Cookie into a HeaderMap.
pub fn clear_token_cookie(headers: &mut HeaderMap) {
    headers.insert(header::SET_COOKIE, build_clear_cookie());
}
