-- alter payment service access
alter table acct."_PaymentService"
    add "CategoryName" varchar(32) default '' not null;
    
alter table acct."_PaymentServiceAccess"
    add "CurrencyId" integer default -1 not null;

alter table acct."_PaymentServiceAccess"
    add constraint "_PaymentServiceAccess__Currency_Id_fk"
        foreign key ("CurrencyId") references acct."_Currency";

drop index acct."_PaymentServiceAccess_PartyId_FundType_PaymentServiceId_uindex";

create unique index "_PaymentServiceAccess_PartyId_FundType_CurrencyId_PaymentServiceId_uindex"
    on acct."_PaymentServiceAccess" ("PartyId", "FundType", "CurrencyId", "PaymentServiceId");

-- alter account
alter table trd."_Account"
    add "CurrencyId" integer default -1 not null;

alter table trd."_Account"
    add constraint "_Account__Currency_Id_fk"
        foreign key ("CurrencyId") references acct."_Currency";

-- alter wallet
alter table acct."_Wallet"
    add "Number" varchar(64) default '' not null;

update acct."_Wallet"
set "Number" = CONCAT('w', "CurrencyId", 'f', "FundType", '000', "Id")
where "Id" > 0;

create unique index "_Wallet_Number_uindex"
    on acct."_Wallet" ("Number");

update acct."_PaymentServiceAccess" psa
set "CurrencyId"=ta."CurrencyId"
from trd."_TradeAccount" ta
         join trd."_Account" a on a."Id" = ta."Id"
where a."PartyId" = psa."PartyId"
  and psa."CurrencyId" = -1;

update trd."_Account" a
set "CurrencyId"=ta."CurrencyId"
from trd."_TradeAccount" ta
where  a."Id" = ta."Id"
  and a."CurrencyId" = -1;