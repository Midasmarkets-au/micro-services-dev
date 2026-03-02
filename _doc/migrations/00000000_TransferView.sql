create or replace view acct."TransferView"
            ("Id", "Type", "StateId", "PostedOn", "Amount", "CurrencyId", "PartyId", "LedgerSide", "FundType",
             "SourceAccountType", "SourceAccountId", "TargetAccountType", "TargetAccountId", "PrevBalance")
as
SELECT m."Id",
       m."Type",
       m."StateId",
       m."PostedOn",
       tr."Amount",
       tr."CurrencyId",
       tr."PartyId",
       tr."LedgerSide",
       tr."FundType",
       tr."SourceAccountType",
       tr."SourceAccountId",
       tr."TargetAccountType",
       tr."TargetAccountId",
       w."PrevBalance"
FROM core."_Matter" m
         LEFT JOIN (SELECT "_Deposit"."Id",
                           "_Deposit"."Amount",
                           "_Deposit"."CurrencyId",
                           "_Deposit"."PartyId",
                           "_Deposit"."FundType",
                           1 AS "LedgerSide",
                           0 AS "SourceAccountType",
                           0 AS "SourceAccountId",
                           1 AS "TargetAccountType",
                           0 AS "TargetAccountId"
                    FROM acct."_Deposit"
                    UNION ALL
                    SELECT "_Withdrawal"."Id",
                           "_Withdrawal"."Amount",
                           "_Withdrawal"."CurrencyId",
                           "_Withdrawal"."PartyId",
                           "_Withdrawal"."FundType",
                           2 AS "LedgerSide",
                           1 AS "SourceAccountType",
                           0 AS "SourceAccountId",
                           0 AS "TargetAccountType",
                           0 AS "TargetAccountId"
                    FROM acct."_Withdrawal"
                    UNION ALL
                    SELECT "_Transaction"."Id",
                           "_Transaction"."Amount",
                           "_Transaction"."CurrencyId",
                           "_Transaction"."PartyId",
                           "_Transaction"."FundType",
                           2 AS "LedgerSide",
                           "_Transaction"."SourceAccountType",
                           "_Transaction"."SourceAccountId",
                           "_Transaction"."TargetAccountType",
                           "_Transaction"."TargetAccountId"
                    FROM acct."_Transaction"
                    WHERE "_Transaction"."SourceAccountType" = 1
                    UNION ALL
                    SELECT "_Transaction"."Id",
                           "_Transaction"."Amount",
                           "_Transaction"."CurrencyId",
                           "_Transaction"."PartyId",
                           "_Transaction"."FundType",
                           1 AS "LedgerSide",
                           "_Transaction"."SourceAccountType",
                           "_Transaction"."SourceAccountId",
                           "_Transaction"."TargetAccountType",
                           "_Transaction"."TargetAccountId"
                    FROM acct."_Transaction"
                    WHERE "_Transaction"."TargetAccountType" = 1
                    UNION ALL
                    SELECT "_Rebate"."Id",
                           "_Rebate"."Amount",
                           "_Rebate"."CurrencyId",
                           "_Rebate"."PartyId",
                           "_Rebate"."FundType",
                           1 AS "LedgerSide",
                           0 AS "SourceAccountType",
                           0 AS "SourceAccountId",
                           1 AS "TargetAccountType",
                           0 AS "TargetAccountId"
                    FROM trd."_Rebate") tr ON m."Id" = tr."Id"
         LEFT JOIN acct."_WalletTransaction" w ON m."Id" = w."MatterId"
WHERE (m."Type" = ANY (ARRAY [200, 300, 400, 500]))
  AND tr."Id" IS NOT NULL;


