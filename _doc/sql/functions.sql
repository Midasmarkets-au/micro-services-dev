create function public.get_all_child_accounts(accountid bigint)
    returns TABLE("Id" bigint, "AgentAccountId" bigint, "Uid" bigint, "PartyId" bigint, "Role" integer, "Type" integer, "Depth" integer)
    language sql
as
$$
WITH RECURSIVE all_children AS (
  -- Base case: Select the account with the provided Id
  SELECT *, 1 as "Depth"
  FROM trd."_Account"
  WHERE "AgentAccountId" = $1

  UNION ALL

  -- Recursive case: Select child accounts
  SELECT child.*, parent."Depth" + 1 as depth
  FROM trd."_Account" AS child
  JOIN all_children AS parent ON child."AgentAccountId" = parent."Id"
  WHERE parent."Depth" < 50 -- Adjust the maximum depth according to your requirements
)
SELECT "Id", "AgentAccountId", "Uid", "PartyId", "Role", "Type", "Depth"
FROM all_children;
$$;

create function public.get_all_parent_accounts(accountid bigint)
    returns TABLE("Id" bigint, "AgentAccountId" bigint, "Uid" bigint, "PartyId" bigint, "Role" integer, "Type" integer, "Depth" integer)
    language sql
as
$$
WITH RECURSIVE all_parents AS (
  -- Base case: Select the account with the provided Id
  SELECT *, 1 as "Depth"
  FROM trd."_Account"
  WHERE "Id" = $1

  UNION ALL

  -- Recursive case: Select parent accounts
  SELECT parent.*, child."Depth" + 1 as depth
  FROM trd."_Account" AS parent
  JOIN all_parents AS child ON parent."Id" = child."AgentAccountId"
  WHERE child."Depth" < 50 -- Adjust the maximum depth according to your requirements
)
SELECT "Id", "AgentAccountId", "Uid", "PartyId", "Role", "Type", "Depth"
FROM all_parents;
$$;

create function public.get_root_agent_account(accountid bigint)
    returns TABLE("Id" bigint, "AgentAccountId" bigint, "Uid" bigint, "PartyId" bigint, "Role" integer, "Type" integer, "Depth" integer)
    language sql
as
$$
WITH RECURSIVE all_parents AS (
  -- Base case: Select the account with the provided Id
  SELECT *, 1 as "Depth"
  FROM trd."_Account"
  WHERE "Id" = $1

  UNION ALL

  -- Recursive case: Select parent accounts
  SELECT parent.*, child."Depth" + 1 as "Depth"
  FROM trd."_Account" AS parent
  JOIN all_parents AS child ON parent."Id" = child."AgentAccountId"
  WHERE child."Depth" < 10 -- Adjust the maximum depth according to your requirements
)
SELECT "Id", "AgentAccountId", "Uid", "PartyId", "Role", "Type", "Depth"
FROM all_parents
ORDER BY "Depth" DESC
LIMIT  1;
$$;

create function public.generate_trade_rebate_from_trade() returns integer
    language plpgsql
as
$$
DECLARE
    updated_records INTEGER;
BEGIN
    with inserted_tr_id as (

        insert into trd."_TradeRebate" ("TradeId", "RuleId", "SymbolCategoryId", "CurrencyId", "Amount", "Status")
            select
-- count(*)
                t."Id"    as "TradeId"
                 , 0      as "RuleId"
                 , c."Id" as "SymbolId"
                 , ta."CurrencyId"
                 , 0      as "Amount"
                 , 0      as "Status"
-- ,   t."CloseAt"
            from trd."_TradeTransaction" t
                     join trd."_Symbol" s on t."Symbol" = s."Code"
                     join trd."_SymbolCategory" c on s."CategoryId" = c."Id"
                     left join trd."_TradeRebate" tr on t."Id" = tr."TradeId"
                     join trd."_TradeAccount" ta on t."TradeAccountId" = ta."Id"
            where tr is null
              and t."Status" = 0
              and t."CloseAt" > '1970-01-01'
              and t."CMD" >= 0
              and t."CMD" <= 5
            order by t."Id"
            -- limit 10
            RETURNING "Id", "TradeId")
    update trd."_TradeTransaction"
    set "Status" = 1
    from inserted_tr_id i
-- join trd."_TradeRebate" tr on inserted_tr_id."Id" = tr."Id"
    WHERE trd."_TradeTransaction"."Id" = i."TradeId"
      and trd."_TradeTransaction"."Status" = 0;
    -- Get the number of updated records
    GET DIAGNOSTICS updated_records = ROW_COUNT;

    -- Return the updated records count
    RETURN updated_records;
END ;
$$;

