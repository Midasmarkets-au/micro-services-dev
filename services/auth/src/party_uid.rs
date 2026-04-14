use chrono::Utc;
use sqlx::PgPool;

/// Mirrors mono's `UserService.GeneratePartyUidAsync()`.
///
/// Format: `9 {sec} {rand_a:02} {rand_t} {rand_b:02} {tenant_suffix}`
///   - sec          = current_year - 2022
///   - rand_a/rand_b = random in [10, 98]
///   - rand_t        = retry counter (1-9), increments on collision, rand_a re-rolls after 9
///   - tenant_suffix = 1=BVI(1), 2=China(10000), 3=Vietnam(10004), 4=10005, 0=other
///
/// Returns the first UID not already present in `core."_CentralParty"`.
pub async fn generate_party_uid(pool: &PgPool, tenant_id: i64) -> Result<i64, sqlx::Error> {
    let suffix = match tenant_id {
        1     => 1u8,
        10000 => 2,
        10004 => 3,
        10005 => 4,
        _     => 0,
    };

    let sec = (Utc::now().format("%Y").to_string().parse::<i64>().unwrap_or(2026) - 2022) as u8;

    // Use current time sub-seconds as a simple entropy source (no external crate needed).
    let nanos = Utc::now().timestamp_subsec_nanos();
    let mut rand_a = (nanos % 89 + 10) as u8;     // [10, 98]
    let mut rand_t: u8 = 1;

    loop {
        let nanos2 = Utc::now().timestamp_subsec_nanos();
        let rand_b = (nanos2 % 89 + 10) as u8;     // [10, 98]

        let uid_str = format!("9{}{:02}{}{:02}{}", sec, rand_a, rand_t, rand_b, suffix);
        let uid: i64 = match uid_str.parse() {
            Ok(v) => v,
            Err(_) => {
                rand_t = rand_t % 9 + 1;
                if rand_t == 1 {
                    rand_a = ((Utc::now().timestamp_subsec_nanos()) % 89 + 10) as u8;
                }
                continue;
            }
        };

        let exists: (bool,) = sqlx::query_as(
            r#"SELECT EXISTS(SELECT 1 FROM core."_CentralParty" WHERE "Uid" = $1)"#,
        )
        .bind(uid)
        .fetch_one(pool)
        .await?;

        if !exists.0 {
            return Ok(uid);
        }

        rand_t += 1;
        if rand_t > 9 {
            rand_t = 1;
            rand_a = ((Utc::now().timestamp_subsec_nanos()) % 89 + 10) as u8;
        }
    }
}
