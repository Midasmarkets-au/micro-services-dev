use anyhow::Result;
use chrono::{DateTime, Utc};
use serde::{Deserialize, Serialize};
use sqlx::PgPool;

use super::csv::to_csv_bytes;
use super::request::DateRangeCriteria;
use crate::report::ReportRequestType;

/// MT5 server is GMT+2
const HOURS_GAP: i64 = 2;

// ──────────────────────────────────────────────────────────────────────────────
// Deposit report
// Mirrors DepositRecord.cs
// Header: account_number,account_uid,client_name,currency,deposit_status,
//         payment_status,payment_id,payment_number,payment_method,amount,created_on
// ──────────────────────────────────────────────────────────────────────────────

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct DepositRecord {
    pub account_number: String,       // "wallet" if no trade account
    pub account_uid: i64,
    pub client_name: Option<String>,
    pub currency: Option<String>,
    pub deposit_status: String,
    pub payment_status: String,
    pub payment_id: Option<i64>,
    pub payment_number: Option<String>,
    pub payment_method: Option<String>,
    pub amount: f64,                  // d."Amount" / 1_000_000
    pub created_on: Option<String>,   // PostedOn + 2h formatted
}

pub async fn generate_deposit_csv(pool: &PgPool, criteria: &DateRangeCriteria) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(30));
    let to = criteria.to.unwrap_or_else(Utc::now);

    let records = sqlx::query_as::<_, DepositRecord>(
        r#"SELECT
            CASE WHEN a."AccountNumber" IS NOT NULL AND a."AccountNumber" != 0
                 THEN a."AccountNumber"::text ELSE 'wallet' END        as account_number,
            COALESCE(a."Uid", 0)                                       as account_uid,
            p."NativeName"                                             as client_name,
            c."Code"                                                   as currency,
            CASE m."StateId"
                WHEN 300 THEN 'DepositCreated'
                WHEN 305 THEN 'DepositCanceled'
                WHEN 306 THEN 'DepositFailed'
                WHEN 310 THEN 'DepositPaymentCompleted'
                WHEN 315 THEN 'DepositCallbackTimeOut'
                WHEN 320 THEN 'DepositCentralApproved'
                WHEN 325 THEN 'DepositCentralRejected'
                WHEN 330 THEN 'DepositTenantApproved'
                WHEN 335 THEN 'DepositTenantRejected'
                WHEN 345 THEN 'DepositCallbackCompleted'
                WHEN 350 THEN 'DepositCompleted'
                ELSE m."StateId"::text
            END                                                        as deposit_status,
            CASE dp."Status"
                WHEN 0 THEN 'Pending'
                WHEN 1 THEN 'Executing'
                WHEN 2 THEN 'Completed'
                WHEN 3 THEN 'Failed'
                WHEN 4 THEN 'Cancelled'
                WHEN 5 THEN 'Rejected'
                ELSE COALESCE(dp."Status"::text, '')
            END                                                        as payment_status,
            d."PaymentId"                                              as payment_id,
            dp."Number"                                                as payment_number,
            pm."Name" || ' ' || COALESCE(dp."ReferenceNumber", '')    as payment_method,
            d."Amount"::float8 / 1000000.0                            as amount,
            TO_CHAR(m."PostedOn" + INTERVAL '2 hours', 'YYYY-MM-DD HH24:MI:SS') as created_on
           FROM acct."_Deposit" d
           LEFT JOIN trd."_Account" a    ON a."Id"  = d."TargetAccountId"
           LEFT JOIN core."_Matter" m    ON m."Id"  = d."Id"
           LEFT JOIN acct."_Payment" dp  ON dp."Id" = d."PaymentId"
           LEFT JOIN acct."_PaymentMethod" pm ON pm."Id" = dp."PaymentServiceId"
           LEFT JOIN acct."_Currency" c  ON c."Id"  = d."CurrencyId"
           LEFT JOIN core."_Party" p     ON p."Id"  = d."PartyId"
           WHERE m."StatedOn" >= $1 AND m."StatedOn" < $2
           ORDER BY m."PostedOn" DESC"#,
    )
    .bind(from)
    .bind(to)
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}

// ──────────────────────────────────────────────────────────────────────────────
// Withdrawal report
// Mirrors WithdrawalRecord.cs / WithdrawalUnionPayRecord.cs / WithdrawalUSDTRecord.cs
// ──────────────────────────────────────────────────────────────────────────────

/// Standard withdrawal (WithdrawForTenant / WithdrawPendingForTenant)
/// Header: account_number,account_uid,client_name,currency,withdrawal_status,
///         payment_status,payment_id,payment_number,payment_method,amount,exchange_rate,
///         created_on,approved_on
#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct WithdrawalRecord {
    pub account_number: String,
    pub account_uid: i64,
    pub client_name: Option<String>,
    pub currency: Option<String>,
    pub withdrawal_status: String,
    pub payment_status: String,
    pub payment_id: Option<i64>,
    pub payment_number: Option<String>,
    pub payment_method: Option<String>,
    pub amount: f64,         // w."Amount" / 1_000_000
    pub exchange_rate: f64,  // amount / payment_amount
    pub created_on: Option<String>,
    pub approved_on: Option<String>,
}

/// UnionPay / USDT pending withdrawal (no exchange_rate, dates not offset)
/// Header: account_number,account_uid,client_name,currency,withdrawal_status,
///         payment_status,payment_id,payment_number,payment_method,amount,created_on,approved_on
#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct WithdrawalSimpleRecord {
    pub account_number: String,
    pub account_uid: i64,
    pub client_name: Option<String>,
    pub currency: Option<String>,
    pub withdrawal_status: String,
    pub payment_status: String,
    pub payment_id: Option<i64>,
    pub payment_number: Option<String>,
    pub payment_method: Option<String>,
    pub amount: f64,
    pub created_on: Option<String>,
    pub approved_on: Option<String>,
}

pub async fn generate_withdrawal_csv(
    pool: &PgPool,
    criteria: &DateRangeCriteria,
    report_type: ReportRequestType,
) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(30));
    let to = criteria.to.unwrap_or_else(Utc::now);

    let state_case = r#"CASE m."StateId"
        WHEN 400 THEN 'WithdrawalCreated'
        WHEN 405 THEN 'WithdrawalCanceled'
        WHEN 406 THEN 'WithdrawalFailed'
        WHEN 410 THEN 'WithdrawalCentralApproved'
        WHEN 415 THEN 'WithdrawalCentralRejected'
        WHEN 420 THEN 'WithdrawalTenantApproved'
        WHEN 425 THEN 'WithdrawalTenantRejected'
        WHEN 430 THEN 'WithdrawalPaymentCompleted'
        WHEN 450 THEN 'WithdrawalCompleted'
        ELSE m.state_id::text
    END"#;

    let payment_status_case = r#"CASE wp."Status"
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Executing'
        WHEN 2 THEN 'Completed'
        WHEN 3 THEN 'Failed'
        WHEN 4 THEN 'Cancelled'
        WHEN 5 THEN 'Rejected'
        ELSE COALESCE(wp."Status"::text, '')
    END"#;

    match report_type {
        ReportRequestType::WithdrawUnionPayPendingForTenant
        | ReportRequestType::WithdrawUSDTPendingForTenant => {
            // No exchange_rate; no +2h offset (matches C# UnionPay/USDT records)
            let sql = format!(
                r#"SELECT
                    CASE WHEN a."AccountNumber" IS NOT NULL AND a."AccountNumber" != 0
                         THEN a."AccountNumber"::text ELSE 'wallet' END       as account_number,
                    COALESCE(a."Uid", 0)                                      as account_uid,
                    p."NativeName"                                            as client_name,
                    c."Code"                                                  as currency,
                    {state_case}                                              as withdrawal_status,
                    {payment_status_case}                                     as payment_status,
                    wp."Id"                                                   as payment_id,
                    wp."Number"                                               as payment_number,
                    pm."Name"                                                 as payment_method,
                    w."Amount"::float8 / 1000000.0                           as amount,
                    TO_CHAR(m."PostedOn", 'YYYY-MM-DD HH24:MI:SS')           as created_on,
                    TO_CHAR(w."ApprovedOn", 'YYYY-MM-DD HH24:MI:SS')         as approved_on
                   FROM acct."_Withdrawal" w
                   LEFT JOIN trd."_Account" a      ON a."Id"  = w."SourceAccountId"
                   LEFT JOIN core."_Matter" m      ON m."Id"  = w."Id"
                   LEFT JOIN acct."_Payment" wp    ON wp."Id" = w."PaymentId"
                   LEFT JOIN acct."_PaymentMethod" pm ON pm."Id" = wp."PaymentServiceId"
                   LEFT JOIN acct."_Currency" c    ON c."Id"  = w."CurrencyId"
                   LEFT JOIN core."_Party" p       ON p."Id"  = w."PartyId"
                   WHERE m."StateId" = 400
                   ORDER BY w."ApprovedOn""#,
                state_case = state_case,
                payment_status_case = payment_status_case,
            );
            let records = sqlx::query_as::<_, WithdrawalSimpleRecord>(&sql)
                .fetch_all(pool)
                .await?;
            to_csv_bytes(&records)
        }
        ReportRequestType::WithdrawPendingForTenant => {
            // Pending: filter by StateId=400, no date range; includes exchange_rate + 2h offset
            let sql = format!(
                r#"SELECT
                    CASE WHEN a."AccountNumber" IS NOT NULL AND a."AccountNumber" != 0
                         THEN a."AccountNumber"::text ELSE 'wallet' END       as account_number,
                    COALESCE(a."Uid", 0)                                      as account_uid,
                    p."NativeName"                                            as client_name,
                    c."Code"                                                  as currency,
                    {state_case}                                              as withdrawal_status,
                    {payment_status_case}                                     as payment_status,
                    wp."Id"                                                   as payment_id,
                    wp."Number"                                               as payment_number,
                    pm."Name" || ' ' || COALESCE(wp."ReferenceNumber", '')    as payment_method,
                    w."Amount"::float8 / 1000000.0                           as amount,
                    CASE WHEN COALESCE(wp."Amount", 0) = 0 THEN 0.0
                         ELSE ROUND(w."Amount"::numeric / NULLIF(wp."Amount"::numeric, 0), 4)::float8
                    END                                                       as exchange_rate,
                    TO_CHAR(m."PostedOn" + INTERVAL '2 hours', 'YYYY-MM-DD HH24:MI:SS')    as created_on,
                    TO_CHAR(w."ApprovedOn" + INTERVAL '2 hours', 'YYYY-MM-DD HH24:MI:SS')  as approved_on
                   FROM acct."_Withdrawal" w
                   LEFT JOIN trd."_Account" a      ON a."Id"  = w."SourceAccountId"
                   LEFT JOIN core."_Matter" m      ON m."Id"  = w."Id"
                   LEFT JOIN acct."_Payment" wp    ON wp."Id" = w."PaymentId"
                   LEFT JOIN acct."_PaymentMethod" pm ON pm."Id" = wp."PaymentServiceId"
                   LEFT JOIN acct."_Currency" c    ON c."Id"  = w."CurrencyId"
                   LEFT JOIN core."_Party" p       ON p."Id"  = w."PartyId"
                   WHERE m."StateId" = 400
                   ORDER BY w."ApprovedOn""#,
                state_case = state_case,
                payment_status_case = payment_status_case,
            );
            let records = sqlx::query_as::<_, WithdrawalRecord>(&sql)
                .fetch_all(pool)
                .await?;
            to_csv_bytes(&records)
        }
        _ => {
            // WithdrawForTenant: filter by ApprovedOn date range; includes exchange_rate + 2h offset
            let sql = format!(
                r#"SELECT
                    CASE WHEN a."AccountNumber" IS NOT NULL AND a."AccountNumber" != 0
                         THEN a."AccountNumber"::text ELSE 'wallet' END       as account_number,
                    COALESCE(a."Uid", 0)                                      as account_uid,
                    p."NativeName"                                            as client_name,
                    c."Code"                                                  as currency,
                    {state_case}                                              as withdrawal_status,
                    {payment_status_case}                                     as payment_status,
                    wp."Id"                                                   as payment_id,
                    wp."Number"                                               as payment_number,
                    pm."Name" || ' ' || COALESCE(wp."ReferenceNumber", '')    as payment_method,
                    w."Amount"::float8 / 1000000.0                           as amount,
                    CASE WHEN COALESCE(wp."Amount", 0) = 0 THEN 0.0
                         ELSE ROUND(w."Amount"::numeric / NULLIF(wp."Amount"::numeric, 0), 4)::float8
                    END                                                       as exchange_rate,
                    TO_CHAR(m."PostedOn" + INTERVAL '2 hours', 'YYYY-MM-DD HH24:MI:SS')    as created_on,
                    TO_CHAR(w."ApprovedOn" + INTERVAL '2 hours', 'YYYY-MM-DD HH24:MI:SS')  as approved_on
                   FROM acct."_Withdrawal" w
                   LEFT JOIN trd."_Account" a      ON a."Id"  = w."SourceAccountId"
                   LEFT JOIN core."_Matter" m      ON m."Id"  = w."Id"
                   LEFT JOIN acct."_Payment" wp    ON wp."Id" = w."PaymentId"
                   LEFT JOIN acct."_PaymentMethod" pm ON pm."Id" = wp."PaymentServiceId"
                   LEFT JOIN acct."_Currency" c    ON c."Id"  = w."CurrencyId"
                   LEFT JOIN core."_Party" p       ON p."Id"  = w."PartyId"
                   WHERE w."ApprovedOn" >= $1 AND w."ApprovedOn" < $2
                   ORDER BY w."ApprovedOn""#,
                state_case = state_case,
                payment_status_case = payment_status_case,
            );
            let records = sqlx::query_as::<_, WithdrawalRecord>(&sql)
                .bind(from)
                .bind(to)
                .fetch_all(pool)
                .await?;
            to_csv_bytes(&records)
        }
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// WalletTransaction report
// Mirrors ProcessWalletTransactionForTenantRequestAsync (complex union of 6 matter types)
// Header: wallet_id,transaction_id,client_name,fund_type,transaction_type,
//         source_currency,source,currency,target,state,source_amount,amount,
//         rebate_target_account_uid,created_on,released_on
// ──────────────────────────────────────────────────────────────────────────────

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct WalletTransactionRecord {
    pub wallet_id: i64,
    pub transaction_id: i64,
    pub client_name: Option<String>,
    pub fund_type: Option<String>,
    pub transaction_type: Option<String>,
    pub source_currency: Option<String>,
    pub source: Option<String>,
    pub currency: Option<String>,
    pub target: Option<String>,
    pub state: Option<String>,
    pub source_amount: Option<f64>,
    pub amount: Option<f64>,
    pub rebate_target_account_uid: Option<String>,
    pub created_on: Option<String>,   // formatted "yyyy-MM-dd HH:mm:ss"
    pub released_on: Option<String>,  // formatted "yyyy-MM-dd HH:mm:ss"
}

/// is_from_api=true (API mode): filter by wt."UpdatedOn" (from-2h, to-2h)
/// is_from_api=false (job mode): filter by Matter.StatedOn (UTC), with ClosedOn for rebates
pub async fn generate_wallet_transaction_csv(
    pool: &PgPool,
    criteria: &DateRangeCriteria,
    is_from_api: bool,
) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(1));
    let to = criteria.to.unwrap_or_else(Utc::now);

    // from/to are MT5 time stored as "UTC"; subtract HOURS_GAP for real UTC
    let from_utc = from - chrono::Duration::hours(HOURS_GAP);
    let to_utc = to - chrono::Duration::hours(HOURS_GAP);
    let to_utc_plus_5m = to_utc + chrono::Duration::minutes(5);

    // State name CASE expression (StateTypes enum)
    let state_case = r#"CASE m."StateId"
        WHEN 0   THEN 'Initialed'
        WHEN 200 THEN 'TransferCreated'
        WHEN 205 THEN 'TransferCanceled'
        WHEN 206 THEN 'TransferFailed'
        WHEN 210 THEN 'TransferAwaitingApproval'
        WHEN 215 THEN 'TransferRejected'
        WHEN 220 THEN 'TransferApproved'
        WHEN 250 THEN 'TransferCompleted'
        WHEN 300 THEN 'DepositCreated'
        WHEN 305 THEN 'DepositCanceled'
        WHEN 306 THEN 'DepositFailed'
        WHEN 310 THEN 'DepositPaymentCompleted'
        WHEN 315 THEN 'DepositCallbackTimeOut'
        WHEN 320 THEN 'DepositCentralApproved'
        WHEN 325 THEN 'DepositCentralRejected'
        WHEN 330 THEN 'DepositTenantApproved'
        WHEN 335 THEN 'DepositTenantRejected'
        WHEN 345 THEN 'DepositCallbackCompleted'
        WHEN 350 THEN 'DepositCompleted'
        WHEN 400 THEN 'WithdrawalCreated'
        WHEN 405 THEN 'WithdrawalCanceled'
        WHEN 406 THEN 'WithdrawalFailed'
        WHEN 410 THEN 'WithdrawalCentralApproved'
        WHEN 415 THEN 'WithdrawalCentralRejected'
        WHEN 420 THEN 'WithdrawalTenantApproved'
        WHEN 425 THEN 'WithdrawalTenantRejected'
        WHEN 430 THEN 'WithdrawalPaymentCompleted'
        WHEN 450 THEN 'WithdrawalCompleted'
        WHEN 500 THEN 'RebateCreated'
        WHEN 505 THEN 'RebateCanceled'
        WHEN 510 THEN 'RebateOnHold'
        WHEN 520 THEN 'RebateReleased'
        WHEN 550 THEN 'RebateCompleted'
        WHEN 590 THEN 'RebateTradeClosedLessThanOneMinute'
        WHEN 600 THEN 'RefundCreated'
        WHEN 650 THEN 'RefundCompleted'
        WHEN 700 THEN 'WalletAdjustCreated'
        WHEN 750 THEN 'WalletAdjustCompleted'
        ELSE m.state_id::text
    END"#;

    let base_sql = format!(r#"
WITH
wallet_adj AS (
    SELECT
        wa."Id",
        wal."CurrencyId" as source_currency_id,
        wal."CurrencyId" as currency_id,
        wa."Amount"      as source_amount,
        wa."Amount"      as amount,
        wa."SourceType"::bigint as source_val,
        0::bigint        as target_val,
        0::bigint        as rebate_uid,
        wa."Amount"      as adj_source_amount
    FROM acct."_WalletAdjust" wa
    JOIN acct."_Wallet" wal ON wal."Id" = wa."WalletId"
),
deposits AS (
    SELECT
        d."Id",
        COALESCE(dp."CurrencyId", d."CurrencyId") as source_currency_id,
        d."CurrencyId" as currency_id,
        COALESCE(dp."Amount", d."Amount") as source_amount,
        d."Amount"     as amount,
        0::bigint      as source_val,
        0::bigint      as target_val,
        0::bigint      as rebate_uid,
        COALESCE(dp."Amount", d."Amount") as adj_source_amount
    FROM acct."_Deposit" d
    LEFT JOIN acct."_Payment" dp ON dp."Id" = d."PaymentId"
),
withdrawals AS (
    SELECT
        w."Id",
        CASE
            WHEN w."SourceAccountId" IS NOT NULL THEN COALESCE(sa."CurrencyId", w."CurrencyId")
            WHEN w."SourceWalletId"  IS NOT NULL THEN COALESCE(sw."CurrencyId", w."CurrencyId")
            ELSE w."CurrencyId"
        END as source_currency_id,
        COALESCE(wp."CurrencyId", w."CurrencyId") as currency_id,
        w."Amount"                   as source_amount,
        COALESCE(wp."Amount", w."Amount") as amount,
        0::bigint as source_val,
        0::bigint as target_val,
        0::bigint as rebate_uid,
        w."Amount" as adj_source_amount
    FROM acct."_Withdrawal" w
    LEFT JOIN trd."_Account"  sa ON sa."Id" = w."SourceAccountId"
    LEFT JOIN acct."_Wallet"  sw ON sw."Id" = w."SourceWalletId"
    LEFT JOIN acct."_Payment" wp ON wp."Id" = w."PaymentId"
),
rebates AS (
    SELECT
        r.id,
        COALESCE(ta."CurrencyId", r.currency_id) as source_currency_id,
        r.currency_id as currency_id,
        r.amount      as source_amount,
        r.amount      as amount,
        COALESCE(tr.ticket, 0) as source_val,
        0::bigint      as target_val,
        a."Uid"        as rebate_uid,
        -- SourceAmount adjusted by exchangeRate from Information JSON (if cross-currency)
        CASE
            WHEN r.information::jsonb ? 'exchangeRate'
                AND COALESCE(ta."CurrencyId", r.currency_id) != r.currency_id
                AND (r.information::jsonb->>'exchangeRate')::float8 > 0
            THEN ROUND(r.amount::numeric / (r.information::jsonb->>'exchangeRate')::numeric, 0)::bigint
            ELSE r.amount
        END as adj_source_amount
    FROM trd.rebate_k8s r
    LEFT JOIN trd.trade_rebate_k8s tr ON tr.id = r.trade_rebate_id
    JOIN  trd."_Account" a  ON a."Id"  = r.account_id
    LEFT JOIN trd."_Account" ta ON ta."Id" = tr.account_id
),
transactions AS (
    SELECT
        t."Id",
        CASE t."SourceAccountType"
            WHEN 2 THEN COALESCE(sa."CurrencyId", 840)
            ELSE 840
        END as source_currency_id,
        CASE t."TargetAccountType"
            WHEN 2 THEN COALESCE(ta."CurrencyId", 840)
            ELSE 840
        END as currency_id,
        t."Amount" as source_amount,
        t."Amount" as amount,
        CASE t."SourceAccountType"
            WHEN 2 THEN COALESCE(sa."AccountNumber", 0)
            ELSE 0
        END as source_val,
        CASE t."TargetAccountType"
            WHEN 2 THEN COALESCE(ta."AccountNumber", 0)
            ELSE 0
        END as target_val,
        0::bigint as rebate_uid,
        t."Amount" as adj_source_amount
    FROM acct."_Transaction" t
    LEFT JOIN trd."_Account" sa ON sa."Id" = t."SourceAccountId" AND t."SourceAccountType" = 2
    LEFT JOIN trd."_Account" ta ON ta."Id" = t."TargetAccountId" AND t."TargetAccountType" = 2
    WHERE t."SourceAccountType" IN (1, 2) AND t."TargetAccountType" IN (1, 2)
),
refunds AS (
    SELECT
        ref."Id",
        ref."CurrencyId" as source_currency_id,
        ref."CurrencyId" as currency_id,
        ref."Amount"     as source_amount,
        ref."Amount"     as amount,
        0::bigint as source_val,
        0::bigint as target_val,
        0::bigint as rebate_uid,
        ref."Amount" as adj_source_amount
    FROM acct."_Refund" ref
),
all_matters AS (
    SELECT "Id", source_currency_id, currency_id, source_amount, amount, source_val, target_val, rebate_uid, adj_source_amount FROM wallet_adj
    UNION ALL
    SELECT "Id", source_currency_id, currency_id, source_amount, amount, source_val, target_val, rebate_uid, adj_source_amount FROM deposits
    UNION ALL
    SELECT "Id", source_currency_id, currency_id, source_amount, amount, source_val, target_val, rebate_uid, adj_source_amount FROM withdrawals
    UNION ALL
    SELECT "Id", source_currency_id, currency_id, source_amount, amount, source_val, target_val, rebate_uid, adj_source_amount FROM rebates
    UNION ALL
    SELECT "Id", source_currency_id, currency_id, source_amount, amount, source_val, target_val, rebate_uid, adj_source_amount FROM transactions
    UNION ALL
    SELECT "Id", source_currency_id, currency_id, source_amount, amount, source_val, target_val, rebate_uid, adj_source_amount FROM refunds
)
SELECT
    wt.wallet_id   as wallet_id,
    wt.matter_id   as transaction_id,
    p."NativeName" as client_name,
    CASE w."FundType"
        WHEN 1 THEN 'Wire'
        WHEN 2 THEN 'Ips'
        WHEN 3 THEN 'FundType3'
        WHEN 4 THEN 'FundType4'
        WHEN 5 THEN 'FundType5'
        ELSE w."FundType"::text
    END as fund_type,
    CASE m.type
        WHEN 0   THEN 'System'
        WHEN 200 THEN 'InternalTransfer'
        WHEN 300 THEN 'Deposit'
        WHEN 400 THEN 'Withdrawal'
        WHEN 500 THEN 'Rebate'
        WHEN 600 THEN 'Refund'
        WHEN 700 THEN 'WalletAdjust'
        ELSE m.type::text
    END as transaction_type,
    COALESCE(sc."Code", '') as source_currency,
    CASE m.type
        WHEN 200 THEN CASE WHEN md.source_val != 0 THEN 'Account No. ' || md.source_val::text ELSE 'Wallet' END
        WHEN 500 THEN 'Ticket No. ' || md.source_val::text
        WHEN 300 THEN 'Deposit Source'
        WHEN 600 THEN 'Refund'
        WHEN 700 THEN CASE WHEN md.source_val = 1 THEN 'Manual Adjust'
                           WHEN md.source_val = 2 THEN 'Sales Rebate'
                           ELSE '' END
        WHEN 0   THEN 'System'
        ELSE ''
    END as source,
    COALESCE(c."Code", '') as currency,
    CASE m.type
        WHEN 200 THEN CASE WHEN md.target_val != 0 THEN 'Account No. ' || md.target_val::text ELSE 'Wallet' END
        WHEN 400 THEN 'Withdrawal Target'
        WHEN 600 THEN 'Wallet'
        WHEN 700 THEN 'Wallet'
        WHEN 0   THEN 'System'
        ELSE ''
    END as target,
    {state_case} as state,
    -- SourceAmount: for InternalTransfer with wallet source, use abs(wt.Amount); else adj_source_amount
    CASE
        WHEN m.type = 200 AND t_join."SourceAccountType" = 1
            THEN ABS(wt.amount)::float8 / 1000000.0
        ELSE md.adj_source_amount::float8 / 1000000.0
    END as source_amount,
    -- Amount: for InternalTransfer with wallet target, use abs(wt.Amount); else amount
    CASE
        WHEN m.type = 200 AND t_join."TargetAccountType" = 1
            THEN ABS(wt.amount)::float8 / 1000000.0
        ELSE md.amount::float8 / 1000000.0
    END as amount,
    CASE WHEN m.type = 500 AND md.rebate_uid != 0 THEN md.rebate_uid::text ELSE '' END as rebate_target_account_uid,
    TO_CHAR(m.posted_on + INTERVAL '2 hours', 'YYYY-MM-DD HH24:MI:SS') as created_on,
    CASE
        WHEN m.type != 400
            THEN TO_CHAR(m.stated_on + INTERVAL '2 hours', 'YYYY-MM-DD HH24:MI:SS')
        ELSE TO_CHAR(wd."ApprovedOn" + INTERVAL '2 hours', 'YYYY-MM-DD HH24:MI:SS')
    END as released_on
FROM acct.wallet_transaction_k8s wt
JOIN acct."_Wallet" w ON w."Id" = wt.wallet_id
JOIN core.matter_k8s m ON m.id = wt.matter_id
LEFT JOIN core."_Party" p ON p."Id" = w."PartyId"
JOIN all_matters md ON md."Id" = m.id
LEFT JOIN acct."_Currency" c  ON c."Id"  = md.currency_id
LEFT JOIN acct."_Currency" sc ON sc."Id" = md.source_currency_id
LEFT JOIN acct."_Transaction" t_join ON t_join."Id" = m.id
LEFT JOIN acct."_Withdrawal"  wd      ON wd."Id"     = m.id
"#, state_case = state_case);

    let records = if is_from_api {
        // API mode: filter by wt."UpdatedOn" (dates already in MT5 time, subtract HOURS_GAP)
        let sql = format!(
            r#"{} WHERE wt.updated_on >= $1 AND wt.updated_on <= $2 ORDER BY wt.id"#,
            base_sql
        );
        sqlx::query_as::<_, WalletTransactionRecord>(&sql)
            .bind(from_utc)
            .bind(to_utc)
            .fetch_all(pool)
            .await?
    } else {
        // Job mode: filter by Matter.StatedOn (UTC), with TradeRebate.ClosedOn for rebates
        let sql = format!(
            r#"{}
LEFT JOIN (
    SELECT r2.id, tr2.closed_on
    FROM trd.rebate_k8s r2
    LEFT JOIN trd.trade_rebate_k8s tr2 ON tr2.id = r2.trade_rebate_id
) rebate_cl ON rebate_cl.id = m.id
WHERE m.stated_on >= $3 AND m.stated_on < $4
  AND (
    rebate_cl.closed_on IS NULL
    OR (rebate_cl.closed_on >= $1 AND rebate_cl.closed_on <= $2)
  )
ORDER BY wt.id"#,
            base_sql
        );
        sqlx::query_as::<_, WalletTransactionRecord>(&sql)
            .bind(from)        // ClosedOn from (MT5 time)
            .bind(to)          // ClosedOn to   (MT5 time)
            .bind(from_utc)    // StatedOn from (UTC)
            .bind(to_utc_plus_5m) // StatedOn to (UTC + 5min buffer)
            .fetch_all(pool)
            .await?
    };

    to_csv_bytes(&records)
}

// ──────────────────────────────────────────────────────────────────────────────
// WalletDailySnapshot report
// Mirrors ProcessWalletDailySnapshotForTenantRequestAsync (snapshot table version)
// Header: id,name,email,currency,fund_type,amount,status
// ──────────────────────────────────────────────────────────────────────────────

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct WalletSnapshotRecord {
    pub id: i64,
    pub name: Option<String>,
    pub email: Option<String>,
    pub currency: Option<String>,
    pub fund_type: Option<String>,
    pub amount: Option<f64>,
    pub status: Option<String>,
}

#[derive(Debug, serde::Deserialize)]
struct WalletSnapshotCriteria {
    #[serde(rename = "snapshotDate")]
    snapshot_date: Option<DateTime<Utc>>,
}

pub async fn generate_wallet_snapshot_csv(pool: &PgPool, query_json: &str) -> Result<Vec<u8>> {
    let criteria: WalletSnapshotCriteria = serde_json::from_str(query_json).unwrap_or(WalletSnapshotCriteria {
        snapshot_date: None,
    });
    // Subtract HOURS_GAP to convert MT5 time to UTC (mirrors DateHelper.MinusMT5GMTHours)
    let date = criteria.snapshot_date
        .map(|d| d - chrono::Duration::hours(HOURS_GAP))
        .unwrap_or_else(Utc::now);

    let records = sqlx::query_as::<_, WalletSnapshotRecord>(
        r#"SELECT
            w."Id"           as id,
            p."NativeName"   as name,
            p."Email"        as email,
            c."Code"         as currency,
            CASE w."FundType"
                WHEN 1 THEN 'Wire'
                WHEN 2 THEN 'Ips'
                WHEN 3 THEN 'FundType3'
                WHEN 4 THEN 'FundType4'
                WHEN 5 THEN 'FundType5'
                ELSE w."FundType"::text
            END              as fund_type,
            s."Balance"::float8 / 1000000.0 as amount,
            CASE p."Status"
                WHEN 1 THEN 'Active'
                ELSE 'Closed'
            END              as status
           FROM acct."_WalletDailySnapshot" s
           JOIN acct."_Wallet" w ON w."Id" = s."WalletId"
           LEFT JOIN core."_Party" p ON p."Id" = w."PartyId"
           LEFT JOIN acct."_Currency" c ON c."Id" = w."CurrencyId"
           WHERE s."SnapshotDate"::date = $1::date
           ORDER BY s."WalletId""#,
    )
    .bind(date)
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}

// ──────────────────────────────────────────────────────────────────────────────
// WalletOverview report
// Mirrors ProcessWalletOverviewForTenantRequestAsync — just reads acct."_Wallet" directly.
// Header: id,name,email,currency,fund_type,amount,status
// ──────────────────────────────────────────────────────────────────────────────

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct WalletOverviewRecord {
    pub id: i64,
    pub name: Option<String>,
    pub email: Option<String>,
    pub currency: Option<String>,
    pub fund_type: Option<String>,
    pub amount: Option<f64>,
    pub status: Option<String>,
}

pub async fn generate_wallet_overview_csv(pool: &PgPool, criteria: &DateRangeCriteria) -> Result<Vec<u8>> {
    let _from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(30));
    let _to = criteria.to.unwrap_or_else(Utc::now);

    let records = sqlx::query_as::<_, WalletOverviewRecord>(
        r#"SELECT
            w."Id"         as id,
            p."NativeName" as name,
            p."Email"      as email,
            c."Code"       as currency,
            CASE w."FundType"
                WHEN 1 THEN 'Wire'
                WHEN 2 THEN 'Ips'
                WHEN 3 THEN 'FundType3'
                WHEN 4 THEN 'FundType4'
                WHEN 5 THEN 'FundType5'
                ELSE w."FundType"::text
            END            as fund_type,
            w."Balance"::float8 / 1000000.0 as amount,
            CASE p."Status"
                WHEN 1 THEN 'Active'
                ELSE 'Closed'
            END            as status
           FROM acct."_Wallet" w
           LEFT JOIN core."_Party" p ON p."Id" = w."PartyId"
           LEFT JOIN acct."_Currency" c ON c."Id" = w."CurrencyId"
           WHERE w."Balance" != 0
           ORDER BY w."Id""#,
    )
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}

// ──────────────────────────────────────────────────────────────────────────────
// Transaction report (InternalTransfer: wallet ↔ account)
// Mirrors ProcessTransactionForTenantRequestAsync
// Header: client_name,source_account_number,target_account_number,currency,
//         transaction_status,amount,created_on
// ──────────────────────────────────────────────────────────────────────────────

#[derive(Debug, Serialize, Deserialize, sqlx::FromRow)]
pub struct TransactionRecord {
    pub client_name: Option<String>,
    pub source_account_number: Option<String>,
    pub target_account_number: Option<String>,
    pub currency: Option<String>,
    pub transaction_status: Option<String>,
    pub amount: Option<f64>,
    pub created_on: Option<String>,  // formatted "yyyy-MM-dd HH:mm:ss"
}

pub async fn generate_transaction_csv(pool: &PgPool, criteria: &DateRangeCriteria) -> Result<Vec<u8>> {
    let from = criteria.from.unwrap_or_else(|| Utc::now() - chrono::Duration::days(30));
    let to = criteria.to.unwrap_or_else(Utc::now);

    // CreatedOn = Matter.PostedOn + HOURS_GAP (mirrors TransactionRecord.ToCsv)
    // Amount = Transaction.Amount / 100 (Transaction stores as ×100 scale)
    // SourceAccountId / TargetAccountId reference trd."_TradeAccount"."Id"
    // When type=1 (Wallet) there's no matching TradeAccount → show "wallet"
    let records = sqlx::query_as::<_, TransactionRecord>(
        r#"SELECT
            p."NativeName"  as client_name,
            CASE WHEN sta."AccountNumber" IS NOT NULL THEN sta."AccountNumber"::text ELSE 'wallet' END as source_account_number,
            CASE WHEN tta."AccountNumber" IS NOT NULL THEN tta."AccountNumber"::text ELSE 'wallet' END as target_account_number,
            c."Code"        as currency,
            CASE m."StateId"
                WHEN 0   THEN 'Initialed'
                WHEN 200 THEN 'TransferCreated'
                WHEN 205 THEN 'TransferCanceled'
                WHEN 250 THEN 'TransferCompleted'
                ELSE m."StateId"::text
            END             as transaction_status,
            t."Amount"::float8 / 100.0 as amount,
            TO_CHAR(m."PostedOn" + INTERVAL '2 hours', 'YYYY-MM-DD HH24:MI:SS') as created_on
           FROM acct."_Transaction" t
           LEFT JOIN core."_Party" p       ON p."Id"  = (SELECT a2."PartyId" FROM trd."_Account" a2 WHERE a2."Id" = t."SourceAccountId" LIMIT 1)
           LEFT JOIN trd."_TradeAccount" sta ON sta."Id" = t."SourceAccountId"
           LEFT JOIN trd."_TradeAccount" tta ON tta."Id" = t."TargetAccountId"
           LEFT JOIN core."_Matter" m      ON m."Id"  = t."Id"
           LEFT JOIN acct."_Currency" c    ON c."Id"  = t."CurrencyId"
           WHERE m."StatedOn" >= $1 AND m."StatedOn" < $2
           ORDER BY m."StatedOn""#,
    )
    .bind(from)
    .bind(to)
    .fetch_all(pool)
    .await?;

    to_csv_bytes(&records)
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::config::Config;
    use crate::storage::s3::S3Storage;

    #[tokio::test]
    #[ignore = "requires live DB + S3 credentials, run manually with: cargo test -- --ignored"]
    async fn test_wallet_transaction_csv_and_s3_upload() {
        dotenvy::dotenv().ok();
        let config = Config::from_env().expect("config");

        let db_url = format!(
            "postgresql://{}:{}@{}:{}/portal_tenant_bvi",
            config.db_user, config.db_password, config.db_host, config.db_port
        );
        let pool = sqlx::PgPool::connect(&db_url).await.expect("db connect");
        let s3 = S3Storage::new(&config).await.expect("s3 init");

        let criteria = DateRangeCriteria {
            from: Some("2026-01-01T00:00:00Z".parse().unwrap()),
            to: Some("2026-03-19T23:59:59Z".parse().unwrap()),
            emails: None,
        };

        let csv_bytes = generate_wallet_transaction_csv(&pool, &criteria, false)
            .await
            .expect("generate csv");

        tracing::debug!("CSV rows bytes: {}", csv_bytes.len());
        tracing::debug!("CSV preview:\n{}", String::from_utf8_lossy(&csv_bytes[..csv_bytes.len().min(500)]));

        let key = "test/wallet_transaction_test.csv".to_string();
        s3.upload_csv(&key, csv_bytes).await.expect("s3 upload");
        tracing::info!("Uploaded to S3 key: {}", key);
    }
}
