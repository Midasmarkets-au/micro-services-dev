-- Party Rebate Summary Report
-- 100: TradeVolume;
-- 110: TradeVolumeForAgent;
-- 200: Deposit;
-- 210: DepositForAgent
-- 500: rebate
insert into rpt."_TransactionSummaryReport"
("PartyId", "RowId", "CurrencyId", "Type", "PeriodType", "DateTime", "TotalTransaction", "TotalValue", "MinValue",
 "MaxValue")
select agent."Id"                       AS "PartyId",    -- report owner party id
       agent."Id"                       AS "RowId",      -- report entity id
       d."CurrencyId",
       210                              AS "Type",       -- 100: TradeVolume; 110: TradeVolumeForAgent; 200: Deposit; 210: DepositForAgent 500: rebate
       1                                AS "PeriodType", -- 1: hourly; 2: daily, always Hourly for now
--     "CloseAt"::date::timestamp      AS "DateTime",   -- Daily
       date_trunc('hour', m."PostedOn") AS "CreatedOn",
       count(*)                         AS "TotalTransaction",
       SUM(d."Amount")                  AS "TotalValue",
       MIN(d."Amount")                  AS "MinValue",
       MAX(d."Amount")                  AS "MaxValue"
from acct."_Deposit" d
         join core."_Matter" m on d."Id" = m."Id"
         join core."_Party" p on d."PartyId" = p."Id"
         join core."_Party" agent on p."Pid" = agent."Id"
where m."PostedOn" >= '2000-01-01'
  and m."PostedOn" < '2024-05-01'
  and m."StateId" = 250
group by agent."Id", agent."Id", "CurrencyId", "PostedOn"
ON CONFLICT ("PartyId", "RowId", "CurrencyId", "Type", "PeriodType", "DateTime")
    DO NOTHING;
