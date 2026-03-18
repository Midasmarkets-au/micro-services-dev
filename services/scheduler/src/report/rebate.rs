use anyhow::Result;
use chrono::{DateTime, Utc};
use serde::{Deserialize, Serialize};
use sqlx::PgPool;

use super::csv::to_csv_bytes;
use super::request::DateRangeCriteria;

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct RebateRecord {
    pub id: i64,
    pub account_id: i64,
    pub account_number: Option<i64>,
    pub party_id: Option<i64>,
    pub amount: f64,
    pub currency: Option<String>,
    pub stated_on: Option<DateTime<Utc>>,
    pub released_on: Option<DateTime<Utc>>,
    pub created_on: Option<DateTime<Utc>>,
    pub source_type: Option<i32>,
    pub reference: Option<String>,
}

pub async fn generate_rebate_csv(
    pool: &PgPool,
    criteria: &DateRangeCriteria,
    use_released_time: bool,
) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(1));
    let to = criteria.to.unwrap_or_else(Utc::now);

    let sql = if use_released_time {
        r#"SELECT
            r."Id" as id,
            r."AccountId" as account_id,
            a."AccountNumber" as account_number,
            a."PartyId" as party_id,
            r."Amount" as amount,
            r."Currency" as currency,
            r."StatedOn" as stated_on,
            r."ReleasedOn" as released_on,
            r."CreatedOn" as created_on,
            r."SourceType" as source_type,
            r."Reference" as reference
           FROM reb."_Rebate" r
           LEFT JOIN trd."_Account" a ON a."Id" = r."AccountId"
           WHERE r."StatedOn" >= $1 AND r."StatedOn" < $2
           ORDER BY r."StatedOn""#
    } else {
        r#"SELECT
            r."Id" as id,
            r."AccountId" as account_id,
            a."AccountNumber" as account_number,
            a."PartyId" as party_id,
            r."Amount" as amount,
            r."Currency" as currency,
            r."StatedOn" as stated_on,
            r."ReleasedOn" as released_on,
            r."CreatedOn" as created_on,
            r."SourceType" as source_type,
            r."Reference" as reference
           FROM reb."_Rebate" r
           LEFT JOIN trd."_Account" a ON a."Id" = r."AccountId"
           WHERE r."CreatedOn" >= $1 AND r."CreatedOn" < $2
           ORDER BY r."CreatedOn""#
    };

    let records = sqlx::query_as::<_, RebateRecord>(sql)
        .bind(from)
        .bind(to)
        .fetch_all(pool)
        .await?;

    to_csv_bytes(&records)
}
