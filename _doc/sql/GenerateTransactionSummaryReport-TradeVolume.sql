-- Account Trade Transaction Summary Report
insert into rpt."_TransactionSummaryReport"
("PartyId", "RowId", "CurrencyId", "Type", "PeriodType", "DateTime", "TotalTransaction", "TotalValue", "MinValue",
 "MaxValue")
select
    a."PartyId" as "PartyId",
    a."Id" as "RowId",
    ta."CurrencyId",
    110                              AS "Type",       -- 100: TradeVolume; 110: TradeVolumeForAgent; 500: rebate
    1                               AS "PeriodType", -- 1: hourly; 2: daily,
--     "CloseAt"::date::timestamp      AS "DateTime",   -- Daily
    date_trunc('hour', "CloseAt")   AS "DateTime",
    count(*)                        AS "TotalTransaction",
    SUM(ROUND(tt."Volume")::BIGINT) AS "TotalValue",
    MIN(ROUND(tt."Volume")::BIGINT) AS "MinValue",
    MAX(ROUND(tt."Volume")::BIGINT) AS "MaxValue"
from trd."_TradeTransaction" tt
         join trd."_TradeAccount" ta on tt."TradeAccountId" = ta."Id"
         join trd."_Account" a on a."Id" = ta."Id"
where tt."CloseAt" >= '2000-01-01'
  and tt."CloseAt" < date_trunc('hour', now())
  and tt."CMD" >= 0
  and tt."CMD" <= 5
group by "PartyId", a."Id", "CurrencyId", "DateTime"
ON CONFLICT ("PartyId", "RowId", "CurrencyId", "Type", "PeriodType", "DateTime")
    DO NOTHING
    -- DO UPDATE
    -- SET
    --     "TotalTransaction" = EXCLUDED."TotalTransaction",
    --     "TotalValue" = EXCLUDED."TotalValue",
    --     "MinValue" = EXCLUDED."MinValue",
    --     "MaxValue" =EXCLUDED."MaxValue"
    ;
