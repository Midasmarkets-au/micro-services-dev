#![allow(dead_code)]

use anyhow::Result;
use serde::{Deserialize, Serialize};
use sqlx::PgPool;

/// Mirrors auth.AspNetUsers (minimal fields needed for report enrichment)
#[derive(Debug, Clone, sqlx::FromRow, Serialize, Deserialize)]
pub struct AuthUser {
    #[sqlx(rename = "Id")]
    pub id: String,
    #[sqlx(rename = "Email")]
    pub email: Option<String>,
    #[sqlx(rename = "UserName")]
    pub user_name: Option<String>,
    #[sqlx(rename = "PartyId")]
    pub party_id: Option<i64>,
    #[sqlx(rename = "NativeName")]
    pub native_name: Option<String>,
    #[sqlx(rename = "Language")]
    pub language: Option<String>,
}

pub async fn get_users_by_party_ids(
    pool: &PgPool,
    party_ids: &[i64],
) -> Result<Vec<AuthUser>> {
    // sqlx doesn't support IN with array directly in all versions; use ANY
    let rows = sqlx::query_as::<_, AuthUser>(
        r#"SELECT "Id", "Email", "UserName", "PartyId", "NativeName", "Language"
           FROM auth."AspNetUsers"
           WHERE "PartyId" = ANY($1)"#,
    )
    .bind(party_ids)
    .fetch_all(pool)
    .await?;
    Ok(rows)
}

pub async fn get_user_by_party_id(pool: &PgPool, party_id: i64) -> Result<Option<AuthUser>> {
    let row = sqlx::query_as::<_, AuthUser>(
        r#"SELECT "Id", "Email", "UserName", "PartyId", "NativeName", "Language"
           FROM auth."AspNetUsers"
           WHERE "PartyId" = $1
           LIMIT 1"#,
    )
    .bind(party_id)
    .fetch_optional(pool)
    .await?;
    Ok(row)
}

/// Check if a party has a role (for Sales role detection)
pub async fn has_role(pool: &PgPool, party_id: i64, role_name: &str) -> Result<bool> {
    let row: Option<(i64,)> = sqlx::query_as(
        r#"SELECT COUNT(*)
           FROM auth."AspNetUserRoles" ur
           JOIN auth."AspNetRoles" r ON r."Id" = ur."RoleId"
           JOIN auth."AspNetUsers" u ON u."Id" = ur."UserId"
           WHERE u."PartyId" = $1 AND r."Name" = $2"#,
    )
    .bind(party_id)
    .bind(role_name)
    .fetch_optional(pool)
    .await?;
    Ok(row.map(|r| r.0 > 0).unwrap_or(false))
}
