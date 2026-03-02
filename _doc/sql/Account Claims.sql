-- Scripts for creating claims for accounts after migration

-- delete from auth."_UserClaim"
--  where "ClaimType" = 'SalesAccount' or "ClaimType" = 'AgentAccount';


create table public.users
(
    id            bigint not null
        constraint users_pk
            primary key,
    sales_account bigint not null,
    ib_account    bigint not null,
    login         bigint not null
);

create index users_ib_account_index
    on public.users (ib_account);

create index users_sales_account_index
    on public.users (sales_account);

 select u."Id" as  "UserId", 'SalesAccount' as "ClaimType", cast( a."Id" as varchar) as  "ClaimValue"
from trd."_Account" a
join auth."_User" u on u."PartyId" = a."PartyId"
left join auth."_UserClaim" c on c."UserId" = u."Id" and c."ClaimType" = 'SalesAccount' and c."ClaimValue"=cast( a."Id" as varchar)
where c is null and
    a."Role"=20
group by u."Id",  a."Id"


update trd."_Account"
set "SalesAccountId" = CASE WHEN sa is not null THEN sa."Id" WHEN sa is null THEN NULL END,
    "AgentAccountId" = CASE WHEN ib is not null THEN ib."Id" WHEN ib is null THEN NULL END
from users u
         join trd."_TradeAccount" ta on ta."AccountNumber" = u.login
         join trd."_Account" a on a."Id" = ta."Id"
left join trd."_TradeAccount" sa on sa."AccountNumber" = u.sales_account
left join trd."_TradeAccount" ib on ib."AccountNumber" = u.ib_account
where trd."_Account"."Id" = ta."Id";



select CASE WHEN u.sales_account > 0 THEN u.sales_account WHEN u.sales_account = 0 THEN NULL END
from users u
         join trd."_TradeAccount" ta on ta."AccountNumber" = u.login
         join trd."_Account" a on a."Id" = ta."Id"
-- where trd."_Account"."Id"=a."Id"
limit 20;

select sa.*
from users u
left join trd."_TradeAccount" sa on sa."AccountNumber" = u.sales_account;

update  trd."_Account" set "AgentAccountId" = null , "SalesAccountId"=null where "Id" >10008


update  trd."_Account" set "Role" = 10
from users u
join trd."_TradeAccount" sa on sa."AccountNumber" = u.sales_account
where trd."_Account"."Id"=sa."Id";

update  trd."_Account" set "Role" = 20
from users u
join trd."_TradeAccount" sa on sa."AccountNumber" = u.ib_account
where trd."_Account"."Id"=sa."Id";



insert into auth."_UserClaim"("UserId", "ClaimType", "ClaimValue")
select u."Id" as  "UserId", 'SalesAccount' as "ClaimType", cast( a."Id" as varchar) as  "ClaimValue"
from trd."_Account" a
join auth."_User" u on u."PartyId" = a."PartyId"
left join auth."_UserClaim" c on c."UserId" = u."Id" and c."ClaimType" = 'SalesAccount' and c."ClaimValue"=cast( a."Id" as varchar)
where c is null and
    a."Role"=10
group by u."Id",  a."Id";


insert into auth."_UserRole"("UserId", "RoleId")
select u."Id" as  "UserId", 20 as "RoleId"
from trd."_Account" a
join auth."_User" u on u."PartyId" = a."PartyId"
left join auth."_UserRole" c on c."UserId" = u."Id" and c."RoleId"=20
where c is null and
    a."Role"=10
group by u."Id";


insert into auth."_UserClaim"("UserId", "ClaimType", "ClaimValue")
select u."Id" as  "UserId", 'AgentAccount' as "ClaimType", cast( a."Id" as varchar) as  "ClaimValue"
from trd."_Account" a
join auth."_User" u on u."PartyId" = a."PartyId"
left join auth."_UserClaim" c on c."UserId" = u."Id" and c."ClaimType" = 'AgentAccount' and c."ClaimValue"=cast( a."Id" as varchar)
where c is null and
    a."Role"=20
group by u."Id",  a."Id";

insert into auth."_UserRole"("UserId", "RoleId")
select u."Id" as  "UserId", 30 as "RoleId"
from trd."_Account" a
join auth."_User" u on u."PartyId" = a."PartyId"
left join auth."_UserRole" c on c."UserId" = u."Id" and c."RoleId"=30
where c is null and
    a."Role"=20
group by u."Id";