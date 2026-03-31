/// Account daily confirmation report — mirrors ReportService.Account.cs + ReportService.Trade.cs
use anyhow::Result;
use chrono::{DateTime, Utc};
use serde::{Deserialize, Serialize};
use sqlx::PgPool;
use std::collections::HashSet;

use crate::db::auth;
use crate::db::tenant;
use crate::mail::sender::MailSender;
use crate::storage::s3::S3Storage;

#[derive(Debug, Serialize, Deserialize)]
#[allow(dead_code)]
pub struct AccountReportModel {
    pub account_id: i64,
    pub account_number: Option<i64>,
    pub party_id: i64,
    pub date: DateTime<Utc>,
    pub email: Option<String>,
    pub native_name: Option<String>,
    pub language: Option<String>,
    pub html_key: Option<String>,
}

/// Stage 1: Identify accounts with trading activity and create AccountReport entries.
///
/// Mirrors C# `GenerateAccountToSendConfirmationReport`:
///   1. Accounts with closed trades in the period (via `_TradeTransaction`)
///   2. Accounts with open MT5 positions but no closed trades (mirrors C# `hasOpenTrades` check)
pub async fn generate_account_to_send_confirmation_report(
    tenant_pool: &PgPool,
    _auth_pool: &PgPool,
    _tenant_id: i64,
    from: DateTime<Utc>,
    to: DateTime<Utc>,
) -> Result<usize> {
    // Stage 1a: accounts with closed trades in the period
    let closed_trade_accounts: Vec<(i64, i64)> = sqlx::query_as(
        r#"SELECT DISTINCT ta."AccountId", a."PartyId"
           FROM trd."_TradeTransaction" t
           JOIN trd."_TradeAccount" ta ON ta."Id" = t."TradeAccountId"
           JOIN trd."_Account" a ON a."Id" = ta."AccountId"
           WHERE t."CloseAt" >= $1 AND t."CloseAt" < $2"#,
    )
    .bind(from)
    .bind(to)
    .fetch_all(tenant_pool)
    .await?;

    let mut seen: HashSet<i64> = HashSet::new();
    let mut count = 0;

    for (account_id, party_id) in &closed_trade_accounts {
        seen.insert(*account_id);
        if insert_account_report_if_new(tenant_pool, *account_id, *party_id, from).await? {
            count += 1;
        }
    }

    // Stage 1b: accounts with open MT5 positions but no closed trades.
    // Mirrors C# `mt5Ctx.Mt5Positions.Where(x => x.Login == login).AnyAsync()`.
    let first_mt5_service = crate::db::tenant::get_mt5_service_ids_from_central(tenant_pool)
        .await
        .ok()
        .and_then(|ids| ids.into_iter().next());
    if let Some(sid) = first_mt5_service {
        if let Ok(Some(mt5_conn)) =
            crate::db::tenant::get_mt5_connection_string_from_central(tenant_pool, sid).await
        {
        match crate::db::mysql_pool(&mt5_conn).await {
            Ok(mt5_pool) => {
                let all_accounts: Vec<(i64, i64, i64)> =
                    crate::db::tenant::get_active_client_account_logins(tenant_pool).await?;

                // Only check accounts not already captured by closed trades
                let unchecked: Vec<(i64, i64, i64)> = all_accounts
                    .into_iter()
                    .filter(|(account_id, _, _)| !seen.contains(account_id))
                    .collect();

                let logins: Vec<i64> = unchecked.iter().map(|(_, login, _)| *login).collect();

                if !logins.is_empty() {
                    let positions =
                        crate::db::mt5::get_open_positions(&mt5_pool, &logins).await?;
                    let logins_with_positions: HashSet<i64> =
                        positions.iter().map(|p| p.login).collect();

                    for (account_id, login, party_id) in unchecked {
                        if logins_with_positions.contains(&login) {
                            if insert_account_report_if_new(
                                tenant_pool,
                                account_id,
                                party_id,
                                from,
                            )
                            .await?
                            {
                                count += 1;
                            }
                        }
                    }
                }
            }
            Err(e) => {
                tracing::warn!(
                    "AccountDailyConfirmation: could not connect to MT5 for open positions check: {}",
                    e
                );
            }
        }
        } // end if let Ok(Some(mt5_conn))
    } // end if let Some(sid)

    Ok(count)
}

/// Insert an AccountReport row if one doesn't already exist for this account+date.
/// Returns `true` if a new row was inserted.
async fn insert_account_report_if_new(
    pool: &PgPool,
    account_id: i64,
    party_id: i64,
    from: DateTime<Utc>,
) -> Result<bool> {
    let existing: Option<(i64,)> = sqlx::query_as(
        r#"SELECT "Id" FROM sto."_AccountReport"
           WHERE "AccountId" = $1 AND "Date"::date = $2::date"#,
    )
    .bind(account_id)
    .bind(from)
    .fetch_optional(pool)
    .await?;

    if existing.is_some() {
        return Ok(false);
    }

    sqlx::query(
        r#"INSERT INTO sto."_AccountReport" ("AccountId", "PartyId", "Date", "Status")
           VALUES ($1, $2, $3, 0)"#,
    )
    .bind(account_id)
    .bind(party_id)
    .bind(from)
    .execute(pool)
    .await?;

    Ok(true)
}

/// Stage 2: Generate HTML for each pending AccountReport and upload to S3.
pub async fn process_account_report_model(
    tenant_pool: &PgPool,
    s3: &S3Storage,
    tenant_id: i64,
    date: DateTime<Utc>,
) -> Result<i64> {
    let pending = tenant::get_pending_account_reports(tenant_pool, date).await?;
    let _remaining = pending.len() as i64;

    for report in pending {
        let html = generate_account_report_html(tenant_pool, &report, date).await?;
        let key = S3Storage::account_report_html_key(
            tenant_id,
            report.account_id,
            &date.format("%Y-%m-%d").to_string(),
        );
        s3.upload_html(&key, html.into_bytes()).await?;

        sqlx::query(
            r#"UPDATE sto."_AccountReport"
               SET "Status" = 1, "FileName" = $1
               WHERE "Id" = $2"#,
        )
        .bind(&key)
        .bind(report.id)
        .execute(tenant_pool)
        .await?;
    }

    // Return remaining unprocessed count
    let remaining = tenant::count_pending_account_reports(tenant_pool, date).await?;
    Ok(remaining)
}

async fn generate_account_report_html(
    pool: &PgPool,
    report: &tenant::AccountReport,
    date: DateTime<Utc>,
) -> Result<String> {
    // Fetch trades for this account on the given date
    let trades: Vec<(Option<i64>, Option<String>, Option<f64>, Option<f64>)> = sqlx::query_as(
        r#"SELECT t."Ticket", t."Symbol", t."Profit", t."Volume"
           FROM trd."_TradeTransaction" t
           JOIN trd."_TradeAccount" ta ON ta."Id" = t."TradeAccountId"
           WHERE ta."AccountId" = $1
           AND t."CloseAt"::date = $2::date
           ORDER BY t."CloseAt""#,
    )
    .bind(report.account_id)
    .bind(date)
    .fetch_all(pool)
    .await?;

    let trade_rows: String = trades
        .iter()
        .map(|(ticket, symbol, profit, volume)| {
            format!(
                "<tr><td>{}</td><td>{}</td><td>{:.2}</td><td>{:.2}</td></tr>",
                ticket.map(|t| t.to_string()).unwrap_or_default(),
                symbol.as_deref().unwrap_or(""),
                profit.unwrap_or(0.0),
                volume.unwrap_or(0.0),
            )
        })
        .collect();

    let html = format!(
        r#"<!DOCTYPE html>
<html>
<head><meta charset="UTF-8"><title>Account Daily Report</title></head>
<body>
<h2>Account Daily Confirmation Report</h2>
<p>Date: {date}</p>
<p>Account ID: {account_id}</p>
<table border="1">
<thead><tr><th>Ticket</th><th>Symbol</th><th>Profit</th><th>Volume</th></tr></thead>
<tbody>{trade_rows}</tbody>
</table>
</body>
</html>"#,
        date = date.format("%Y-%m-%d"),
        account_id = report.account_id,
        trade_rows = trade_rows,
    );

    Ok(html)
}

/// Stage 3: Send emails for processed AccountReports.
pub async fn process_send_account_daily_report_email(
    tenant_pool: &PgPool,
    auth_pool: &PgPool,
    s3: &S3Storage,
    mail: &MailSender,
    date: DateTime<Utc>,
) -> Result<i64> {
    // Find reports that have HTML but haven't been emailed (Status = 1)
    let ready: Vec<tenant::AccountReport> = sqlx::query_as::<_, tenant::AccountReport>(
        r#"SELECT "Id", "AccountId", "PartyId", "Date", "Status", "FileName", "Email"
           FROM sto."_AccountReport"
           WHERE "Date"::date = $1::date AND "Status" = 1"#,
    )
    .bind(date)
    .fetch_all(tenant_pool)
    .await?;

    for report in &ready {
        let user = auth::get_user_by_party_id(auth_pool, report.party_id).await?;
        let email = match user.as_ref().and_then(|u| u.email.as_ref()) {
            Some(e) => e.clone(),
            None => continue,
        };

        let html_key = match &report.file_name {
            Some(k) => k.clone(),
            None => continue,
        };

        let html_bytes = s3.download(&html_key).await?;
        let html = String::from_utf8_lossy(&html_bytes).to_string();

        let _native_name = user
            .as_ref()
            .and_then(|u| u.native_name.as_ref())
            .cloned()
            .unwrap_or_default();

        let subject = format!("Account Daily Report - {}", date.format("%Y-%m-%d"));
        mail.send(&email, &subject, &html).await?;

        sqlx::query(
            r#"UPDATE sto."_AccountReport" SET "Status" = 2, "Email" = $1 WHERE "Id" = $2"#,
        )
        .bind(&email)
        .bind(report.id)
        .execute(tenant_pool)
        .await?;
    }

    // Return remaining unsent count
    let remaining: (i64,) = sqlx::query_as(
        r#"SELECT COUNT(*) FROM sto."_AccountReport"
           WHERE "Date"::date = $1::date AND "Status" < 2"#,
    )
    .bind(date)
    .fetch_one(tenant_pool)
    .await?;

    Ok(remaining.0)
}
