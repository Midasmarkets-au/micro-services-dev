use chrono::{DateTime, Utc};
use sqlx::PgPool;

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

/// User roles fetched by joining auth."_UserRole" and auth."_Role".
#[derive(Debug, sqlx::FromRow)]
pub struct UserRoleName {
    #[sqlx(rename = "Name")]
    pub name: Option<String>,
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
        r#"SELECT EXISTS(SELECT 1 FROM central."_IpBlackList" WHERE "Ip" = $1)"#,
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
