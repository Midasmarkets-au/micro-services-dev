create view "AccountDeposit"
            ("DepositId", "FundType", "CurrencyId", "Amount", "PostedOn", "StatedOn", "AccountId", "PartyId", "Uid",
             "AccountNumber", "Level", "ReferPath", "Type", "AgentAccountId", "SalesAccountId", "Group")
as
SELECT d."Id" AS "DepositId",
       d."FundType",
       d."CurrencyId",
       d."Amount",
       m."PostedOn",
       m."StatedOn",
       a."Id" AS "AccountId",
       a."PartyId",
       a."Uid",
       a."AccountNumber",
       a."Level",
       a."ReferPath",
       a."Type",
       a."AgentAccountId",
       a."SalesAccountId",
       a."Group"
FROM acct."_Deposit" d
         LEFT JOIN core."_Matter" m ON m."Id" = d."Id"
         LEFT JOIN trd."_Account" a ON a."Id" = d."TargetAccountId"
WHERE m."StateId" = 350;

alter table "AccountDeposit"
    owner to bcrpro;

create view "AccountRebate"
            ("AccountId", "Amount", "CurrencyId", "FundType", "StatedOn", "PostedOn", "TradeAccountId", "AccountNumber",
             "Ticket", "Symbol", "Volume", "Profit", "SalesAccountId", "AgentAccountId", "Group", "Type", "SiteId",
             "Level", "Uid", "ReferPath", "StateId", "Id")
as
SELECT r."AccountId",
       r."Amount",
       r."CurrencyId",
       r."FundType",
       m."StatedOn",
       m."PostedOn",
       tr."AccountId" AS "TradeAccountId",
       tr."AccountNumber",
       tr."Ticket",
       tr."Symbol",
       tr."Volume",
       tr."Profit",
       ta."SalesAccountId",
       ta."AgentAccountId",
       ta."Group",
       ta."Type",
       ta."SiteId",
       ta."Level",
       ta."Uid",
       ta."ReferPath",
       m."StateId",
       r."Id"
FROM trd."_Rebate" r
         LEFT JOIN core."_Matter" m ON r."Id" = m."Id"
         LEFT JOIN trd."_TradeRebate" tr ON r."TradeRebateId" = tr."Id"
         LEFT JOIN trd."_Account" ta ON ta."Id" = tr."AccountId";

alter table "AccountRebate"
    owner to bcrpro;

create view "AccountWithdrawal"
            ("WithdrawalId", "FundType", "CurrencyId", "Amount", "PostedOn", "StatedOn", "AccountId", "PartyId", "Uid",
             "AccountNumber", "Level", "ReferPath", "Type", "AgentAccountId", "SalesAccountId", "Group", "ApprovedOn")
as
SELECT d."Id" AS "WithdrawalId",
       d."FundType",
       d."CurrencyId",
       d."Amount",
       m."PostedOn",
       m."StatedOn",
       a."Id" AS "AccountId",
       a."PartyId",
       a."Uid",
       a."AccountNumber",
       a."Level",
       a."ReferPath",
       a."Type",
       a."AgentAccountId",
       a."SalesAccountId",
       a."Group",
       d."ApprovedOn"
FROM acct."_Withdrawal" d
         LEFT JOIN core."_Matter" m ON m."Id" = d."Id"
         LEFT JOIN trd."_Account" a ON a."Id" = d."SourceAccountId"
WHERE m."StateId" = 450;

alter table "AccountWithdrawal"
    owner to bcrpro;

create view "AlllWithdrawal"
            ("WithdrawalId", "FundType", "CurrencyId", "Amount", "PostedOn", "StatedOn", "WithdrawPartId", "AccountId",
             "PartyId", "Uid", "AccountNumber", "Level", "ReferPath", "Type", "AgentAccountId", "SalesAccountId",
             "Group", "ApprovedOn")
as
SELECT d."Id"      AS "WithdrawalId",
       d."FundType",
       d."CurrencyId",
       d."Amount",
       m."PostedOn",
       m."StatedOn",
       d."PartyId" AS "WithdrawPartId",
       a."Id"      AS "AccountId",
       a."PartyId",
       a."Uid",
       a."AccountNumber",
       a."Level",
       a."ReferPath",
       a."Type",
       a."AgentAccountId",
       a."SalesAccountId",
       a."Group",
       d."ApprovedOn"
FROM acct."_Withdrawal" d
         LEFT JOIN core."_Matter" m ON m."Id" = d."Id"
         LEFT JOIN trd."_Account" a ON a."Id" = d."SourceAccountId"
WHERE m."StateId" = 450;

alter table "AlllWithdrawal"
    owner to bcrpro;

create view "DepositView"
            ("Id", "PartyId", "Amount", "StateId", "StateName", "CurrencyId", "FundType", "TargetAccountId",
             "TargetAccountNumber", "PaymentId", "PaymentName", "PaymentPlatform", "AutoDeposit", "PaymentNumber",
             "PaymentStatus", "CreatedOn", "UpdatedOn")
as
SELECT d."Id",
       d."PartyId",
       d."Amount",
       m."StateId",
       s."Name"                  AS "StateName",
       d."CurrencyId",
       d."FundType",
       d."TargetAccountId",
       a."AccountNumber"         AS "TargetAccountNumber",
       d."PaymentId",
       ps."Name"                 AS "PaymentName",
       ps."Platform"             AS "PaymentPlatform",
       ps."IsAutoDepositEnabled" AS "AutoDeposit",
       p."Number"                AS "PaymentNumber",
       p."Status"                AS "PaymentStatus",
       m."PostedOn"              AS "CreatedOn",
       m."StatedOn"              AS "UpdatedOn"
FROM acct."_Deposit" d
         JOIN core."_Matter" m ON d."Id" = m."Id"
         JOIN acct."_Payment" p ON d."PaymentId" = p."Id"
         JOIN acct."_PaymentService" ps ON ps."Id" = p."PaymentServiceId"
         JOIN core."_State" s ON s."Id" = m."StateId"
         LEFT JOIN trd."_Account" a ON d."TargetAccountId" = a."Id";

alter table "DepositView"
    owner to bcrpro;

create view "TransferView"
            ("Id", "Type", "StateId", "PostedOn", "StatedOn", "Amount", "CurrencyId", "PartyId", "LedgerSide",
             "FundType", "SourceAccountType", "SourceAccountId", "TargetAccountType", "TargetAccountId", "PrevBalance")
as
SELECT m."Id",
       m."Type",
       m."StateId",
       m."PostedOn",
       m."StatedOn",
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

alter table "TransferView"
    owner to bcrpro;

grant select on "TransferView" to dev_read;

create view "WalletAdjustView"("WalletAdjustId", "SourceType", "Comment", "Amount", "CreatedOn", "PartyId", "WalletId") as
SELECT wa."Id" AS "WalletAdjustId",
       wa."SourceType",
       wa."Comment",
       wa."Amount",
       wa."CreatedOn",
       w."PartyId",
       wa."WalletId"
FROM acct."_WalletAdjust" wa
         LEFT JOIN core."_Matter" m ON m."Id" = wa."Id"
         LEFT JOIN acct."_Wallet" w ON w."Id" = wa."WalletId"
WHERE m."StateId" = 750;

alter table "WalletAdjustView"
    owner to bcrpro;

create view "WalletWithdrawal"
            ("WithdrawalId", "FundType", "CurrencyId", "Amount", "PostedOn", "StatedOn", "WithdrawPartyId", "ApprovedOn", "SourceWalletId") as
SELECT d."Id"      AS "WithdrawalId",
       d."FundType",
       d."CurrencyId",
       d."Amount",
       m."PostedOn",
       m."StatedOn",
       d."PartyId" AS "WithdrawPartyId",
       d."ApprovedOn",
       d."SourceWalletId"
FROM acct."_Withdrawal" d
         LEFT JOIN core."_Matter" m ON m."Id" = d."Id"
WHERE m."StateId" = 450
  AND d."SourceWalletId" IS NOT NULL;

alter table "WalletWithdrawal"
    owner to bcrpro;

create view "AccountRebate"
            ("AccountId", "Amount", "CurrencyId", "FundType", "StatedOn", "PostedOn", "TradeAccountId", "AccountNumber",
             "Ticket", "Symbol", "Volume", "Profit", "SalesAccountId", "AgentAccountId", "Group", "Type", "SiteId",
             "Level", "Uid", "ReferPath", "StateId", "Id")
as
SELECT r."AccountId",
       r."Amount",
       r."CurrencyId",
       r."FundType",
       m."StatedOn",
       m."PostedOn",
       tr."AccountId" AS "TradeAccountId",
       tr."AccountNumber",
       tr."Ticket",
       tr."Symbol",
       tr."Volume",
       tr."Profit",
       ta."SalesAccountId",
       ta."AgentAccountId",
       ta."Group",
       ta."Type",
       ta."SiteId",
       ta."Level",
       ta."Uid",
       ta."ReferPath",
       m."StateId",
       r."Id"
FROM trd."_Rebate" r
         LEFT JOIN core."_Matter" m ON r."Id" = m."Id"
         LEFT JOIN trd."_TradeRebate" tr ON r."TradeRebateId" = tr."Id"
         LEFT JOIN trd."_Account" ta ON ta."Id" = tr."AccountId";

alter table "AccountRebate"
    owner to bcrpro;

create view "AccountTrade"
            ("AccountId", "ReferPath", "CurrencyId", "Symbol", "Volume", "Amount", "OpenedOn", "ClosedOn") as
SELECT tr."AccountId",
       a."ReferPath",
       tr."CurrencyId",
       tr."Symbol",
       tr."Volume",
       floor(tr."Profit" * 100::double precision)::bigint AS "Amount",
       tr."OpenedOn",
       tr."ClosedOn"
FROM trd."_TradeRebate" tr
         JOIN trd."_Account" a ON tr."AccountId" = a."Id"
WHERE tr."AccountId" IS NOT NULL
  AND a."IsClosed" = 0;

alter table "AccountTrade"
    owner to bcrpro;

grant select on "AccountTrade" to dev_read;

create view "RebateToWallet"
            ("Id", "PartyId", "AccountId", "TransFundType", "CurrencyId", "RebateAmount", "TradeRebateId",
             "Information", "PostedOn", "MatterName", "StateName", "WalletId", "TransAmount", "PrevBalance",
             "WalletFundType", "WalletCurrencyId", "Balance", "TradeAccountCurrencyId", "TradeAccountFundType",
             "ReciverCurrencyId", "ReciverFundType")
as
SELECT r."Id",
       r."PartyId",
       r."AccountId",
       r."FundType"    AS "TransFundType",
       r."CurrencyId",
       r."Amount"      AS "RebateAmount",
       r."TradeRebateId",
       r."Information",
       m."PostedOn",
       mt."Name"       AS "MatterName",
       s."Name"        AS "StateName",
       wt."WalletId",
       wt."Amount"     AS "TransAmount",
       wt."PrevBalance",
       w."FundType"    AS "WalletFundType",
       w."CurrencyId"  AS "WalletCurrencyId",
       w."Balance",
       a."CurrencyId"  AS "TradeAccountCurrencyId",
       a."FundType"    AS "TradeAccountFundType",
       ra."CurrencyId" AS "ReciverCurrencyId",
       ra."FundType"   AS "ReciverFundType"
FROM trd."_Rebate" r
         LEFT JOIN core."_Matter" m ON m."Id" = r."Id"
         LEFT JOIN core."_MatterType" mt ON mt."Id" = m."Type"
         LEFT JOIN core."_State" s ON s."Id" = m."StateId"
         LEFT JOIN acct."_WalletTransaction" wt ON wt."MatterId" = r."Id"
         LEFT JOIN acct."_Wallet" w ON w."Id" = wt."WalletId"
         LEFT JOIN trd."_TradeRebate" tr ON tr."Id" = r."TradeRebateId"
         LEFT JOIN trd."_Account" a ON a."Id" = tr."AccountId"
         LEFT JOIN trd."_Account" ra ON ra."Id" = r."AccountId";

alter table "RebateToWallet"
    owner to bcrpro;

grant select on "RebateToWallet" to dev_read;

