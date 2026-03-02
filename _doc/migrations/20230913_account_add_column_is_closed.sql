alter table trd."_Account"
    add "IsClosed" integer default 0 not null;

create index "_Account_IsClosed_index"
    on trd."_Account" ("IsClosed");