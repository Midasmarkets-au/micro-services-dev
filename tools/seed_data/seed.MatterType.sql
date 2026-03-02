create table "_MatterType"
(
    "Id"   integer     not null
        constraint matter_types_pkey
            primary key,
    "Name" varchar(64) not null
);

alter table "_MatterType"
    owner to bcrpro;

grant select on "_MatterType" to dev_read;

INSERT INTO core."_MatterType" ("Id", "Name") VALUES (0, 'System');
INSERT INTO core."_MatterType" ("Id", "Name") VALUES (200, 'InternalTransfer');
INSERT INTO core."_MatterType" ("Id", "Name") VALUES (300, 'Deposit');
INSERT INTO core."_MatterType" ("Id", "Name") VALUES (400, 'Withdrawal');
INSERT INTO core."_MatterType" ("Id", "Name") VALUES (500, 'Rebate');
INSERT INTO core."_MatterType" ("Id", "Name") VALUES (600, 'Refund');
INSERT INTO core."_MatterType" ("Id", "Name") VALUES (700, 'WalletAdjust');
