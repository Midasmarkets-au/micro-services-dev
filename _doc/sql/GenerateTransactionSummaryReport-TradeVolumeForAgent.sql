-- IB Account Trade Transaction Summary Report
INSERT INTO rpt."_TransactionSummaryReport"
("PartyId", "RowId", "CurrencyId", "Type", "PeriodType", "DateTime", "TotalTransaction", "TotalValue", "MinValue",
 "MaxValue")
SELECT agent."PartyId",
       agent."Id",
       ta."CurrencyId",
       110                              AS "Type",       -- 100: TradeVolume; 110: ClientTradeVolumeForAgent; 500: rebate
       1                               AS "PeriodType", -- 1: hourly; 2: daily,
--     "CloseAt"::date::timestamp      AS "DateTime",   -- Daily
       DATE_TRUNC('hour', "CloseAt")   AS "DateTime",  -- Hourly
       COUNT(*)                        AS "TotalTransaction",
       SUM(ROUND(tt."Volume")::BIGINT) AS "TotalValue",
       MIN(ROUND(tt."Volume")::BIGINT) AS "MinValue",
       MAX(ROUND(tt."Volume")::BIGINT) AS "MaxValue"
FROM trd."_TradeTransaction" tt
       JOIN trd."_TradeAccount" ta ON tt."TradeAccountId" = ta."Id"
       JOIN trd."_Account" a ON a."Id" = ta."Id"
       JOIN trd."_Account" agent ON a."AgentAccountId" = agent."Id"
WHERE tt."CloseAt" >= '2000-01-01'
  AND tt."CloseAt" < '2024-05-01'
  AND tt."CMD" >= 0
  AND tt."CMD" <= 5
--   and a."AgentAccountId" is not null
--   and a."PartyId" = 15237
GROUP BY agent."PartyId", agent."Id", "CurrencyId", "DateTime"
ON CONFLICT ("PartyId", "RowId", "CurrencyId", "Type", "PeriodType", "DateTime")
    DO NOTHING;