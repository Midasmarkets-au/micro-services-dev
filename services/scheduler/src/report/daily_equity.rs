/// DailyEquity report: mirrors ReportService.DailyEquity.cs
/// Groups by Office (SalesAccountId) and Currency (USD / Wallet / USC).
/// PostgreSQL supplies transaction aggregates; MySQL (MT5) supplies equity / lots / P&L.
use anyhow::Result;
use chrono::{DateTime, NaiveDate, Utc};
use sqlx::PgPool;
use std::collections::HashMap;
use tracing::{info, warn};

use crate::AppContext;

// ─── PostgreSQL row ───────────────────────────────────────────────────────────

#[derive(Debug, sqlx::FromRow)]
struct PgRow {
    #[sqlx(rename = "Office")]
    office: String,
    #[sqlx(rename = "Currency")]
    currency: String,
    #[sqlx(rename = "AccountList")]
    account_list: Option<String>,
    #[sqlx(rename = "ServiceId")]
    service_id: i32,
    #[sqlx(rename = "NewAccUser")]
    new_acc_user: i64,
    #[sqlx(rename = "PrevEquity")]
    prev_equity: f64,
    #[sqlx(rename = "CurrEquity")]
    curr_equity: f64,
    #[sqlx(rename = "MarginIn")]
    margin_in: f64,
    #[sqlx(rename = "MarginOut")]
    margin_out: f64,
    #[sqlx(rename = "TransferIn")]
    transfer_in: f64,
    #[sqlx(rename = "TransferOut")]
    transfer_out: f64,
    #[sqlx(rename = "Credit")]
    credit: f64,
    #[sqlx(rename = "Adjust")]
    adjust: f64,
    #[sqlx(rename = "Rebate")]
    rebate: f64,
}

// ─── MT5 aggregated result ────────────────────────────────────────────────────

#[derive(Debug)]
struct Mt5Row {
    office: String,
    currency: String,
    previous_equity: f64,
    current_equity: f64,
    lots: f64,
    pl: f64,
}

// ─── Final output record ──────────────────────────────────────────────────────

#[derive(Debug, Default, Clone)]
struct OutputRecord {
    currency: String,
    office: String,
    new_user: i64,
    new_acc: i64,
    previous_equity: f64,
    current_equity: f64,
    margin_in: f64,
    margin_out: f64,
    transfer: f64,
    credit: f64,
    adjust: f64,
    rebate: f64,
    net_in_out: f64,
    lots: f64,
    pl: f64,
    estimates_net_pl: f64,
}

// ─── Public entry point ───────────────────────────────────────────────────────

pub async fn generate_daily_equity_csv(
    ctx: &AppContext,
    tenant_pool: &PgPool,
    from: DateTime<Utc>,
    to: DateTime<Utc>,
    use_closing_time: bool,
    closed_on_from: DateTime<Utc>,
    closed_on_to: DateTime<Utc>,
) -> Result<Vec<u8>> {
    let prev_dt: NaiveDate = from.date_naive();
    let curr_dt: NaiveDate = to.date_naive();

    let pg_rows = query_postgres(
        tenant_pool,
        from,
        to,
        prev_dt,
        curr_dt,
        use_closing_time,
        closed_on_from,
        closed_on_to,
    )
    .await?;

    info!(
        "[DailyEquity PG] {} rows, from={} to={}",
        pg_rows.len(),
        from,
        to
    );

    let mt5_rows = query_mt5(ctx, tenant_pool, &pg_rows, to).await;
    let merged = merge(pg_rows, mt5_rows);
    let final_records = build_output(merged);

    Ok(render_csv(&final_records, to))
}

// ─── PostgreSQL CTE query ─────────────────────────────────────────────────────

async fn query_postgres(
    pool: &PgPool,
    from: DateTime<Utc>,
    to: DateTime<Utc>,
    prev_dt: NaiveDate,
    curr_dt: NaiveDate,
    use_closing_time: bool,
    closed_on_from: DateTime<Utc>,
    closed_on_to: DateTime<Utc>,
) -> Result<Vec<PgRow>> {
    let closing_filter = if use_closing_time {
        r#"AND tr."ClosedOn" >= $5 AND tr."ClosedOn" <= $6"#
    } else {
        ""
    };

    let sql = format!(
        r#"
WITH
    params AS (
        SELECT
            $1::timestamptz AS fromDT,
            $2::timestamptz AS toDT,
            $3::date        AS prevDT,
            $4::date        AS currDT,
            $5::timestamptz AS closedOnFrom,
            $6::timestamptz AS closedOnTo
    ),
    client_accounts AS (
        SELECT
            a."Id" AS account_id,
            a."AccountNumber",
            a."ServiceId",
            a."CurrencyId",
            a."WalletId",
            a."PartyId",
            a."CreatedOn",
            COALESCE(sa."Code", 'NO_SALES') AS sales_code
        FROM trd."_Account" a
        LEFT JOIN trd."_Account" sa ON a."SalesAccountId" = sa."Id"
        WHERE a."Role" = 400
    ),
    agent_accounts AS (
        SELECT
            a."Id" AS account_id,
            a."CurrencyId",
            a."PartyId",
            a."WalletId",
            a."AccountNumber",
            a."ServiceId",
            COALESCE(sa."Code", 'NO_SALES') AS sales_code
        FROM trd."_Account" a
        LEFT JOIN trd."_Account" sa ON a."SalesAccountId" = sa."Id"
        WHERE a."Role" = 300
    ),
    client_account_list AS (
        SELECT
            sales_code,
            "CurrencyId",
            STRING_AGG("AccountNumber"::text, ',' ORDER BY "AccountNumber") AS account_list,
            COALESCE(MAX(NULLIF("ServiceId", 0)), 0) AS service_id
        FROM client_accounts
        GROUP BY sales_code, "CurrencyId"
    ),
    agent_wallet_list AS (
        SELECT
            aa.sales_code,
            STRING_AGG(DISTINCT w."Number", ',' ORDER BY w."Number") AS wallet_list
        FROM agent_accounts aa
        JOIN acct."_Wallet" w ON aa."WalletId" = w."Id"
        GROUP BY aa.sales_code
    ),
    new_accounts AS (
        SELECT sales_code, "CurrencyId", COUNT(*) AS new_acc_count
        FROM client_accounts
        CROSS JOIN params p
        WHERE "CreatedOn" >= p.fromDT AND "CreatedOn" < p.toDT
        GROUP BY sales_code, "CurrencyId"
    ),
    new_users AS (
        SELECT
            COALESCE(sa."Code", 'NO_SALES') AS sales_code,
            COUNT(DISTINCT pty."Id") AS new_user_count
        FROM core."_Party" pty
        JOIN trd."_Account" a ON pty."Id" = a."PartyId" AND a."Role" = 400
        LEFT JOIN trd."_Account" sa ON a."SalesAccountId" = sa."Id"
        CROSS JOIN params p
        WHERE pty."CreatedOn" >= p.fromDT AND pty."CreatedOn" < p.toDT
        GROUP BY sa."Code"
    ),
    wallet_prev_equity AS (
        SELECT sales_code, SUM(balance) AS total_balance
        FROM (
            SELECT DISTINCT ON (wds."WalletId")
                CASE
                    WHEN a."Role" = 100 THEN a."Code"
                    ELSE COALESCE(sa."Code", 'NO_SALES')
                END AS sales_code,
                wds."Balance" / 1000000.0 AS balance
            FROM acct."_WalletDailySnapshot" wds
            JOIN acct."_Wallet" w ON wds."WalletId" = w."Id"
            LEFT JOIN trd."_Account" a ON w."PartyId" = a."PartyId"
                AND a."Role" IN (100, 300, 400)
                AND a."CurrencyId" = 840
            LEFT JOIN trd."_Account" sa ON a."SalesAccountId" = sa."Id"
            CROSS JOIN params p
            WHERE w."CurrencyId" = 840
              AND wds."SnapshotDate"::date = p.prevDT::date
            ORDER BY wds."WalletId", wds."SnapshotDate" DESC
        ) sub
        GROUP BY sales_code
    ),
    wallet_curr_equity AS (
        SELECT sales_code, SUM(balance) AS total_balance
        FROM (
            SELECT DISTINCT ON (wds."WalletId")
                CASE
                    WHEN a."Role" = 100 THEN a."Code"
                    ELSE COALESCE(sa."Code", 'NO_SALES')
                END AS sales_code,
                wds."Balance" / 1000000.0 AS balance
            FROM acct."_WalletDailySnapshot" wds
            JOIN acct."_Wallet" w ON wds."WalletId" = w."Id"
            LEFT JOIN trd."_Account" a ON w."PartyId" = a."PartyId"
                AND a."Role" IN (100, 300, 400)
                AND a."CurrencyId" = 840
            LEFT JOIN trd."_Account" sa ON a."SalesAccountId" = sa."Id"
            CROSS JOIN params p
            WHERE w."CurrencyId" = 840
              AND wds."SnapshotDate"::date = p.currDT::date
            ORDER BY wds."WalletId", wds."SnapshotDate" DESC
        ) sub
        GROUP BY sales_code
    ),
    margin_in_deposit AS (
        SELECT ca.sales_code, ca."CurrencyId", SUM(d."Amount") AS total_amount
        FROM core."_Matter" m
        JOIN acct."_Deposit" d ON d."Id" = m."Id"
        JOIN client_accounts ca ON d."TargetAccountId" = ca.account_id
        CROSS JOIN params p
        WHERE m."StateId" IN (350, 351)
          AND m."StatedOn" >= p.fromDT AND m."StatedOn" < p.toDT
        GROUP BY ca.sales_code, ca."CurrencyId"
    ),
    margin_in_acc2acc AS (
        SELECT ca.sales_code, ca."CurrencyId", SUM(t."Amount") AS total_amount
        FROM core."_Matter" m
        JOIN acct."_Transaction" t ON t."Id" = m."Id"
        JOIN client_accounts ca ON t."TargetAccountId" = ca.account_id
        CROSS JOIN params p
        WHERE m."StateId" = 250
          AND t."SourceAccountType" = 2
          AND t."TargetAccountType" = 2
          AND m."StatedOn" >= p.fromDT AND m."StatedOn" < p.toDT
        GROUP BY ca.sales_code, ca."CurrencyId"
    ),
    margin_in AS (
        SELECT sales_code, "CurrencyId", SUM(total_amount) AS total_amount
        FROM (
            SELECT * FROM margin_in_deposit
            UNION ALL
            SELECT * FROM margin_in_acc2acc
        ) combined
        GROUP BY sales_code, "CurrencyId"
    ),
    margin_out_withdrawal AS (
        WITH withdrawal_first_approval AS (
            SELECT a."MatterId", MIN(a."PerformedOn") AS first_approval_time
            FROM core."_Activity" a
            WHERE a."ToStateId" = 420
            GROUP BY a."MatterId"
        )
        SELECT ca.sales_code, ca."CurrencyId", SUM(w."Amount") AS total_amount
        FROM core."_Matter" m
        JOIN acct."_Withdrawal" w ON w."Id" = m."Id"
        JOIN withdrawal_first_approval wfa ON wfa."MatterId" = m."Id"
        JOIN client_accounts ca ON w."SourceAccountId" = ca.account_id
        CROSS JOIN params p
        WHERE w."SourceAccountId" IS NOT NULL
          AND wfa.first_approval_time >= p.fromDT
          AND wfa.first_approval_time < p.toDT
        GROUP BY ca.sales_code, ca."CurrencyId"
    ),
    margin_out_acc2acc AS (
        SELECT ca.sales_code, ca."CurrencyId", SUM(t."Amount") AS total_amount
        FROM core."_Matter" m
        JOIN acct."_Transaction" t ON t."Id" = m."Id"
        JOIN client_accounts ca ON t."SourceAccountId" = ca.account_id
        CROSS JOIN params p
        WHERE m."StateId" = 250
          AND t."SourceAccountType" = 2
          AND t."TargetAccountType" = 2
          AND m."StatedOn" >= p.fromDT AND m."StatedOn" < p.toDT
        GROUP BY ca.sales_code, ca."CurrencyId"
    ),
    margin_out AS (
        SELECT sales_code, "CurrencyId", SUM(total_amount) AS total_amount
        FROM (
            SELECT * FROM margin_out_withdrawal
            UNION ALL
            SELECT * FROM margin_out_acc2acc
        ) combined
        GROUP BY sales_code, "CurrencyId"
    ),
    wallet_margin_out AS (
        WITH wallet_withdrawal_first_approval AS (
            SELECT a."MatterId", MIN(a."PerformedOn") AS first_approval_time
            FROM core."_Activity" a
            WHERE a."ToStateId" = 420
            GROUP BY a."MatterId"
        ),
        wallet_withdrawal_with_sales AS (
            SELECT DISTINCT ON (wd."Id")
                wd."Id" AS withdrawal_id,
                w."CurrencyId",
                wd."Amount",
                COALESCE(sa."Code", 'NO_SALES') AS sales_code,
                wfa.first_approval_time
            FROM core."_Matter" m
            JOIN acct."_Withdrawal" wd ON wd."Id" = m."Id"
            JOIN wallet_withdrawal_first_approval wfa ON wfa."MatterId" = m."Id"
            JOIN acct."_Wallet" w ON wd."SourceWalletId" = w."Id"
            LEFT JOIN trd."_Account" a ON w."PartyId" = a."PartyId"
                AND a."Role" IN (300, 400)
                AND a."CurrencyId" = w."CurrencyId"
            LEFT JOIN trd."_Account" sa ON a."SalesAccountId" = sa."Id"
            CROSS JOIN params p
            WHERE wd."SourceAccountId" IS NULL
              AND wfa.first_approval_time >= p.fromDT
              AND wfa.first_approval_time < p.toDT
            ORDER BY wd."Id", a."Id"
        )
        SELECT sales_code, "CurrencyId", SUM("Amount") AS total_amount
        FROM wallet_withdrawal_with_sales
        GROUP BY sales_code, "CurrencyId"
    ),
    transfer_in_acc AS (
        SELECT ca.sales_code, ca."CurrencyId", SUM(t."Amount") AS total_amount
        FROM core."_Matter" m
        JOIN acct."_Transaction" t ON t."Id" = m."Id"
        JOIN client_accounts ca ON t."TargetAccountId" = ca.account_id
        CROSS JOIN params p
        WHERE m."StateId" = 250
          AND t."SourceAccountType" = 1
          AND t."TargetAccountType" = 2
          AND m."StatedOn" >= p.fromDT AND m."StatedOn" < p.toDT
        GROUP BY ca.sales_code, ca."CurrencyId"
    ),
    transfer_out_acc AS (
        SELECT ca.sales_code, ca."CurrencyId", SUM(t."Amount") AS total_amount
        FROM core."_Matter" m
        JOIN acct."_Transaction" t ON t."Id" = m."Id"
        JOIN client_accounts ca ON t."SourceAccountId" = ca.account_id
        CROSS JOIN params p
        WHERE m."StateId" = 250
          AND t."SourceAccountType" = 2
          AND t."TargetAccountType" = 1
          AND m."StatedOn" >= p.fromDT AND m."StatedOn" < p.toDT
        GROUP BY ca.sales_code, ca."CurrencyId"
    ),
    transfer_out_wallet AS (
        WITH transfer_out_with_sales AS (
            SELECT DISTINCT ON (t."Id")
                t."Id" AS transaction_id,
                w."CurrencyId",
                t."CurrencyId" AS target_currency_id,
                t."Amount",
                COALESCE(sa."Code", 'NO_SALES') AS sales_code
            FROM core."_Matter" m
            JOIN acct."_Transaction" t ON t."Id" = m."Id"
            JOIN acct."_Wallet" w ON t."SourceAccountId" = w."Id"
            LEFT JOIN trd."_Account" a ON w."PartyId" = a."PartyId"
                AND a."Role" IN (300, 400)
                AND a."CurrencyId" = w."CurrencyId"
            LEFT JOIN trd."_Account" sa ON a."SalesAccountId" = sa."Id"
            CROSS JOIN params p
            WHERE m."StateId" = 250 AND t."SourceAccountType" = 1
              AND m."StatedOn" >= p.fromDT AND m."StatedOn" < p.toDT
            ORDER BY t."Id", a."Id"
        )
        SELECT
            sales_code,
            "CurrencyId",
            SUM(CASE
                WHEN "CurrencyId" = 840 AND target_currency_id = 841 THEN "Amount" / 100
                WHEN "CurrencyId" = 841 AND target_currency_id = 840 THEN "Amount" * 100
                ELSE "Amount"
            END) AS total_amount
        FROM transfer_out_with_sales
        GROUP BY sales_code, "CurrencyId"
    ),
    transfer_in_wallet AS (
        WITH transfer_in_with_sales AS (
            SELECT DISTINCT ON (t."Id")
                t."Id" AS transaction_id,
                w."CurrencyId",
                t."CurrencyId" AS source_currency_id,
                t."Amount",
                COALESCE(sa."Code", 'NO_SALES') AS sales_code
            FROM core."_Matter" m
            JOIN acct."_Transaction" t ON t."Id" = m."Id"
            JOIN acct."_Wallet" w ON t."TargetAccountId" = w."Id"
            LEFT JOIN trd."_Account" a ON w."PartyId" = a."PartyId"
                AND a."Role" IN (300, 400)
                AND a."CurrencyId" = w."CurrencyId"
            LEFT JOIN trd."_Account" sa ON a."SalesAccountId" = sa."Id"
            CROSS JOIN params p
            WHERE m."StateId" = 250 AND t."TargetAccountType" = 1
              AND m."StatedOn" >= p.fromDT AND m."StatedOn" < p.toDT
            ORDER BY t."Id", a."Id"
        )
        SELECT
            sales_code,
            "CurrencyId",
            SUM(CASE
                WHEN "CurrencyId" = 840 AND source_currency_id = 841 THEN "Amount" / 100
                WHEN "CurrencyId" = 841 AND source_currency_id = 840 THEN "Amount" * 100
                ELSE "Amount"
            END) AS total_amount
        FROM transfer_in_with_sales
        GROUP BY sales_code, "CurrencyId"
    ),
    credit_adjust AS (
        SELECT ca.sales_code, ca."CurrencyId", SUM(ar."Amount") AS total_amount
        FROM trd."_AdjustRecord" ar
        JOIN client_accounts ca ON ar."AccountId" = ca.account_id
        CROSS JOIN params p
        WHERE ar."Type" = 2 AND ar."Status" = 2
          AND ar."UpdatedOn" >= p.fromDT AND ar."UpdatedOn" < p.toDT
        GROUP BY ca.sales_code, ca."CurrencyId"
    ),
    balance_adjust AS (
        SELECT ca.sales_code, ca."CurrencyId", SUM(ar."Amount") AS total_amount
        FROM trd."_AdjustRecord" ar
        JOIN client_accounts ca ON ar."AccountId" = ca.account_id
        CROSS JOIN params p
        WHERE ar."Type" IN (1, 3) AND ar."Status" = 2
          AND ar."UpdatedOn" >= p.fromDT AND ar."UpdatedOn" < p.toDT
        GROUP BY ca.sales_code, ca."CurrencyId"
    ),
    wallet_adjust AS (
        SELECT
            CASE
                WHEN a_sales."Code" IS NOT NULL THEN a_sales."Code"
                WHEN sa."Code" IS NOT NULL THEN sa."Code"
                ELSE 'NO_SALES'
            END AS sales_code,
            SUM(wa."Amount") AS total_amount
        FROM acct."_WalletAdjust" wa
        JOIN core."_Matter" m ON wa."Id" = m."Id"
        JOIN acct."_Wallet" w ON wa."WalletId" = w."Id"
        LEFT JOIN trd."_Account" a_sales ON w."PartyId" = a_sales."PartyId" AND a_sales."Role" = 100
        LEFT JOIN trd."_Account" a_client ON w."PartyId" = a_client."PartyId" AND a_client."Role" IN (300, 400)
        LEFT JOIN trd."_Account" sa ON a_client."SalesAccountId" = sa."Id"
        CROSS JOIN params p
        WHERE m."StateId" = 750
          AND w."CurrencyId" = 840
          AND m."StatedOn" >= p.fromDT AND m."StatedOn" < p.toDT
        GROUP BY
            CASE
                WHEN a_sales."Code" IS NOT NULL THEN a_sales."Code"
                WHEN sa."Code" IS NOT NULL THEN sa."Code"
                ELSE 'NO_SALES'
            END
    ),
    rebate_usd AS (
        SELECT
            COALESCE(sa."Code", 'NO_SALES') AS sales_code,
            SUM(CASE
                WHEN w."CurrencyId" = 840 THEN wt."Amount"
                WHEN w."CurrencyId" = 841 THEN wt."Amount" / 100
                ELSE wt."Amount"
            END) AS total_amount
        FROM acct."_WalletTransaction" wt
        JOIN core."_Matter" m ON wt."MatterId" = m."Id"
        JOIN trd."_Rebate" r ON r."Id" = m."Id"
        JOIN trd."_TradeRebate" tr ON r."TradeRebateId" = tr."Id"
        JOIN acct."_Wallet" w ON wt."WalletId" = w."Id"
        JOIN trd."_Account" aa ON r."AccountId" = aa."Id"
        LEFT JOIN trd."_Account" sa ON aa."SalesAccountId" = sa."Id"
        CROSS JOIN params p
        WHERE m."Type" = 500 AND m."StateId" = 550
          AND tr."CurrencyId" = 840
          AND m."StatedOn" >= p.fromDT AND m."StatedOn" < p.toDT
          {closing_filter}
        GROUP BY sa."Code"
    ),
    rebate_usc AS (
        SELECT
            COALESCE(sa."Code", 'NO_SALES') AS sales_code,
            SUM(CASE
                WHEN w."CurrencyId" = 841 THEN wt."Amount"
                WHEN w."CurrencyId" = 840 THEN wt."Amount" * 100
                ELSE wt."Amount"
            END) AS total_amount
        FROM acct."_WalletTransaction" wt
        JOIN core."_Matter" m ON wt."MatterId" = m."Id"
        JOIN trd."_Rebate" r ON r."Id" = m."Id"
        JOIN trd."_TradeRebate" tr ON r."TradeRebateId" = tr."Id"
        JOIN acct."_Wallet" w ON wt."WalletId" = w."Id"
        JOIN trd."_Account" aa ON r."AccountId" = aa."Id"
        LEFT JOIN trd."_Account" sa ON aa."SalesAccountId" = sa."Id"
        CROSS JOIN params p
        WHERE m."Type" = 500 AND m."StateId" = 550
          AND tr."CurrencyId" = 841
          AND m."StatedOn" >= p.fromDT AND m."StatedOn" < p.toDT
          {closing_filter}
        GROUP BY sa."Code"
    ),
    rebate_wallet AS (
        SELECT
            COALESCE(sa."Code", 'NO_SALES') AS sales_code,
            SUM(CASE
                WHEN w."CurrencyId" = 840 THEN wt."Amount"
                WHEN w."CurrencyId" = 841 THEN wt."Amount" / 100
                ELSE wt."Amount"
            END) AS total_amount
        FROM acct."_WalletTransaction" wt
        JOIN core."_Matter" m ON wt."MatterId" = m."Id"
        JOIN acct."_Wallet" w ON wt."WalletId" = w."Id"
        JOIN trd."_Rebate" r ON r."Id" = m."Id"
        JOIN trd."_Account" aa ON r."AccountId" = aa."Id"
        LEFT JOIN trd."_Account" sa ON aa."SalesAccountId" = sa."Id"
        CROSS JOIN params p
        WHERE m."Type" = 500
          AND m."StateId" = 550
          AND m."StatedOn" >= p.fromDT AND m."StatedOn" < p.toDT
        GROUP BY sa."Code"
    ),
    all_sales_codes AS (
        SELECT DISTINCT sales_code FROM client_accounts
        UNION
        SELECT DISTINCT sales_code FROM agent_accounts
    ),
    all_combinations AS (
        SELECT sales_code, 840 AS currency_id, 'USD' AS currency_name FROM all_sales_codes
        UNION ALL
        SELECT sales_code, 841 AS currency_id, 'USC' AS currency_name FROM all_sales_codes
        UNION ALL
        SELECT sales_code, 840 AS currency_id, 'Wallet' AS currency_name FROM all_sales_codes
        UNION ALL
        SELECT sales_code, 0 AS currency_id, 'User' AS currency_name FROM all_sales_codes
    )
SELECT
    ac.sales_code AS "Office",
    ac.currency_name AS "Currency",
    CASE
        WHEN ac.currency_name IN ('USD', 'USC') THEN cal.account_list
        WHEN ac.currency_name = 'Wallet' THEN awl.wallet_list
        ELSE NULL
    END AS "AccountList",
    CASE
        WHEN ac.currency_name IN ('USD', 'USC') THEN COALESCE(cal.service_id, 0)
        ELSE 0
    END AS "ServiceId",
    CASE
        WHEN ac.currency_name = 'User' THEN COALESCE(nu.new_user_count, 0)
        WHEN ac.currency_name = 'Wallet' THEN 0
        ELSE COALESCE(na.new_acc_count, 0)
    END AS "NewAccUser",
    (CASE WHEN ac.currency_name = 'Wallet' THEN COALESCE(wpe.total_balance, 0) ELSE 0 END)::float8 AS "PrevEquity",
    (CASE WHEN ac.currency_name = 'Wallet' THEN COALESCE(wce.total_balance, 0) ELSE 0 END)::float8 AS "CurrEquity",
    (CASE WHEN ac.currency_name IN ('Wallet', 'User') THEN 0 ELSE COALESCE(mi.total_amount, 0) / 1000000.0 END)::float8 AS "MarginIn",
    (CASE
        WHEN ac.currency_name = 'User' THEN 0
        WHEN ac.currency_name = 'Wallet' THEN COALESCE(wmo.total_amount, 0) / 1000000.0
        ELSE COALESCE(mo.total_amount, 0) / 1000000.0
    END)::float8 AS "MarginOut",
    (CASE
        WHEN ac.currency_name = 'User' THEN 0
        WHEN ac.currency_name = 'Wallet' THEN COALESCE(tiw.total_amount, 0) / 1000000.0
        ELSE COALESCE(ti.total_amount, 0) / 1000000.0
    END)::float8 AS "TransferIn",
    (CASE
        WHEN ac.currency_name = 'User' THEN 0
        WHEN ac.currency_name = 'Wallet' THEN COALESCE(tow.total_amount, 0) / 1000000.0
        ELSE COALESCE(tro.total_amount, 0) / 1000000.0
    END)::float8 AS "TransferOut",
    (CASE WHEN ac.currency_name IN ('Wallet', 'User') THEN 0 ELSE COALESCE(cr.total_amount, 0) / 1000000.0 END)::float8 AS "Credit",
    (CASE
        WHEN ac.currency_name = 'Wallet' THEN COALESCE(wadj.total_amount, 0) / 1000000.0
        WHEN ac.currency_name = 'User' THEN 0
        ELSE COALESCE(adj.total_amount, 0) / 1000000.0
    END)::float8 AS "Adjust",
    (CASE
        WHEN ac.currency_name = 'USD' THEN COALESCE(rbu.total_amount, 0) / 1000000.0
        WHEN ac.currency_name = 'USC' THEN COALESCE(rbs.total_amount, 0) / 1000000.0
        WHEN ac.currency_name = 'Wallet' THEN COALESCE(rbw.total_amount, 0) / 1000000.0
        ELSE 0
    END)::float8 AS "Rebate"
FROM all_combinations ac
LEFT JOIN client_account_list cal ON ac.sales_code = cal.sales_code AND ac.currency_id = cal."CurrencyId" AND ac.currency_name NOT IN ('Wallet', 'User')
LEFT JOIN agent_wallet_list awl ON ac.sales_code = awl.sales_code AND ac.currency_name = 'Wallet'
LEFT JOIN new_accounts na ON ac.sales_code = na.sales_code AND ac.currency_id = na."CurrencyId" AND ac.currency_name NOT IN ('Wallet', 'User')
LEFT JOIN new_users nu ON ac.sales_code = nu.sales_code AND ac.currency_name = 'User'
LEFT JOIN wallet_prev_equity wpe ON ac.sales_code = wpe.sales_code AND ac.currency_name = 'Wallet'
LEFT JOIN wallet_curr_equity wce ON ac.sales_code = wce.sales_code AND ac.currency_name = 'Wallet'
LEFT JOIN margin_in mi ON ac.sales_code = mi.sales_code AND ac.currency_id = mi."CurrencyId" AND ac.currency_name NOT IN ('Wallet', 'User')
LEFT JOIN margin_out mo ON ac.sales_code = mo.sales_code AND ac.currency_id = mo."CurrencyId" AND ac.currency_name NOT IN ('Wallet', 'User')
LEFT JOIN transfer_in_acc ti ON ac.sales_code = ti.sales_code AND ac.currency_id = ti."CurrencyId" AND ac.currency_name NOT IN ('Wallet', 'User')
LEFT JOIN transfer_out_acc tro ON ac.sales_code = tro.sales_code AND ac.currency_id = tro."CurrencyId" AND ac.currency_name NOT IN ('Wallet', 'User')
LEFT JOIN credit_adjust cr ON ac.sales_code = cr.sales_code AND ac.currency_id = cr."CurrencyId" AND ac.currency_name NOT IN ('Wallet', 'User')
LEFT JOIN balance_adjust adj ON ac.sales_code = adj.sales_code AND ac.currency_id = adj."CurrencyId" AND ac.currency_name NOT IN ('Wallet', 'User')
LEFT JOIN wallet_adjust wadj ON ac.sales_code = wadj.sales_code AND ac.currency_name = 'Wallet'
LEFT JOIN rebate_usd rbu ON ac.sales_code = rbu.sales_code AND ac.currency_name = 'USD'
LEFT JOIN rebate_usc rbs ON ac.sales_code = rbs.sales_code AND ac.currency_name = 'USC'
LEFT JOIN rebate_wallet rbw ON ac.sales_code = rbw.sales_code AND ac.currency_name = 'Wallet'
LEFT JOIN wallet_margin_out wmo ON ac.sales_code = wmo.sales_code AND wmo."CurrencyId" = 840 AND ac.currency_name = 'Wallet'
LEFT JOIN transfer_in_wallet tiw ON ac.sales_code = tiw.sales_code AND tiw."CurrencyId" = 840 AND ac.currency_name = 'Wallet'
LEFT JOIN transfer_out_wallet tow ON ac.sales_code = tow.sales_code AND tow."CurrencyId" = 840 AND ac.currency_name = 'Wallet'
WHERE ac.sales_code != 'SYDS1'
ORDER BY ac.sales_code,
         CASE ac.currency_name WHEN 'User' THEN 0 WHEN 'USD' THEN 1 WHEN 'USC' THEN 2 WHEN 'Wallet' THEN 3 END
        "#,
        closing_filter = closing_filter,
    );

    let rows = sqlx::query_as::<_, PgRow>(&sql)
        .bind(from)
        .bind(to)
        .bind(prev_dt)
        .bind(curr_dt)
        .bind(closed_on_from)
        .bind(closed_on_to)
        .fetch_all(pool)
        .await?;

    Ok(rows)
}

// ─── MT5 queries ──────────────────────────────────────────────────────────────

async fn query_mt5(
    _ctx: &AppContext,
    tenant_pool: &PgPool,
    pg_rows: &[PgRow],
    to: DateTime<Utc>,
) -> Vec<Mt5Row> {
    let mut results = Vec::new();

    let mut service_groups: HashMap<i64, Vec<&PgRow>> = HashMap::new();
    for row in pg_rows {
        if row.account_list.as_deref().map(|s| !s.is_empty()).unwrap_or(false)
            && (row.currency == "USD" || row.currency == "USC")
            && row.service_id > 0
        {
            service_groups.entry(row.service_id as i64).or_default().push(row);
        }
    }

    if service_groups.is_empty() {
        info!("[DailyEquity MT5] No USD/USC accounts — skipping MT5 query");
        return results;
    }

    let report_date = to.date_naive();
    let day_start = report_date.and_hms_opt(0, 0, 0).unwrap().and_utc();
    let start_ts = day_start.timestamp();
    let end_ts = start_ts + 86399;
    let day_end = report_date.and_hms_opt(23, 59, 59).unwrap().and_utc();

    for (service_id, rows) in &service_groups {
        let mt5_conn =
            match crate::db::tenant::get_mt5_connection_string_from_central(tenant_pool, *service_id as i32).await {
                Ok(Some(c)) => c,
                _ => {
                    warn!("[DailyEquity MT5] No connection string for ServiceId={}", service_id);
                    continue;
                }
            };

        let mt5_pool = match crate::db::mysql_pool(&mt5_conn).await {
            Ok(p) => p,
            Err(e) => {
                warn!("[DailyEquity MT5] Connect failed ServiceId={}: {}", service_id, e);
                continue;
            }
        };

        for pg_row in rows {
            let account_str = match &pg_row.account_list {
                Some(s) if !s.is_empty() => s,
                _ => continue,
            };

            let logins: Vec<u64> = account_str
                .split(',')
                .filter_map(|s| s.trim().parse::<u64>().ok())
                .filter(|&v| v > 0)
                .collect();

            if logins.is_empty() {
                continue;
            }

            let (prev_eq, curr_eq, pl) =
                query_mt5_daily(&mt5_pool, &logins, start_ts, end_ts).await;
            let lots = query_mt5_lots(&mt5_pool, &logins, day_start, day_end).await;

            info!(
                "[DailyEquity MT5] Office={} Currency={} PrevEq={} CurrEq={} PL={} Lots={}",
                pg_row.office, pg_row.currency, prev_eq, curr_eq, pl, lots
            );

            results.push(Mt5Row {
                office: pg_row.office.clone(),
                currency: pg_row.currency.clone(),
                previous_equity: prev_eq,
                current_equity: curr_eq,
                lots,
                pl,
            });
        }
    }

    results
}

async fn query_mt5_daily(
    pool: &sqlx::MySqlPool,
    logins: &[u64],
    start_ts: i64,
    end_ts: i64,
) -> (f64, f64, f64) {
    if logins.is_empty() {
        return (0.0, 0.0, 0.0);
    }
    let ph = logins.iter().map(|_| "?").collect::<Vec<_>>().join(", ");
    let sql = format!(
        "SELECT COALESCE(SUM(EquityPrevDay),0) AS prev_eq, \
                COALESCE(SUM(ProfitEquity),0) AS curr_eq, \
                COALESCE(SUM(DailyProfit),0)  AS daily_profit \
         FROM mt5_daily \
         WHERE Datetime >= ? AND Datetime <= ? AND Login IN ({})",
        ph
    );

    #[derive(sqlx::FromRow)]
    struct Row {
        prev_eq: f64,
        curr_eq: f64,
        daily_profit: f64,
    }

    let mut q = sqlx::query_as::<_, Row>(&sql)
        .bind(start_ts)
        .bind(end_ts);
    for &l in logins {
        q = q.bind(l as i64);
    }
    match q.fetch_optional(pool).await {
        Ok(Some(r)) => (r.prev_eq, r.curr_eq, r.daily_profit),
        _ => (0.0, 0.0, 0.0),
    }
}

async fn query_mt5_lots(
    pool: &sqlx::MySqlPool,
    logins: &[u64],
    from: DateTime<Utc>,
    to: DateTime<Utc>,
) -> f64 {
    if logins.is_empty() {
        return 0.0;
    }
    let ph = logins.iter().map(|_| "?").collect::<Vec<_>>().join(", ");
    let sql = format!(
        "SELECT COALESCE(SUM(Volume / 10000.0), 0) AS lots \
         FROM mt5_deals \
         WHERE Time >= ? AND Time <= ? \
           AND Login IN ({}) \
           AND Entry IN (1, 2, 3) \
           AND Action IN (0, 1)",
        ph
    );

    #[derive(sqlx::FromRow)]
    struct Row {
        lots: f64,
    }

    let mut q = sqlx::query_as::<_, Row>(&sql)
        .bind(from)
        .bind(to);
    for &l in logins {
        q = q.bind(l as i64);
    }
    match q.fetch_optional(pool).await {
        Ok(Some(r)) => r.lots,
        _ => 0.0,
    }
}

// ─── Merge PG + MT5 ──────────────────────────────────────────────────────────

fn merge(pg_rows: Vec<PgRow>, mt5_rows: Vec<Mt5Row>) -> Vec<OutputRecord> {
    let mt5_map: HashMap<(String, String), (f64, f64, f64, f64)> = mt5_rows
        .into_iter()
        .map(|r| {
            (
                (r.office, r.currency),
                (r.previous_equity, r.current_equity, r.lots, r.pl),
            )
        })
        .collect();

    pg_rows
        .into_iter()
        .map(|pg| {
            let mut rec = OutputRecord {
                currency: pg.currency.clone(),
                office: pg.office.clone(),
                new_user: if pg.currency == "User" { pg.new_acc_user } else { 0 },
                new_acc: if pg.currency != "User" { pg.new_acc_user } else { 0 },
                margin_in: pg.margin_in,
                margin_out: -pg.margin_out.abs(),
                credit: pg.credit,
                adjust: pg.adjust,
                rebate: pg.rebate,
                ..Default::default()
            };
            rec.transfer = pg.transfer_in - pg.transfer_out;

            match pg.currency.as_str() {
                "USD" | "USC" => {
                    if let Some(&(prev_eq, curr_eq, lots, pl)) =
                        mt5_map.get(&(pg.office.clone(), pg.currency.clone()))
                    {
                        rec.previous_equity = prev_eq;
                        rec.current_equity = curr_eq;
                        rec.lots = lots;
                        rec.pl = pl;
                    }
                    rec.net_in_out = rec.margin_in + rec.margin_out + rec.transfer;
                    rec.estimates_net_pl = rec.pl + rec.rebate;
                }
                "Wallet" => {
                    rec.previous_equity = pg.prev_equity;
                    rec.current_equity = pg.curr_equity;
                    rec.net_in_out = rec.transfer + rec.adjust + rec.rebate;
                }
                _ => {} // "User" row — all zeros
            }

            rec
        })
        .collect()
}

// ─── Group by currency, inject NewUser, add Total + blank rows ────────────────

fn build_output(records: Vec<OutputRecord>) -> Vec<OutputRecord> {
    let user_map: HashMap<String, i64> = records
        .iter()
        .filter(|r| r.currency == "User")
        .map(|r| (r.office.clone(), r.new_user))
        .collect();

    let currency_order = ["USD", "Wallet", "USC"];
    let mut out: Vec<OutputRecord> = Vec::new();

    for (idx, &currency) in currency_order.iter().enumerate() {
        let mut group: Vec<OutputRecord> = records
            .iter()
            .filter(|r| r.currency == currency)
            .map(|r| {
                let mut rec = r.clone();
                rec.new_user = *user_map.get(&r.office).unwrap_or(&0);
                rec
            })
            .collect();

        if group.is_empty() {
            continue;
        }

        let is_usc = currency == "USC";
        let r2 = |v: f64| if is_usc { v } else { (v * 100.0).round() / 100.0 };

        let subtotal = OutputRecord {
            currency: currency.to_string(),
            office: "Total".to_string(),
            new_user: group.iter().map(|r| r.new_user).sum(),
            new_acc: group.iter().map(|r| r.new_acc).sum(),
            previous_equity: group.iter().map(|r| r2(r.previous_equity)).sum(),
            current_equity: group.iter().map(|r| r2(r.current_equity)).sum(),
            margin_in: group.iter().map(|r| r2(r.margin_in)).sum(),
            margin_out: group.iter().map(|r| r2(r.margin_out)).sum(),
            transfer: group.iter().map(|r| r2(r.transfer)).sum(),
            credit: group.iter().map(|r| r2(r.credit)).sum(),
            adjust: group.iter().map(|r| r2(r.adjust)).sum(),
            rebate: group.iter().map(|r| r2(r.rebate)).sum(),
            net_in_out: group.iter().map(|r| r2(r.net_in_out)).sum(),
            lots: group.iter().map(|r| r2(r.lots)).sum(),
            pl: group.iter().map(|r| r2(r.pl)).sum(),
            estimates_net_pl: group.iter().map(|r| r2(r.estimates_net_pl)).sum(),
        };

        out.append(&mut group);
        out.push(subtotal);

        if idx < currency_order.len() - 1 {
            out.push(OutputRecord::default()); // blank separator
        }
    }

    out
}

// ─── CSV rendering ────────────────────────────────────────────────────────────

fn render_csv(records: &[OutputRecord], to: DateTime<Utc>) -> Vec<u8> {
    let report_date = to.format("%Y-%m-%d");
    let mut lines: Vec<String> = Vec::new();

    lines.push(format!("Daily Equity Report {},,,,,,,,,,,,,,", report_date));
    lines.push(String::new());
    lines.push(
        "Currency,Office,New User,New Acc,Previous Equity,Current Equity,\
Margin In,Margin Out,Transfer,Credit,Adjust,Rebate,Net In/Out,Lots,P&L,Estimates Net PL"
            .to_string(),
    );

    let mut last_currency: Option<String> = None;

    for rec in records {
        // blank separator
        if rec.currency.is_empty() && rec.office.is_empty() {
            lines.push(",,,,,,,,,,,,,,".to_string());
            last_currency = None;
            continue;
        }

        let is_usc = rec.currency == "USC";
        let fmt = |v: f64| -> String {
            if v != 0.0 {
                if is_usc { format!("{:.4}", v) } else { format!("{:.2}", v) }
            } else {
                "0".to_string()
            }
        };

        if rec.office == "Total" {
            last_currency = None;
            lines.push(format!(
                "Total,,{},{},{},{},{},{},{},{},{},{},{},{},{},{}",
                rec.new_user, rec.new_acc,
                fmt(rec.previous_equity), fmt(rec.current_equity),
                fmt(rec.margin_in), fmt(rec.margin_out), fmt(rec.transfer),
                fmt(rec.credit), fmt(rec.adjust), fmt(rec.rebate),
                fmt(rec.net_in_out), fmt(rec.lots), fmt(rec.pl), fmt(rec.estimates_net_pl),
            ));
            continue;
        }

        let currency_col = if last_currency.as_deref() != Some(&rec.currency) {
            last_currency = Some(rec.currency.clone());
            rec.currency.clone()
        } else {
            String::new()
        };

        lines.push(format!(
            "{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{}",
            currency_col, rec.office,
            rec.new_user, rec.new_acc,
            fmt(rec.previous_equity), fmt(rec.current_equity),
            fmt(rec.margin_in), fmt(rec.margin_out), fmt(rec.transfer),
            fmt(rec.credit), fmt(rec.adjust), fmt(rec.rebate),
            fmt(rec.net_in_out), fmt(rec.lots), fmt(rec.pl), fmt(rec.estimates_net_pl),
        ));
    }

    lines.join("\n").into_bytes()
}
