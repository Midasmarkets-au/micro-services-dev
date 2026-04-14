use serde::Deserialize;
use tracing::warn;

#[derive(Deserialize)]
struct VerificationCheckResponse {
    status: String,
}

/// Send an OTP via Twilio Verify (SMS channel).
/// Returns Ok(()) on success, Err on HTTP/network failure.
pub async fn send_otp(
    account_sid: &str,
    auth_token: &str,
    service_sid: &str,
    to: &str,
) -> Result<(), String> {
    let url = format!(
        "https://verify.twilio.com/v2/Services/{}/Verifications",
        service_sid
    );
    let client = reqwest::Client::new();
    let resp = client
        .post(&url)
        .basic_auth(account_sid, Some(auth_token))
        .form(&[("To", to), ("Channel", "sms")])
        .send()
        .await
        .map_err(|e| e.to_string())?;

    if resp.status().is_success() {
        Ok(())
    } else {
        let body = resp.text().await.unwrap_or_default();
        warn!("Twilio send_otp failed: {}", body);
        Err(body)
    }
}

/// Verify an OTP via Twilio Verify.
/// Returns true if the code is "approved".
pub async fn verify_otp(
    account_sid: &str,
    auth_token: &str,
    service_sid: &str,
    to: &str,
    code: &str,
) -> bool {
    let url = format!(
        "https://verify.twilio.com/v2/Services/{}/VerificationCheck",
        service_sid
    );
    let client = reqwest::Client::new();
    let resp = match client
        .post(&url)
        .basic_auth(account_sid, Some(auth_token))
        .form(&[("To", to), ("Code", code)])
        .send()
        .await
    {
        Ok(r) => r,
        Err(e) => {
            warn!("Twilio verify_otp request failed: {}", e);
            return false;
        }
    };

    match resp.json::<VerificationCheckResponse>().await {
        Ok(body) => body.status == "approved",
        Err(e) => {
            warn!("Twilio verify_otp parse failed: {}", e);
            false
        }
    }
}
