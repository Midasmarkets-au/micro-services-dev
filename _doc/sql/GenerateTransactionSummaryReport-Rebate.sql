-- Party Rebate Summary Report
insert into rpt."_TransactionSummaryReport"
("PartyId", "RowId", "CurrencyId", "Type", "PeriodType", "DateTime", "TotalTransaction", "TotalValue", "MinValue",
 "MaxValue")
select r."PartyId",
       r."Id",
       r."CurrencyId",
       500                              AS "Type",       -- 10: TradeVolume; 20: ClientTradeVolumeForAgent; 500: rebate
       1                                AS "PeriodType", -- 1: hourly; 2: daily,
--     "CloseAt"::date::timestamp      AS "DateTime",   -- Daily
       date_trunc('hour', m."PostedOn") AS "CreatedOn",
       count(*)                         AS "TotalTransaction",
       SUM(r."Amount")                  AS "TotalValue",
       MIN(r."Amount")                  AS "MinValue",
       MAX(r."Amount")                  AS "MaxValue"
from trd."_Rebate" r
         join core."_Matter" m on r."Id" = m."Id"
where m."PostedOn" >= '2000-01-01'
  and m."PostedOn" < '2024-05-01'
  and m."StateId" = 550
group by "PartyId", r."Id", "CurrencyId", "PostedOn"
ON CONFLICT ("PartyId", "RowId", "CurrencyId", "Type", "PeriodType", "DateTime")
    DO NOTHING;
