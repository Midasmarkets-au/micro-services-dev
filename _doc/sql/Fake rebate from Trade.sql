with source as (select a."PartyId"                                    as "PartyId",
                       "TradeAccountId",
                       t."Id"                                         as "TradeId",
                       840                                            as "CurrencyId",
                       cast((round("Volume" * 0.05 * 100)) as bigint) as "Amount",
                       "OpenAt"                                       as "OpenAt",
                       "CloseAt"                                      as "CloseAt",
                       '{}'                                           as "Info",
                from trd."_TradeTransaction" t
                         join trd."_Account" a on a."Id" = t."TradeAccountId"
                where "CMD" >= 0
                  and "CMD" <= 5
                limit 100),
     rebate as (
         insert into trd."_Rebate" ("Id", "PartyId", "AccountId", "TradeId", "CurrencyId", "Amount", "HoldUntilOn",
                                    "Information")
             select insert_matter(500, 550, "OpenAt", "CloseAt"),
                    source."PartyId",
                    source."TradeAccountId",
                    source."TradeId",
                    source."CurrencyId",
                    source."Amount",
                    now(),
                    source."Info"
             from source
             returning *)
SELECT *
from rebate;

CREATE OR REPLACE FUNCTION insert_matter(matter_type INT, state_id INT, posted_on TIMESTAMP, stated_on TIMESTAMP)
RETURNS BIGINT AS $$
DECLARE
    new_id BIGINT;
BEGIN
    INSERT INTO core."_Matter" ("Type", "StateId", "PostedOn", "StatedOn")
    VALUES (matter_type, state_id, posted_on, stated_on)
    RETURNING "Id" INTO new_id;

    RETURN new_id;
END;
$$ LANGUAGE plpgsql;