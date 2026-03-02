-- auto-generated definition
create table "_FundType"
(
    "Id"   integer     not null
        constraint wallet_types_pkey
            primary key,
    "Name" varchar(64) not null
);

alter table "_FundType"
    owner to bcrpro;

grant select on "_FundType" to dev_read;

INSERT INTO acct."_FundType" ("Id", "Name") VALUES (1, 'Wire Transfer');
INSERT INTO acct."_FundType" ("Id", "Name") VALUES (2, 'IPS/Demo');
INSERT INTO acct."_FundType" ("Id", "Name") VALUES (3, 'Bonus');
INSERT INTO acct."_FundType" ("Id", "Name") VALUES (4, 'Standard Account');
INSERT INTO acct."_FundType" ("Id", "Name") VALUES (5, 'Rewards/Commission');
INSERT INTO acct."_FundType" ("Id", "Name") VALUES (6, 'Alpha');