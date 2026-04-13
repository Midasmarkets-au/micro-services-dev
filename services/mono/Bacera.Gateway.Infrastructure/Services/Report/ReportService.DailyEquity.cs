using Bacera.Gateway.Services.Report.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway.Services;

partial class ReportService
{
    public async Task<List<DailyEquityRecord>> DailyEquityReportQueryAsync(
        DailyEquity.Criteria criteria,
        double timeZoneOffset = 0)
    {
        var fromDT = criteria.From;
        var toDT = criteria.To;
        var useClosingTime = criteria.UseClosingTime ?? false;
        var closedOnFrom = criteria.ClosedOnFrom;
        var closedOnTo = criteria.ClosedOnTo;

        if (fromDT == null || toDT == null)
        {
            throw new ArgumentException("From and To dates are required for Daily Equity Report");
        }

        // Step 1: Query PostgreSQL for account data, transactions, rebates, etc.
        var postgresResults = await QueryPostgreSQLForDailyEquityAsync(
            fromDT.Value, 
            toDT.Value, 
            useClosingTime,
            closedOnFrom,
            closedOnTo);

        // Step 2: For USD and USC rows with AccountList, query MySQL for MT5 data
        // For monthly reports (span > 25h), pass the original start date for MT5 range
        DateTime? monthlyMt5Start = null;
        if (closedOnFrom.HasValue && closedOnTo.HasValue && (closedOnTo.Value - closedOnFrom.Value).TotalHours > 25)
            monthlyMt5Start = closedOnFrom.Value.Date;

        var mysqlResults = await QueryMySQLForDailyEquityAsync(postgresResults, fromDT.Value, toDT.Value, timeZoneOffset, monthlyMt5Start);

        // Step 3: Merge results and calculate final values
        var finalRecords = MergeDailyEquityResults(postgresResults, mysqlResults);

        return finalRecords;
    }

    private async Task<List<DailyEquityPostgresResult>> QueryPostgreSQLForDailyEquityAsync(
        DateTime from,
        DateTime to,
        bool useClosingTime,
        DateTime? closedOnFrom,
        DateTime? closedOnTo)
    {
        // Build the full SQL query from the requirement document
        var sql = $"""
-- @fromDT: '2026-01-21 22:00:00'
-- @toDT: '2026-01-22 22:00:00'
-- @prevDT: '2026-01-21'
-- @currDT: '2026-01-22'
-- @closedOnFrom: '2026-01-22 00:00:00'
-- @closedOnTo: '2026-01-22 23:59:59'

WITH
    -- ============================================
    -- 0. Parameters (统一参数入口)
    -- ============================================
    params AS (
        SELECT
            @fromDT       ::timestamptz AS fromDT,
            @toDT         ::timestamptz AS toDT,
            @prevDT       ::date        AS prevDT,
            @currDT       ::date        AS currDT,
            @closedOnFrom ::timestamptz AS closedOnFrom,
            @closedOnTo   ::timestamptz AS closedOnTo
    ),

    -- ============================================
    -- 1. Client账户基础数据 (Role=400)
    -- ============================================
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

    -- ============================================
    -- 2. Agent账户基础数据 (Role=300)
    -- ============================================
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

    -- ============================================
    -- 2.1 账号列表汇总
    -- ============================================
    client_account_list AS (
        SELECT
            sales_code,
            "CurrencyId",
            STRING_AGG("AccountNumber"::text, ',' ORDER BY "AccountNumber") AS account_list,
            -- Use MAX to get the non-zero ServiceId (in case some accounts have 0 or NULL ServiceId)
            -- Filter out 0 and NULL values, then take MAX. If all are 0/NULL, COALESCE to 0
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

    -- ============================================
    -- 3. 新开户数 (New Acc)
    -- ============================================
    new_accounts AS (
        SELECT sales_code, "CurrencyId", COUNT(*) AS new_acc_count
        FROM client_accounts
		CROSS JOIN params p
        WHERE "CreatedOn" >= p.fromDT AND "CreatedOn" < p.toDT
        GROUP BY sales_code, "CurrencyId"
    ),

    -- ============================================
    -- 3.1 新用户数 (New User) - 按 Office 分组，不分 Currency
    -- ============================================
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

    -- ============================================
    -- 3.2 Wallet Previous Equity (前一天钱包余额 - Starting Balance)
    -- ============================================
    wallet_prev_equity AS (
        SELECT sales_code, SUM(balance) AS total_balance
        FROM (
            SELECT DISTINCT ON (wds."WalletId")
                CASE
                    WHEN a_sales."Code" IS NOT NULL THEN a_sales."Code"
                    WHEN sa."Code" IS NOT NULL THEN sa."Code"
                    ELSE 'NO_SALES'
                END AS sales_code,
                wds."Balance" / 1000000.0 AS balance
            FROM acct."_WalletDailySnapshot" wds
            JOIN acct."_Wallet" w ON wds."WalletId" = w."Id"
            LEFT JOIN trd."_Account" a_sales ON w."PartyId" = a_sales."PartyId" AND a_sales."Role" = 100
            LEFT JOIN (
                SELECT DISTINCT ON ("PartyId")
                    "Id", "PartyId", "SalesAccountId", "Role", "CurrencyId"
                FROM trd."_Account"
                WHERE "Role" IN (300, 400)
                ORDER BY "PartyId", "Role"
            ) a_client ON w."PartyId" = a_client."PartyId" AND a_client."CurrencyId" = w."CurrencyId"
            LEFT JOIN trd."_Account" sa ON a_client."SalesAccountId" = sa."Id"
            CROSS JOIN params p
            WHERE w."CurrencyId" = 840
              AND wds."SnapshotDate"::date = p.prevDT::date
            ORDER BY wds."WalletId", wds."SnapshotDate" DESC
        ) sub
        GROUP BY sales_code
    ),

    -- ============================================
    -- 3.3 Wallet Current Equity (当天钱包余额 - Ending Balance)
    -- ============================================
    wallet_curr_equity AS (
        SELECT sales_code, SUM(balance) AS total_balance
        FROM (
            SELECT DISTINCT ON (wds."WalletId")
                CASE
                    WHEN a_sales."Code" IS NOT NULL THEN a_sales."Code"
                    WHEN sa."Code" IS NOT NULL THEN sa."Code"
                    ELSE 'NO_SALES'
                END AS sales_code,
                wds."Balance" / 1000000.0 AS balance
            FROM acct."_WalletDailySnapshot" wds
            JOIN acct."_Wallet" w ON wds."WalletId" = w."Id"
            LEFT JOIN trd."_Account" a_sales ON w."PartyId" = a_sales."PartyId" AND a_sales."Role" = 100
            LEFT JOIN (
                SELECT DISTINCT ON ("PartyId")
                    "Id", "PartyId", "SalesAccountId", "Role", "CurrencyId"
                FROM trd."_Account"
                WHERE "Role" IN (300, 400)
                ORDER BY "PartyId", "Role"
            ) a_client ON w."PartyId" = a_client."PartyId" AND a_client."CurrencyId" = w."CurrencyId"
            LEFT JOIN trd."_Account" sa ON a_client."SalesAccountId" = sa."Id"
            CROSS JOIN params p
            WHERE w."CurrencyId" = 840
              AND wds."SnapshotDate"::date = p.currDT::date
            ORDER BY wds."WalletId", wds."SnapshotDate" DESC
        ) sub
        GROUP BY sales_code
    ),

    -- ============================================
    -- 4. Margin In (Deposit + Account→Account 接收方)
    -- ============================================
    margin_in_deposit AS (
        WITH deposit_first_completion AS (
            SELECT 
                a."MatterId",
                MIN(a."PerformedOn") AS first_completion_time
            FROM core."_Activity" a
            WHERE a."ToStateId" IN (345, 350) -- DepositCallbackCompleted or DepositCompleted
            GROUP BY a."MatterId"
        )
        SELECT ca.sales_code, ca."CurrencyId", SUM(d."Amount") AS total_amount
        FROM deposit_first_completion dfc
        JOIN acct."_Deposit" d ON d."Id" = dfc."MatterId"
        JOIN client_accounts ca ON d."TargetAccountId" = ca.account_id
        CROSS JOIN params p
        WHERE dfc.first_completion_time >= p.fromDT 
          AND dfc.first_completion_time < p.toDT
        GROUP BY ca.sales_code, ca."CurrencyId"
    ),

    margin_in_acc2acc AS (
        SELECT ca.sales_code, ca."CurrencyId",
        SUM(CASE
            WHEN ca."CurrencyId" = 841 AND t."CurrencyId" = 840 THEN t."Amount" * 100
            WHEN ca."CurrencyId" = 840 AND t."CurrencyId" = 841 THEN t."Amount" / 100
            ELSE t."Amount"
        END) AS total_amount
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

    -- ============================================
    -- 5. Margin Out (Withdrawal + Account→Account 发送方)
    -- ============================================
    margin_out_withdrawal AS (
        WITH withdrawal_first_approval AS (
            SELECT 
                a."MatterId",
                MIN(a."PerformedOn") AS first_approval_time
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
        SELECT ca.sales_code, ca."CurrencyId",
        SUM(CASE
            WHEN ca."CurrencyId" = 841 AND t."CurrencyId" = 840 THEN t."Amount" * 100
            WHEN ca."CurrencyId" = 840 AND t."CurrencyId" = 841 THEN t."Amount" / 100
            ELSE t."Amount"
        END) AS total_amount
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

    -- ============================================
    -- 6. Wallet Margin Out (Withdrawal from Wallet)
    -- ============================================
    wallet_margin_out AS (
        WITH wallet_withdrawal_first_approval AS (
            SELECT 
                a."MatterId",
                MIN(a."PerformedOn") AS first_approval_time
            FROM core."_Activity" a
            WHERE a."ToStateId" = 420
            GROUP BY a."MatterId"
        ),
        wallet_withdrawal_with_sales AS (
            SELECT DISTINCT ON (wd."Id")
                wd."Id" AS withdrawal_id,
                w."CurrencyId",
                wd."Amount",
                CASE
                    WHEN a_sales."Code" IS NOT NULL THEN a_sales."Code"
                    WHEN sa."Code" IS NOT NULL THEN sa."Code"
                    ELSE 'NO_SALES'
                END AS sales_code,
                wfa.first_approval_time
            FROM core."_Matter" m
            JOIN acct."_Withdrawal" wd ON wd."Id" = m."Id"
            JOIN wallet_withdrawal_first_approval wfa ON wfa."MatterId" = m."Id"
            JOIN acct."_Wallet" w ON wd."SourceWalletId" = w."Id"
            LEFT JOIN trd."_Account" a_sales ON w."PartyId" = a_sales."PartyId" AND a_sales."Role" = 100
            LEFT JOIN (
                SELECT DISTINCT ON ("PartyId")
                    "Id", "PartyId", "SalesAccountId", "Role", "CurrencyId"
                FROM trd."_Account"
                WHERE "Role" IN (300, 400)
                ORDER BY "PartyId", "Role"
            ) a_client ON w."PartyId" = a_client."PartyId" AND a_client."CurrencyId" = w."CurrencyId"
            LEFT JOIN trd."_Account" sa ON a_client."SalesAccountId" = sa."Id"
            CROSS JOIN params p
            WHERE wd."SourceAccountId" IS NULL
              AND wfa.first_approval_time >= p.fromDT 
              AND wfa.first_approval_time < p.toDT
            ORDER BY wd."Id"
        )
        SELECT
            sales_code,
            "CurrencyId",
            SUM("Amount") AS total_amount
        FROM wallet_withdrawal_with_sales
        GROUP BY sales_code, "CurrencyId"
    ),

    -- ============================================
    -- 7. Transfer In (Wallet → Account)
    -- ============================================
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

    -- ============================================
    -- 8. Transfer Out (Account → Wallet)
    -- ============================================
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

    -- ============================================
    -- 9. Transfer Out (from Wallet to Account) - for Wallet row
    -- 按钱包货币分组，跨币种做汇率转换
    -- ============================================
    transfer_out_wallet AS (
        WITH transfer_out_with_sales AS (
            SELECT DISTINCT ON (t."Id")
                t."Id" AS transaction_id,
                w."CurrencyId",
                t."CurrencyId" AS target_currency_id,
                t."Amount",
                CASE
                    WHEN a_sales."Code" IS NOT NULL THEN a_sales."Code"
                    WHEN sa."Code" IS NOT NULL THEN sa."Code"
                    ELSE 'NO_SALES'
                END AS sales_code
            FROM core."_Matter" m
            JOIN acct."_Transaction" t ON t."Id" = m."Id"
            JOIN acct."_Wallet" w ON t."SourceAccountId" = w."Id"
            LEFT JOIN trd."_Account" a_sales ON w."PartyId" = a_sales."PartyId" AND a_sales."Role" = 100
            LEFT JOIN (
                SELECT DISTINCT ON ("PartyId")
                    "Id", "PartyId", "SalesAccountId", "Role", "CurrencyId"
                FROM trd."_Account"
                WHERE "Role" IN (300, 400)
                ORDER BY "PartyId", "Role"
            ) a_client ON w."PartyId" = a_client."PartyId" AND a_client."CurrencyId" = w."CurrencyId"
            LEFT JOIN trd."_Account" sa ON a_client."SalesAccountId" = sa."Id"
            CROSS JOIN params p
            WHERE m."StateId" = 250 AND t."SourceAccountType" = 1
              AND m."StatedOn" >= p.fromDT AND m."StatedOn" < p.toDT
            ORDER BY t."Id"
        )
        SELECT
            sales_code,
            "CurrencyId",
            SUM(CASE
                -- USD钱包 → USC账户: Transaction.Amount 是 USC，除以 100 转成 USD
                WHEN "CurrencyId" = 840 AND target_currency_id = 841 THEN "Amount" / 100
                -- USC钱包 → USD账户: Transaction.Amount 是 USD，乘以 100 转成 USC
                WHEN "CurrencyId" = 841 AND target_currency_id = 840 THEN "Amount" * 100
                -- 同币种，不转换
                ELSE "Amount"
            END) AS total_amount
        FROM transfer_out_with_sales
        GROUP BY sales_code, "CurrencyId"
    ),

    -- ============================================
    -- 10. Transfer In (to Wallet from Account) - for Wallet row
    -- 按钱包货币分组，跨币种做汇率转换
    -- ============================================
    transfer_in_wallet AS (
        WITH transfer_in_with_sales AS (
            SELECT DISTINCT ON (t."Id")
                t."Id" AS transaction_id,
                w."CurrencyId",
                t."CurrencyId" AS source_currency_id,
                t."Amount",
                CASE
                    WHEN a_sales."Code" IS NOT NULL THEN a_sales."Code"
                    WHEN sa."Code" IS NOT NULL THEN sa."Code"
                    ELSE 'NO_SALES'
                END AS sales_code
            FROM core."_Matter" m
            JOIN acct."_Transaction" t ON t."Id" = m."Id"
            JOIN acct."_Wallet" w ON t."TargetAccountId" = w."Id"
            LEFT JOIN trd."_Account" a_sales ON w."PartyId" = a_sales."PartyId" AND a_sales."Role" = 100
            LEFT JOIN (
                SELECT DISTINCT ON ("PartyId")
                    "Id", "PartyId", "SalesAccountId", "Role", "CurrencyId"
                FROM trd."_Account"
                WHERE "Role" IN (300, 400)
                ORDER BY "PartyId", "Role"
            ) a_client ON w."PartyId" = a_client."PartyId" AND a_client."CurrencyId" = w."CurrencyId"
            LEFT JOIN trd."_Account" sa ON a_client."SalesAccountId" = sa."Id"
            CROSS JOIN params p
            WHERE m."StateId" = 250 AND t."TargetAccountType" = 1
              AND m."StatedOn" >= p.fromDT AND m."StatedOn" < p.toDT
            ORDER BY t."Id"
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

    -- ============================================
    -- 11. Credit Adjust
    -- ============================================
    credit_adjust AS (
        SELECT ca.sales_code, ca."CurrencyId", SUM(ar."Amount") AS total_amount
        FROM trd."_AdjustRecord" ar
        JOIN client_accounts ca ON ar."AccountId" = ca.account_id
        CROSS JOIN params p
        WHERE ar."Type" = 2 AND ar."Status" = 20
          AND ar."UpdatedOn" >= p.fromDT AND ar."UpdatedOn" < p.toDT
        GROUP BY ca.sales_code, ca."CurrencyId"
    ),

    -- ============================================
    -- 12. Balance Adjust
    -- ============================================
    balance_adjust AS (
        SELECT ca.sales_code, ca."CurrencyId", SUM(ar."Amount") AS total_amount
        FROM trd."_AdjustRecord" ar
        JOIN client_accounts ca ON ar."AccountId" = ca.account_id
        CROSS JOIN params p
        WHERE ar."Type" IN (1, 3) AND ar."Status" = 20
          AND ar."UpdatedOn" >= p.fromDT AND ar."UpdatedOn" < p.toDT
        GROUP BY ca.sales_code, ca."CurrencyId"
    ),

    -- ============================================
    -- 12.1 Wallet Adjust - CORRECTED VERSION
    -- ============================================
    wallet_adjust AS (
        SELECT
            CASE
                -- For Sales accounts (Role 100), use their own Code
                WHEN a_sales."Code" IS NOT NULL THEN a_sales."Code"
                -- For Client/Agent accounts (Role 300/400), use their SalesAccountId's Code
                WHEN sa."Code" IS NOT NULL THEN sa."Code"
                -- Otherwise, NO_SALES
                ELSE 'NO_SALES'
            END AS sales_code,
            SUM(wa."Amount") AS total_amount
        FROM acct."_WalletAdjust" wa
        JOIN core."_Matter" m ON wa."Id" = m."Id"
        JOIN acct."_Wallet" w ON wa."WalletId" = w."Id"
        -- Try to find Sales account (Role 100)
        LEFT JOIN trd."_Account" a_sales ON w."PartyId" = a_sales."PartyId" AND a_sales."Role" = 100
        -- Try to find Client/Agent account (Role 300/400)
        LEFT JOIN (
		    SELECT DISTINCT ON ("PartyId")
			    "Id", "PartyId", "SalesAccountId", "Role"
		    FROM trd."_Account"
		    WHERE "Role" IN (300, 400)
		    ORDER BY "PartyId", "Role"  -- explicit priority: 300 before 400
	    ) a_client ON w."PartyId" = a_client."PartyId"
        -- Get the SalesAccountId for clients
        LEFT JOIN trd."_Account" sa ON a_client."SalesAccountId" = sa."Id"
        CROSS JOIN params p
        WHERE m."StateId" = 750  -- WalletAdjustCompleted
            AND w."CurrencyId" = 840  -- USD Wallet only
            AND m."StatedOn" >= p.fromDT AND m."StatedOn" < p.toDT
        GROUP BY 
            CASE
                WHEN a_sales."Code" IS NOT NULL THEN a_sales."Code"
                WHEN sa."Code" IS NOT NULL THEN sa."Code"
                ELSE 'NO_SALES'
            END
    ),

    -- ============================================
    -- 13. Rebate USD
    -- ============================================
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
          {(useClosingTime && closedOnFrom.HasValue && closedOnTo.HasValue ? """AND tr."ClosedOn" >= p.closedOnFrom AND tr."ClosedOn" <= p.closedOnTo""" : "")}
        GROUP BY sa."Code"
    ),

    -- ============================================
    -- 14. Rebate USC
    -- ============================================
    rebate_usc AS (
        SELECT
            COALESCE(sa."Code", 'NO_SALES') AS sales_code,
            SUM(wt."Amount") AS total_amount
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
          {(useClosingTime && closedOnFrom.HasValue && closedOnTo.HasValue ? """AND tr."ClosedOn" >= p.closedOnFrom AND tr."ClosedOn" <= p.closedOnTo""" : "")}
        GROUP BY sa."Code"
    ),

    -- ============================================
    -- 15. Rebate Wallet
    -- ============================================
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

    -- ============================================
    -- 16. All Sales Codes & Combinations
    -- ============================================
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

-- ============================================
-- 最终汇总
-- ============================================
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
    CASE
        WHEN ac.currency_name = 'Wallet' THEN COALESCE(wpe.total_balance, 0)
        ELSE 0
    END AS "PrevEquity",
    CASE
        WHEN ac.currency_name = 'Wallet' THEN COALESCE(wce.total_balance, 0)
        ELSE 0
    END AS "CurrEquity",
    -- Transfer amounts merged into Margin In/Out
    CASE
        WHEN ac.currency_name = 'User' THEN 0
        WHEN ac.currency_name = 'Wallet' THEN COALESCE(tiw.total_amount, 0) / 1000000.0
        ELSE (COALESCE(mi.total_amount, 0) + COALESCE(ti.total_amount, 0)) / 1000000.0
    END AS "MarginIn",
    CASE
        WHEN ac.currency_name = 'User' THEN 0
        WHEN ac.currency_name = 'Wallet' THEN (COALESCE(wmo.total_amount, 0) + COALESCE(tow.total_amount, 0)) / 1000000.0
        ELSE (COALESCE(mo.total_amount, 0) + COALESCE(tro.total_amount, 0)) / 1000000.0
    END AS "MarginOut",
    0 AS "TransferIn",
    0 AS "TransferOut",
    CASE WHEN ac.currency_name IN ('Wallet', 'User') THEN 0 ELSE COALESCE(cr.total_amount, 0) / 1000000.0 END AS "Credit",
    -- UPDATED: Adjust now includes WalletAdjust for Wallet row
    CASE
        WHEN ac.currency_name = 'Wallet' THEN COALESCE(wadj.total_amount, 0) / 1000000.0
        WHEN ac.currency_name = 'User' THEN 0
        ELSE COALESCE(adj.total_amount, 0) / 1000000.0
    END AS "Adjust",
    CASE
        WHEN ac.currency_name = 'USD' THEN COALESCE(rbu.total_amount, 0) / 1000000.0
        WHEN ac.currency_name = 'USC' THEN COALESCE(rbs.total_amount, 0) / 1000000.0
        WHEN ac.currency_name = 'Wallet' THEN COALESCE(rbw.total_amount, 0) / 1000000.0
        ELSE 0
    END AS "Rebate"

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
LEFT JOIN wallet_adjust wadj ON ac.sales_code = wadj.sales_code AND ac.currency_name = 'Wallet'  -- NEW
LEFT JOIN rebate_usd rbu ON ac.sales_code = rbu.sales_code AND ac.currency_name = 'USD'
LEFT JOIN rebate_usc rbs ON ac.sales_code = rbs.sales_code AND ac.currency_name = 'USC'
LEFT JOIN rebate_wallet rbw ON ac.sales_code = rbw.sales_code AND ac.currency_name = 'Wallet'
LEFT JOIN wallet_margin_out wmo ON ac.sales_code = wmo.sales_code AND wmo."CurrencyId" = 840 AND ac.currency_name = 'Wallet'
LEFT JOIN transfer_in_wallet tiw ON ac.sales_code = tiw.sales_code AND tiw."CurrencyId" = 840 AND ac.currency_name = 'Wallet'
LEFT JOIN transfer_out_wallet tow ON ac.sales_code = tow.sales_code AND tow."CurrencyId" = 840 AND ac.currency_name = 'Wallet'

WHERE ac.sales_code != 'SYDS1'  -- Exclude SYDS1 as per requirement
ORDER BY ac.sales_code, CASE ac.currency_name WHEN 'User' THEN 0 WHEN 'USD' THEN 1 WHEN 'USC' THEN 2 WHEN 'Wallet' THEN 3 END;
""";

        // Calculate previous date for wallet snapshots
        var prevDT = from.ToString("yyyy-MM-dd");
        var currDT = to.ToString("yyyy-MM-dd");

        var parameters = new
        {
            fromDT = from,
            toDT = to,
            prevDT = prevDT,
            currDT = currDT,
            closedOnFrom = closedOnFrom,
            closedOnTo = closedOnTo
        };

        var results = await tenantCon.ToListAsync<DailyEquityPostgresResult>(sql, parameters);

        // Debug logging: record all offices' USD/USC/Wallet account information
        var accountRecords = results
            .Where(r => (r.Currency == "USD" || r.Currency == "USC" || r.Currency == "Wallet") && !string.IsNullOrEmpty(r.AccountList))
            .OrderBy(r => r.Office)
            .ThenBy(r => r.Currency)
            .ToList();
        
        if (accountRecords.Any())
        {
            logger.LogInformation($"[DailyEquity PostgreSQL] Found {accountRecords.Count} office-currency records with accounts:");
            foreach (var record in accountRecords)
            {
                logger.LogInformation($" - Office={record.Office}, Currency={record.Currency}, AccountList={record.AccountList}, ServiceId={record.ServiceId})");
            }
        }
        else
        {
            logger.LogWarning("[DailyEquity PostgreSQL] No USD/USC/Wallet accounts found in PostgreSQL results!");
        }
        
        return results;
    }

    private async Task<List<DailyEquityMysqlResult>> QueryMySQLForDailyEquityAsync(
        List<DailyEquityPostgresResult> postgresResults,
        DateTime from,
        DateTime to,
        double timeZoneOffset,
        DateTime? monthlyMt5Start = null)
    {
        var mysqlResults = new List<DailyEquityMysqlResult>();

        // Group postgres results by ServiceId and get accounts for MT5 queries
        var accountGroups = postgresResults
            .Where(p => !string.IsNullOrEmpty(p.AccountList) && 
                       (p.Currency == "USD" || p.Currency == "USC") &&
                       p.ServiceId > 0)
            .GroupBy(p => p.ServiceId)
            .ToList();

        if (!accountGroups.Any())
        {
            logger.LogInformation("No USD/USC accounts found for MT5 query");
            return mysqlResults;
        }

        var reportDate = to.Date; // Extract the report date
        var isMultiDay = monthlyMt5Start.HasValue;

        // 基于MT5 Server 的时区查询Mysql, 区间为当天 00:00:00 到 23:59:59, 不用减去时区偏移
        DateTime mt5StartTime, mt5EndTime;
        if (isMultiDay)
        {
            mt5StartTime = new DateTime(monthlyMt5Start.Value.Year, monthlyMt5Start.Value.Month, monthlyMt5Start.Value.Day, 0, 0, 0, DateTimeKind.Utc);
            mt5EndTime = new DateTime(reportDate.Year, reportDate.Month, reportDate.Day, 23, 59, 59, DateTimeKind.Utc);
        }
        else
        {
            mt5StartTime = new DateTime(reportDate.Year, reportDate.Month, reportDate.Day, 0, 0, 0, DateTimeKind.Utc);
            mt5EndTime = new DateTime(reportDate.Year, reportDate.Month, reportDate.Day, 23, 59, 59, DateTimeKind.Utc);
        }
        
        // Convert to Unix timestamp
        var startTs = ((DateTimeOffset)mt5StartTime).ToUnixTimeSeconds();
        var endTs = ((DateTimeOffset)mt5EndTime).ToUnixTimeSeconds();

        // For mt5_deals, use DateTime string (assuming same GMT+0 storage)
        var dealsStart = mt5StartTime; //当天00:00:00
        var dealsEnd = mt5EndTime.AddSeconds(1); //Next day 00:00:00

        logger.LogInformation($"[DailyEquity MT5 Query] ReportDate={reportDate:yyyy-MM-dd}, MultiDay={isMultiDay}, MT5 Range: {mt5StartTime:yyyy-MM-dd HH:mm:ss} to {mt5EndTime:yyyy-MM-dd HH:mm:ss}, StartTs={startTs}, EndTs={endTs}, CurrentUTC={DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

        foreach (var serviceGroup in accountGroups)
        {
            var serviceId = serviceGroup.Key;

            try
            {
                // Get MT5 database context for this service
                using var mt5DbContext = myDbContextPool.CreateCentralMT5DbContextAsync(serviceId);

                // ⚠️ CRITICAL: Check if mt5_daily data is available for the report date
                // MT5 daily aggregation runs after trading day ends and may take 30-60 minutes
                // If the report runs too early, mt5_daily table will be empty for that date
                var mt5DailyAvailabilityCheck = await mt5DbContext.Mt5Dailys
                    .Where(d => d.Datetime >= startTs && d.Datetime <= endTs)
                    .AnyAsync();

                if (!mt5DailyAvailabilityCheck)
                {
                    logger.LogWarning($"[DailyEquity MT5] NO mt5_daily DATA AVAILABLE for ServiceId={serviceId}, ReportDate={reportDate:yyyy-MM-dd}. MT5 daily aggregation may not have completed yet. Report will show zero equity values. Current UTC: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}, Expected data timestamp range: {startTs} to {endTs}");
                    logger.LogWarning($"💡 [DailyEquity MT5] SOLUTION: Wait 1-2 hours after midnight (MT5 GMT+2) and regenerate the report, OR adjust job schedule to run later (e.g., 23:00 UTC instead of 21:10 UTC)");
                    // Continue processing but all equity values will be 0
                }
                else
                {
                    logger.LogInformation($"[DailyEquity MT5] mt5_daily data IS available for ServiceId={serviceId}, ReportDate={reportDate:yyyy-MM-dd}");
                }

                foreach (var pgResult in serviceGroup)
                {
                    if (string.IsNullOrEmpty(pgResult.AccountList))
                        continue;

                    // Parse account numbers from comma-separated string
                    var accountNumbers = pgResult.AccountList
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => ulong.TryParse(x.Trim(), out var num) ? num : 0)
                        .Where(x => x > 0)
                        .ToList();

                    if (!accountNumbers.Any())
                        continue;

                    logger.LogInformation($"[DailyEquity MySQL] Office={pgResult.Office}, Currency={pgResult.Currency}, ServiceId={serviceId}, Accounts={string.Join(",", accountNumbers)}, StartTs={startTs}, EndTs={endTs}");

                    // First, let's count how many records match our query
                    var recordCount = await mt5DbContext.Mt5Dailys
                        .Where(d => accountNumbers.Contains(d.Login))
                        .Where(d => d.Datetime >= startTs && d.Datetime <= endTs)
                        .CountAsync();
                    
                    logger.LogInformation($"[DailyEquity MySQL Record Count] Office={pgResult.Office}, Currency={pgResult.Currency}, RecordCount={recordCount}");

                    if (recordCount == 0)
                    {
                        logger.LogWarning($"⚠️ [DailyEquity MySQL] NO RECORDS found for Office={pgResult.Office}, Currency={pgResult.Currency}. Accounts={string.Join(",", accountNumbers)}, Timestamp range={startTs} to {endTs}. This will result in zero equity values.");
                    }

                    double previousEquity, currentEquity, dailyProfit;

                    if (isMultiDay)
                    {
                        // Monthly: find actual first/last trading days (skips weekends, holidays, future dates)
                        var firstTradingDayTs = await mt5DbContext.Mt5Dailys
                            .Where(d => accountNumbers.Contains(d.Login))
                            .Where(d => d.Datetime >= startTs && d.Datetime <= endTs)
                            .MinAsync(d => (long?)d.Datetime);

                        var lastTradingDayTs = await mt5DbContext.Mt5Dailys
                            .Where(d => accountNumbers.Contains(d.Login))
                            .Where(d => d.Datetime >= startTs && d.Datetime <= endTs)
                            .MaxAsync(d => (long?)d.Datetime);

                        if (firstTradingDayTs.HasValue && lastTradingDayTs.HasValue)
                        {
                            previousEquity = await mt5DbContext.Mt5Dailys
                                .Where(d => accountNumbers.Contains(d.Login))
                                .Where(d => d.Datetime == firstTradingDayTs.Value)
                                .SumAsync(d => d.EquityPrevDay);

                            currentEquity = await mt5DbContext.Mt5Dailys
                                .Where(d => accountNumbers.Contains(d.Login))
                                .Where(d => d.Datetime == lastTradingDayTs.Value)
                                .SumAsync(d => d.ProfitEquity);
                        }
                        else
                        {
                            previousEquity = 0;
                            currentEquity = 0;
                        }

                        dailyProfit = 0; // P&L is calculated by formula in MergeDailyEquityResults
                    }
                    else
                    {
                        // Query mt5_daily for Previous Equity, Current Equity, and P&L
                        // Based on requirement doc (daily_equity_report_mysql.md):
                        // - EquityPrevDay = 当日开盘净值 (Previous/Starting Equity)
                        // - ProfitEquity = 当日收盘净值 (Current/Ending Equity)
                        // - DailyProfit = 当日盈亏 (P&L)
                        // Daily: single day — sum across accounts
                        var dailyData = await mt5DbContext.Mt5Dailys
                            .Where(d => accountNumbers.Contains(d.Login))
                            .Where(d => d.Datetime >= startTs && d.Datetime <= endTs)
                            .GroupBy(d => 1) // Group all results together
                            .Select(g => new
                            {
                                PreviousEquity = g.Sum(d => d.EquityPrevDay),   // 开盘净值
                                CurrentEquity = g.Sum(d => d.ProfitEquity),     // 收盘净值
                                DailyProfit = g.Sum(d => d.DailyProfit)         // 盈亏
                            })
                            .FirstOrDefaultAsync();

                        previousEquity = dailyData?.PreviousEquity ?? 0;
                        currentEquity = dailyData?.CurrentEquity ?? 0;
                        dailyProfit = dailyData?.DailyProfit ?? 0;
                    }

                    // Query mt5_deals for Lots (closed trades only)
                    var lotsData = await mt5DbContext.Mt5Deals2025s
                        .Where(d => accountNumbers.Contains(d.Login))
                        .Where(d => d.Time >= dealsStart && d.Time < dealsEnd)
                        .Where(d => new uint[] { 1, 2, 3 }.Contains(d.Entry)) // Entry: 1=close, 2=reverse, 3=hedge close
                        .Where(d => new uint[] { 0, 1 }.Contains(d.Action)) // Action: 0=buy, 1=sell
                        .SumAsync(d => d.Volume / 10000.0); // Convert volume to lots

                    logger.LogInformation($"[DailyEquity MySQL Result] Office={pgResult.Office}, Currency={pgResult.Currency}, PrevEq(EquityPrevDay)={previousEquity}, CurrEq(ProfitEquity)={currentEquity}, DailyProfit={dailyProfit}, Lots={lotsData}");

                    mysqlResults.Add(new DailyEquityMysqlResult
                    {
                        Office = pgResult.Office,
                        Currency = pgResult.Currency,
                        PreviousEquity = (decimal)previousEquity,
                        CurrentEquity = (decimal)currentEquity,
                        Lots = (decimal)lotsData,
                        PL = (decimal)dailyProfit  // Use DailyProfit from MT5
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error querying MT5 database for ServiceId {ServiceId}", serviceId);
                // Continue processing other service groups even if one fails
            }
        }

        return mysqlResults;
    }

    private List<DailyEquityRecord> MergeDailyEquityResults(
        List<DailyEquityPostgresResult> postgresResults,
        List<DailyEquityMysqlResult> mysqlResults)
    {
        var allRecords = new List<DailyEquityRecord>();

        // Create a lookup for MySQL results
        var mysqlLookup = mysqlResults.ToDictionary(x => (x.Office, x.Currency));

        foreach (var pgResult in postgresResults)
        {
            var record = new DailyEquityRecord
            {
                Currency = pgResult.Currency,
                Office = pgResult.Office,
                NewUser = pgResult.Currency == "User" ? pgResult.NewAccUser : 0,
                NewAcc = pgResult.Currency != "User" ? pgResult.NewAccUser : 0,
                MarginIn = pgResult.MarginIn,
                MarginOut = -Math.Abs(pgResult.MarginOut), // Display as negative
                Credit = pgResult.Credit,
                Adjust = pgResult.Adjust,
                Rebate = pgResult.Rebate
            };

            // Calculate Transfer
            record.Transfer = pgResult.TransferIn - pgResult.TransferOut;

            // Handle currency-specific logic
            if (pgResult.Currency == "USD" || pgResult.Currency == "USC")
            {
                // For USD and USC, wait for MT5 data - don't use PostgreSQL equity values
                // MT5 provides the actual Previous/Current Equity from mt5_daily table
                
                // Get MT5 data from MySQL
                if (mysqlLookup.TryGetValue((pgResult.Office, pgResult.Currency), out var mysqlData))
                {
                    // Use MySQL data directly (before any currency conversion)
                    record.PreviousEquity = mysqlData.PreviousEquity;
                    record.CurrentEquity = mysqlData.CurrentEquity;
                    record.Lots = mysqlData.Lots;

                    // Calculate Net In/Out first (needed for P&L formula)
                    record.NetInOut = record.MarginIn + record.MarginOut + record.Transfer;
                    record.PL = record.CurrentEquity - record.PreviousEquity - record.NetInOut;
                }
                else
                {
                    // No MT5 data found - leave as 0
                    record.PreviousEquity = 0;
                    record.CurrentEquity = 0;
                    record.Lots = 0;
                    record.PL = 0;
                    record.NetInOut = record.MarginIn + record.MarginOut + record.Transfer;
                    logger.LogWarning($"[DailyEquity Merge] No MySQL data found for Office={pgResult.Office}, Currency={pgResult.Currency}");
                }

                // Calculate Estimates Net PL
                record.EstimatesNetPL = record.PL + record.Rebate;

                // USC CONVERSION REMOVED - Report expects USC in cents, not dollars
                // The database stores USC in cents (1 USC = 100 cents)
                // Previously we divided by 100 to convert to dollars, but the report format expects cents
                // So we keep the original values from MT5 (which are already in cents)
                
                // REMOVED: USC /100 conversion block
                // if (pgResult.Currency == "USC")
                // {
                //     record.PreviousEquity /= 100;
                //     record.CurrentEquity /= 100;
                //     record.MarginIn /= 100;
                //     record.MarginOut /= 100;
                //     record.Transfer /= 100;
                //     record.Rebate /= 100;
                //     record.PL /= 100;
                //     record.NetInOut /= 100;
                //     record.EstimatesNetPL /= 100;
                //     record.Lots /= 100;
                // }
            }
            else if (pgResult.Currency == "Wallet")
            {
                record.PreviousEquity = pgResult.PrevEquity;
                record.CurrentEquity = pgResult.CurrEquity;
                record.Lots = 0;
                record.PL = 0;
                record.NetInOut = record.MarginIn + record.MarginOut + record.Adjust + record.Rebate;
                record.EstimatesNetPL = 0;
            }
            else if (pgResult.Currency == "User")
            {
                // User row has minimal data
                record.PreviousEquity = 0;
                record.CurrentEquity = 0;
                record.Lots = 0;
                record.PL = 0;
                record.NetInOut = 0;
                record.EstimatesNetPL = 0;
            }

            allRecords.Add(record);
        }

        // Group records by Currency and add subtotal rows
        return GroupAndAddSubtotals(allRecords);
    }

    private List<DailyEquityRecord> GroupAndAddSubtotals(
        List<DailyEquityRecord> records, string[]? customCurrencyOrder = null,
        List<string>? officeOrder = null)
    {
        var finalRecords = new List<DailyEquityRecord>();
        
        // Extract User records to get New User counts by Office
        var userRecords = records.Where(r => r.Currency == "User").ToList();
        var newUserByOffice = userRecords.ToDictionary(r => r.Office, r => r.NewUser);
        
        var currencyOrder = customCurrencyOrder ?? new[] { "USD", "Wallet", "USC" };
        
        foreach (var currency in currencyOrder)
        {
            var currencyRecords = records.Where(r => r.Currency == currency).ToList();
            if (!currencyRecords.Any())
                continue;

            if (officeOrder is { Count: > 0 })
            {
                currencyRecords = currencyRecords
                    .OrderBy(r =>
                    {
                        var baseOffice = r.Office.Replace("-Wallet", "");
                        var idx = officeOrder.IndexOf(baseOffice);
                        return idx >= 0 ? idx : int.MaxValue;
                    })
                    .ToList();
            }

            // Merge New User data from User records into the first row of each office
            foreach (var record in currencyRecords)
            {
                if (newUserByOffice.TryGetValue(record.Office, out var newUserCount))
                {
                    record.NewUser = newUserCount;
                }
            }

            // Add all records for this currency
            finalRecords.AddRange(currencyRecords);

            // Calculate and add subtotal row
            // For Wallet and USD: round to 2 decimals (dollars)
            // For USC: keep full precision (cents - no rounding needed since MT5 already provides integer cents)
            var subtotal = new DailyEquityRecord
            {
                Currency = currency,
                Office = "Total",
                NewUser = currencyRecords.Sum(r => r.NewUser),
                NewAcc = currencyRecords.Sum(r => r.NewAcc),
                // For Wallet and USD: round each value to 2 decimals before summing
                // For USC: sum with full precision (values are already in cents)
                PreviousEquity = currency == "USC" 
                    ? currencyRecords.Sum(r => r.PreviousEquity)
                    : currencyRecords.Sum(r => Math.Round(r.PreviousEquity, 2)),
                CurrentEquity = currency == "USC" 
                    ? currencyRecords.Sum(r => r.CurrentEquity)
                    : currencyRecords.Sum(r => Math.Round(r.CurrentEquity, 2)),
                MarginIn = currency == "USC" 
                    ? currencyRecords.Sum(r => r.MarginIn)
                    : currencyRecords.Sum(r => Math.Round(r.MarginIn, 2)),
                MarginOut = currency == "USC" 
                    ? currencyRecords.Sum(r => r.MarginOut)
                    : currencyRecords.Sum(r => Math.Round(r.MarginOut, 2)),
                Transfer = currency == "USC" 
                    ? currencyRecords.Sum(r => r.Transfer)
                    : currencyRecords.Sum(r => Math.Round(r.Transfer, 2)),
                Credit = currency == "USC" 
                    ? currencyRecords.Sum(r => r.Credit)
                    : currencyRecords.Sum(r => Math.Round(r.Credit, 2)),
                Adjust = currency == "USC" 
                    ? currencyRecords.Sum(r => r.Adjust)
                    : currencyRecords.Sum(r => Math.Round(r.Adjust, 2)),
                Rebate = currency == "USC" 
                    ? currencyRecords.Sum(r => r.Rebate)
                    : currencyRecords.Sum(r => Math.Round(r.Rebate, 2)),
                NetInOut = currency == "USC" 
                    ? currencyRecords.Sum(r => r.NetInOut)
                    : currencyRecords.Sum(r => Math.Round(r.NetInOut, 2)),
                Lots = currency == "USC" 
                    ? currencyRecords.Sum(r => r.Lots)
                    : currencyRecords.Sum(r => Math.Round(r.Lots, 2)),
                PL = currency == "USC" 
                    ? currencyRecords.Sum(r => r.PL)
                    : currencyRecords.Sum(r => Math.Round(r.PL, 2)),
                EstimatesNetPL = currency == "USC" 
                    ? currencyRecords.Sum(r => r.EstimatesNetPL)
                    : currencyRecords.Sum(r => Math.Round(r.EstimatesNetPL, 2))
            };
            
            finalRecords.Add(subtotal);
            
            // Add a blank separator row (except after the last group)
            if (currency != currencyOrder.Last())
            {
                finalRecords.Add(new DailyEquityRecord 
                { 
                    Currency = "", 
                    Office = "" 
                });
            }
        }

        return finalRecords;
    }

    public List<DailyEquityPerOfficeRecord> AggregateByOffice(List<DailyEquityRecord> records)
    {
        var dataRecords = records
            .Where(r => !string.IsNullOrEmpty(r.Office) && r.Office != "Total")
            .ToList();

        var grouped = dataRecords.GroupBy(r => r.Office);

        var result = new List<DailyEquityPerOfficeRecord>();

        foreach (var group in grouped.OrderBy(g => g.Key))
        {
            var office = new DailyEquityPerOfficeRecord { Office = group.Key };

            foreach (var r in group)
            {
                decimal divisor = r.Currency == "USC" ? 100m : 1m;

                office.PreviousEquity += Math.Round(r.PreviousEquity / divisor, 2);
                office.CurrentEquity += Math.Round(r.CurrentEquity / divisor, 2);
                office.MarginIn += Math.Round(r.MarginIn / divisor, 2);
                office.MarginOut += Math.Round(r.MarginOut / divisor, 2);
                office.Transfer += Math.Round(r.Transfer / divisor, 2);
                office.Credit += Math.Round(r.Credit / divisor, 2);
                office.Adjust += Math.Round(r.Adjust / divisor, 2);
                office.Lots += Math.Round(r.Lots / divisor, 2);
                office.PL += Math.Round(r.PL / divisor, 2);
                office.EstimatesNetPL += Math.Round(r.EstimatesNetPL / divisor, 2);

                // Wallet Rebate is already counted in USD/USC, skip to avoid double-counting
                var rebate = r.Currency == "Wallet" ? 0m : r.Rebate;
                office.Rebate += Math.Round(rebate / divisor, 2);

                // Recalculate Wallet NetInOut without rebate for consistency
                var netInOut = r.Currency == "Wallet"
                    ? r.MarginIn + r.MarginOut + r.Adjust
                    : r.NetInOut;
                office.NetInOut += Math.Round(netInOut / divisor, 2);

                if (r.Currency == "User")
                    office.NewUser += r.NewUser;
                else if (r.Currency is "USD" or "USC")
                    office.NewAcc += r.NewAcc;
            }

            result.Add(office);
        }

        var total = new DailyEquityPerOfficeRecord
        {
            Office = "Total",
            NewUser = result.Sum(r => r.NewUser),
            NewAcc = result.Sum(r => r.NewAcc),
            PreviousEquity = result.Sum(r => r.PreviousEquity),
            CurrentEquity = result.Sum(r => r.CurrentEquity),
            MarginIn = result.Sum(r => r.MarginIn),
            MarginOut = result.Sum(r => r.MarginOut),
            Transfer = result.Sum(r => r.Transfer),
            Credit = result.Sum(r => r.Credit),
            Adjust = result.Sum(r => r.Adjust),
            Rebate = result.Sum(r => r.Rebate),
            NetInOut = result.Sum(r => r.NetInOut),
            Lots = result.Sum(r => r.Lots),
            PL = result.Sum(r => r.PL),
            EstimatesNetPL = result.Sum(r => r.EstimatesNetPL),
        };
        result.Add(total);

        return result;
    }

    private static string ResolveOffice(string salesCode, OfficeMergeMapping mapping)
    {
        if (string.IsNullOrEmpty(salesCode))
            return salesCode;

        foreach (var kvp in mapping.PrefixMappings.OrderByDescending(k => k.Key.Length))
        {
            if (salesCode.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }

        return salesCode;
    }

    public List<DailyEquityRecord> MergeRecordsByOfficeMapping(
        List<DailyEquityRecord> records, OfficeMergeMapping mapping)
    {
        var dataRecords = records
            .Where(r => !string.IsNullOrEmpty(r.Office) && r.Office != "Total"
                        && !string.IsNullOrEmpty(r.Currency))
            .ToList();

        var merged = dataRecords
            .GroupBy(r => new { Office = ResolveOffice(r.Office, mapping), r.Currency })
            .Select(g => new DailyEquityRecord
            {
                Office = g.Key.Office,
                Currency = g.Key.Currency,
                NewUser = g.Sum(r => r.NewUser),
                NewAcc = g.Sum(r => r.NewAcc),
                PreviousEquity = g.Sum(r => r.PreviousEquity),
                CurrentEquity = g.Sum(r => r.CurrentEquity),
                MarginIn = g.Sum(r => r.MarginIn),
                MarginOut = g.Sum(r => r.MarginOut),
                Transfer = g.Sum(r => r.Transfer),
                Credit = g.Sum(r => r.Credit),
                Adjust = g.Sum(r => r.Adjust),
                Rebate = g.Sum(r => r.Rebate),
                NetInOut = g.Sum(r => r.NetInOut),
                Lots = g.Sum(r => r.Lots),
                PL = g.Sum(r => r.PL),
                EstimatesNetPL = g.Sum(r => r.EstimatesNetPL)
            })
            .ToList();

        // Wallet Rebate is already counted in USD/USC sections, zero it out to avoid
        // double counting in the grand total (follows finance team convention).
        foreach (var r in merged.Where(r => r.Currency == "Wallet"))
        {
            r.Rebate = 0;
            r.NetInOut = r.MarginIn + r.MarginOut + r.Adjust;
        }

        foreach (var staticOffice in mapping.StaticOffices)
        {
            foreach (var currency in new[] { "USD", "USC", "Wallet", "User" })
            {
                if (!merged.Any(r => r.Office == staticOffice && r.Currency == currency))
                {
                    merged.Add(new DailyEquityRecord
                    {
                        Office = staticOffice,
                        Currency = currency
                    });
                }
            }
        }

        return GroupAndAddSubtotals(merged, new[] { "USD", "USC", "Wallet" },
            mapping.OfficeOrder is { Count: > 0 } ? mapping.OfficeOrder : null);
    }

    /// <summary>
    /// 从DailyEquitySnapshot表中聚合数据生成月度报告,这样保证月度数据 = 每日数据之和, 避免了CTE中AND条件边界导致的统计口径问题.
    /// Build a monthly report by aggregating stored daily snapshots instead of re-running the CTE.
    /// This guarantees monthly values = sum of daily values (no AND-condition boundary gaps).
    /// </summary>
    public async Task<List<DailyEquityRecord>> AggregateSnapshotsForMonthlyAsync(
        DateTime from, DateTime to, EquityReportVersion reportVersion)
    {
        var fromDate = from.Date;
        var toDate = to.Date;

        var snapshots = await tenantDbContext.DailyEquitySnapshots
            .Where(s => s.ReportDate >= fromDate && s.ReportDate <= toDate
                        && s.ReportVersion == reportVersion)
            .OrderBy(s => s.ReportDate)
            .ToListAsync();

        if (!snapshots.Any())
        {
            logger.LogWarning(
                "[MonthlyFromSnapshots] No snapshots found for {From} to {To} v{Version}",
                fromDate, toDate, reportVersion);
            return new List<DailyEquityRecord>();
        }

        var firstDate = snapshots.Min(s => s.ReportDate);
        var lastDate = snapshots.Max(s => s.ReportDate);
        var snapshotDays = snapshots.Select(s => s.ReportDate).Distinct().Count();

        logger.LogInformation(
            "[MonthlyFromSnapshots] Aggregating {Days} days of snapshots ({First} to {Last}) for v{Version}",
            snapshotDays, firstDate, lastDate, reportVersion);

        var rawRecords = snapshots
            .GroupBy(s => new { s.Office, s.Currency })
            .Select(g => new DailyEquityRecord
            {
                Office = g.Key.Office,
                Currency = g.Key.Currency,
                NewUser = g.Sum(s => s.NewUser),
                NewAcc = g.Sum(s => s.NewAcc),
                PreviousEquity = g.Where(s => s.ReportDate == firstDate).Sum(s => s.PreviousEquity),
                CurrentEquity = g.Where(s => s.ReportDate == lastDate).Sum(s => s.CurrentEquity),
                MarginIn = g.Sum(s => s.MarginIn),
                MarginOut = g.Sum(s => s.MarginOut),
                Transfer = g.Sum(s => s.Transfer),
                Credit = g.Sum(s => s.Credit),
                Adjust = g.Sum(s => s.Adjust),
                Rebate = g.Sum(s => s.Rebate),
                NetInOut = g.Sum(s => s.NetInOut),
                Lots = g.Sum(s => s.Lots),
                PL = g.Sum(s => s.PL),
                EstimatesNetPL = g.Sum(s => s.EstimatesNetPL)
            })
            .ToList();

        return GroupAndAddSubtotals(rawRecords);
    }

    /// <summary>
    /// Persist daily equity records to DailyEquitySnapshot table.
    /// Uses delete-then-insert (upsert) keyed on ReportDate + ReportVersion.
    /// </summary>
    public async Task SaveDailyEquitySnapshotAsync(
        List<DailyEquityRecord> records,
        DateTime reportDate,
        EquityReportVersion reportVersion)
    {
        var dataRecords = records
            .Where(r => !string.IsNullOrEmpty(r.Office)
                        && r.Office != "Total"
                        && !string.IsNullOrEmpty(r.Currency))
            .ToList();

        if (!dataRecords.Any())
        {
            logger.LogWarning("[DailyEquitySnapshot] No data records to save for {ReportDate}", reportDate);
            return;
        }

        var dateOnly = reportDate.Date;

        var existing = await tenantDbContext.DailyEquitySnapshots
            .Where(s => s.ReportDate == dateOnly && s.ReportVersion == reportVersion)
            .ToListAsync();

        if (existing.Any())
        {
            tenantDbContext.DailyEquitySnapshots.RemoveRange(existing);
            logger.LogInformation(
                "[DailyEquitySnapshot] Removed {Count} existing snapshots for {ReportDate} v{Version}",
                existing.Count, dateOnly, reportVersion);
        }

        var now = DateTime.UtcNow;
        var snapshots = dataRecords.Select(r => new DailyEquitySnapshot
        {
            ReportDate = dateOnly,
            ReportVersion = reportVersion,
            Office = r.Office,
            Currency = r.Currency,
            NewUser = r.NewUser,
            NewAcc = r.NewAcc,
            PreviousEquity = r.PreviousEquity,
            CurrentEquity = r.CurrentEquity,
            MarginIn = r.MarginIn,
            MarginOut = r.MarginOut,
            Transfer = r.Transfer,
            Credit = r.Credit,
            Adjust = r.Adjust,
            Rebate = r.Rebate,
            NetInOut = r.NetInOut,
            Lots = r.Lots,
            PL = r.PL,
            EstimatesNetPL = r.EstimatesNetPL,
            CreatedOn = now
        }).ToList();

        tenantDbContext.DailyEquitySnapshots.AddRange(snapshots);
        await tenantDbContext.SaveChangesAsync();

        logger.LogInformation(
            "[DailyEquitySnapshot] Saved {Count} snapshots for {ReportDate} v{Version}",
            snapshots.Count, dateOnly, reportVersion);
    }
}

