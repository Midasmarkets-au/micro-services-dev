create index "_Rebate_HoldUntilOn_index"
    on trd."_Rebate" ("HoldUntilOn");

alter table acct."_Payment"
    alter column "ReferenceNumber" type varchar(256) using "ReferenceNumber"::varchar(256);