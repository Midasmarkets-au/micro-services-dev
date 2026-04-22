use chrono::{DateTime, Utc};
use sqlx::PgPool;
use uuid::Uuid;

/// Mirrors auth."_User" from AuthDbContext (ASP.NET Identity + custom fields).
#[derive(Debug, sqlx::FromRow)]
#[allow(dead_code)]
pub struct User {
    #[sqlx(rename = "Id")]
    pub id: i64,
    #[sqlx(rename = "Uid")]
    pub uid: i64,
    #[sqlx(rename = "PartyId")]
    pub party_id: i64,
    #[sqlx(rename = "TenantId")]
    pub tenant_id: i64,
    #[sqlx(rename = "Email")]
    pub email: Option<String>,
    #[sqlx(rename = "NormalizedEmail")]
    pub normalized_email: Option<String>,
    #[sqlx(rename = "EmailConfirmed")]
    pub email_confirmed: bool,
    #[sqlx(rename = "PasswordHash")]
    pub password_hash: Option<String>,
    #[sqlx(rename = "Status")]
    pub status: i16,
    #[sqlx(rename = "LockoutEnabled")]
    pub lockout_enabled: bool,
    #[sqlx(rename = "LockoutEnd")]
    pub lockout_end: Option<DateTime<Utc>>,
    #[sqlx(rename = "TwoFactorEnabled")]
    pub two_factor_enabled: bool,
    #[sqlx(rename = "NativeName")]
    pub native_name: String,
    #[sqlx(rename = "FirstName")]
    pub first_name: String,
    #[sqlx(rename = "LastName")]
    pub last_name: String,
    #[sqlx(rename = "Language")]
    pub language: String,
    #[sqlx(rename = "LastLoginOn")]
    pub last_login_on: Option<DateTime<Utc>>,
    #[sqlx(rename = "LastLoginIp")]
    pub last_login_ip: String,
}

impl User {
    /// Mirrors mono's User.GuessUserName() — prefer NativeName, fall back to FirstName+LastName, then email.
    pub fn guess_display_name(&self) -> String {
        if !self.native_name.is_empty() {
            return self.native_name.clone();
        }
        let full = format!("{} {}", self.first_name, self.last_name).trim().to_string();
        if !full.is_empty() {
            return full;
        }
        self.email.clone().unwrap_or_default()
    }
}

/// User roles fetched by joining auth."_UserRole" and auth."_Role".
#[derive(Debug, sqlx::FromRow)]
pub struct UserRoleName {
    #[sqlx(rename = "Name")]
    pub name: Option<String>,
}

pub async fn find_user_by_id(pool: &PgPool, user_id: i64) -> Result<Option<User>, sqlx::Error> {
    sqlx::query_as::<_, User>(
        r#"
        SELECT "Id", "Uid", "PartyId", "TenantId", "Email", "NormalizedEmail",
               "EmailConfirmed", "PasswordHash", "Status",
               "LockoutEnabled", "LockoutEnd", "TwoFactorEnabled",
               "NativeName", "FirstName", "LastName", "Language",
               "LastLoginOn", "LastLoginIp"
        FROM auth."_User"
        WHERE "Id" = $1
        "#,
    )
    .bind(user_id)
    .fetch_optional(pool)
    .await
}

pub async fn find_users_by_email(pool: &PgPool, email: &str) -> Result<Vec<User>, sqlx::Error> {
    sqlx::query_as::<_, User>(
        r#"
        SELECT "Id", "Uid", "PartyId", "TenantId", "Email", "NormalizedEmail",
               "EmailConfirmed", "PasswordHash", "Status",
               "LockoutEnabled", "LockoutEnd", "TwoFactorEnabled",
               "NativeName", "FirstName", "LastName", "Language",
               "LastLoginOn", "LastLoginIp"
        FROM auth."_User"
        WHERE "Status" = 0 AND LOWER("Email") = $1
        "#,
    )
    .bind(email)
    .fetch_all(pool)
    .await
}

pub async fn is_ip_blocked(pool: &PgPool, ip: &str) -> Result<bool, sqlx::Error> {
    let row: (bool,) = sqlx::query_as(
        r#"SELECT EXISTS(SELECT 1 FROM core."_IpBlackList" WHERE "Ip" = $1)"#,
    )
    .bind(ip)
    .fetch_one(pool)
    .await?;
    Ok(row.0)
}

pub async fn get_user_roles(pool: &PgPool, user_id: i64) -> Result<Vec<String>, sqlx::Error> {
    let rows = sqlx::query_as::<_, UserRoleName>(
        r#"
        SELECT r."Name"
        FROM auth."_UserRole" ur
        JOIN auth."_Role" r ON ur."RoleId" = r."Id"
        WHERE ur."UserId" = $1
        "#,
    )
    .bind(user_id)
    .fetch_all(pool)
    .await?;
    Ok(rows
        .into_iter()
        .filter_map(|r| r.name)
        .collect())
}

pub async fn update_password_hash(
    pool: &PgPool,
    user_id: i64,
    hash: &str,
) -> Result<(), sqlx::Error> {
    sqlx::query(r#"UPDATE auth."_User" SET "PasswordHash" = $1 WHERE "Id" = $2"#)
        .bind(hash)
        .bind(user_id)
        .execute(pool)
        .await?;
    Ok(())
}

pub async fn update_password_hash_by_email(
    pool: &PgPool,
    email: &str,
    hash: &str,
) -> Result<(), sqlx::Error> {
    sqlx::query(r#"UPDATE auth."_User" SET "PasswordHash" = $1 WHERE LOWER("Email") = $2"#)
        .bind(hash)
        .bind(email)
        .execute(pool)
        .await?;
    Ok(())
}

pub async fn update_two_factor_enabled(
    pool: &PgPool,
    user_id: i64,
    enabled: bool,
) -> Result<(), sqlx::Error> {
    sqlx::query(r#"UPDATE auth."_User" SET "TwoFactorEnabled" = $1 WHERE "Id" = $2"#)
        .bind(enabled)
        .bind(user_id)
        .execute(pool)
        .await?;
    Ok(())
}

pub async fn get_authenticator_key(
    pool: &PgPool,
    user_id: i64,
) -> Result<Option<String>, sqlx::Error> {
    let row: Option<(String,)> = sqlx::query_as(
        r#"SELECT "Value" FROM auth."_UserToken"
           WHERE "UserId" = $1
             AND "LoginProvider" = '[AspNetUserStoreProvider]'
             AND "Name" = 'AuthenticatorKey'"#,
    )
    .bind(user_id)
    .fetch_optional(pool)
    .await?;
    Ok(row.map(|(v,)| v))
}

pub async fn upsert_authenticator_key(
    pool: &PgPool,
    user_id: i64,
    key: &str,
) -> Result<(), sqlx::Error> {
    sqlx::query(
        r#"INSERT INTO auth."_UserToken" ("UserId", "LoginProvider", "Name", "Value")
           VALUES ($1, '[AspNetUserStoreProvider]', 'AuthenticatorKey', $2)
           ON CONFLICT ("UserId", "LoginProvider", "Name") DO UPDATE SET "Value" = EXCLUDED."Value""#,
    )
    .bind(user_id)
    .bind(key)
    .execute(pool)
    .await?;
    Ok(())
}

pub async fn update_last_login(
    pool: &PgPool,
    user_id: i64,
    ip: &str,
) -> Result<(), sqlx::Error> {
    sqlx::query(
        r#"UPDATE auth."_User" SET "LastLoginOn" = NOW(), "LastLoginIp" = $1 WHERE "Id" = $2"#,
    )
    .bind(ip)
    .bind(user_id)
    .execute(pool)
    .await?;
    Ok(())
}

// ─── Registration helpers ─────────────────────────────────────────────────────

/// Parameters for creating a new user during registration.
pub struct NewUser<'a> {
    pub uid: i64,
    pub party_id: i64,
    pub tenant_id: i64,
    pub email: &'a str,
    pub password_hash: &'a str,
    pub first_name: &'a str,
    pub last_name: &'a str,
    pub native_name: &'a str,
    pub phone: &'a str,
    pub phone_confirmed: bool,
    pub ccc: &'a str,
    pub country_code: &'a str,
    pub currency: &'a str,
    pub refer_code: &'a str,
    pub language: &'a str,
    pub register_ip: &'a str,
}

/// INSERT a new user into auth."_User". Returns the generated Id.
pub async fn insert_user(pool: &PgPool, u: &NewUser<'_>) -> Result<i64, sqlx::Error> {
    let email_upper = u.email.to_uppercase();
    let security_stamp = Uuid::new_v4().to_string().to_uppercase();
    let concurrency_stamp = Uuid::new_v4().to_string();
    let now = Utc::now();

    // Epoch date used for DateOnly columns that have no meaningful default (Birthday, IdIssuedOn, IdExpireOn).
    // mono uses C# default(DateOnly) = 0001-01-01; PostgreSQL date minimum is 4713-01-01 BC,
    // but EF Core maps default(DateOnly) to '0001-01-01'. Use the same value.
    let epoch_date = chrono::NaiveDate::from_ymd_opt(1, 1, 1).unwrap();

    let id: i64 = sqlx::query_scalar(
        r#"
        INSERT INTO auth."_User" (
            "Uid", "PartyId", "TenantId",
            "UserName", "NormalizedUserName",
            "Email",    "NormalizedEmail",
            "EmailConfirmed", "PasswordHash",
            "SecurityStamp", "ConcurrencyStamp",
            "PhoneNumber", "PhoneNumberConfirmed",
            "TwoFactorEnabled", "LockoutEnabled",
            "AccessFailedCount", "Status",
            "FirstName", "LastName", "NativeName", "Language",
            "CCC", "CountryCode", "Currency", "ReferCode",
            "ReferrerPartyId",
            "Avatar", "TimeZone", "Citizen", "Address",
            "IdType", "IdNumber", "IdIssuer",
            "Birthday", "IdIssuedOn", "IdExpireOn",
            "Gender", "ReferPath",
            "RegisteredIp", "LastLoginIp",
            "CreatedOn", "UpdatedOn"
        ) VALUES (
            $1,  $2,  $3,
            $4,  $5,
            $6,  $7,
            true, $8,
            $9,  $10,
            $11, $12,
            false, true,
            0, 0,
            $13, $14, $15, $16,
            $17, $18, $19, $20,
            0,
            '', '', '', '',
            0, '', '',
            $21, $21, $21,
            0, '',
            $22, '',
            $23, $23
        )
        RETURNING "Id"
        "#,
    )
    .bind(u.uid)
    .bind(u.party_id)
    .bind(u.tenant_id)
    .bind(u.email)
    .bind(&email_upper)
    .bind(u.email)
    .bind(&email_upper)
    .bind(u.password_hash)
    .bind(&security_stamp)
    .bind(&concurrency_stamp)
    .bind(u.phone)
    .bind(u.phone_confirmed)
    .bind(u.first_name)
    .bind(u.last_name)
    .bind(u.native_name)
    .bind(u.language)
    .bind(u.ccc)
    .bind(u.country_code)
    .bind(u.currency)
    .bind(u.refer_code)
    .bind(epoch_date)       // Birthday / IdIssuedOn / IdExpireOn ($21)
    .bind(u.register_ip)    // RegisteredIp ($22)
    .bind(now)              // CreatedOn / UpdatedOn ($23)
    .fetch_one(pool)
    .await?;

    Ok(id)
}

/// INSERT a role into auth."_UserRole" by role name (e.g. "Guest").
/// Silently does nothing if the role does not exist.
pub async fn insert_user_role_by_name(
    pool: &PgPool,
    user_id: i64,
    role_name: &str,
) -> Result<(), sqlx::Error> {
    sqlx::query(
        r#"
        INSERT INTO auth."_UserRole" ("UserId", "RoleId")
        SELECT $1, "Id" FROM auth."_Role" WHERE "Name" = $2
        ON CONFLICT DO NOTHING
        "#,
    )
    .bind(user_id)
    .bind(role_name)
    .execute(pool)
    .await?;
    Ok(())
}

/// INSERT a new CentralParty into core."_CentralParty". Returns the generated Id.
pub async fn insert_central_party(
    pool: &PgPool,
    uid: i64,
    email: &str,
    name: &str,
    site_id: i32,
    tenant_id: i64,
) -> Result<i64, sqlx::Error> {
    let now = Utc::now();
    let id: i64 = sqlx::query_scalar(
        r#"
        INSERT INTO core."_CentralParty" (
            "Uid", "Email", "Name", "NativeName",
            "Code", "Note", "SiteId", "TenantId",
            "CreatedOn", "UpdatedOn"
        ) VALUES ($1, $2, $3, $3, '', '', $4, $5, $6, $6)
        RETURNING "Id"
        "#,
    )
    .bind(uid)
    .bind(email)
    .bind(name)
    .bind(site_id)
    .bind(tenant_id)
    .bind(now)
    .fetch_one(pool)
    .await?;

    Ok(id)
}

/// Resolve tenant_id from a referral code (reads core."_CentralReferralCode").
/// Returns (tenant_id) or None if the code doesn't exist.
pub async fn find_tenant_by_refer_code(
    pool: &PgPool,
    code: &str,
) -> Result<Option<i64>, sqlx::Error> {
    let row: Option<(i64,)> = sqlx::query_as(
        r#"
        SELECT t."Id"
        FROM core."_CentralReferralCode" rc
        JOIN core."_Tenant" t ON rc."TenantId" = t."Id"
        WHERE rc."Code" = $1
        LIMIT 1
        "#,
    )
    .bind(code)
    .fetch_optional(pool)
    .await?;
    Ok(row.map(|(id,)| id))
}

/// Check if a tenant exists by Id.
pub async fn tenant_exists(pool: &PgPool, tenant_id: i64) -> Result<bool, sqlx::Error> {
    let row: (bool,) = sqlx::query_as(
        r#"SELECT EXISTS(SELECT 1 FROM core."_Tenant" WHERE "Id" = $1)"#,
    )
    .bind(tenant_id)
    .fetch_one(pool)
    .await?;
    Ok(row.0)
}

// ─── Rollback helpers ─────────────────────────────────────────────────────────

pub async fn delete_user(pool: &PgPool, user_id: i64) -> Result<(), sqlx::Error> {
    sqlx::query(r#"DELETE FROM auth."_UserRole" WHERE "UserId" = $1"#)
        .bind(user_id)
        .execute(pool)
        .await?;
    sqlx::query(r#"DELETE FROM auth."_User" WHERE "Id" = $1"#)
        .bind(user_id)
        .execute(pool)
        .await?;
    Ok(())
}

pub async fn delete_central_party(pool: &PgPool, party_id: i64) -> Result<(), sqlx::Error> {
    sqlx::query(r#"DELETE FROM core."_CentralParty" WHERE "Id" = $1"#)
        .bind(party_id)
        .execute(pool)
        .await?;
    Ok(())
}
