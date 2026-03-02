create table "_Site"
(
    "Id"   integer     not null
        constraint "_Site_pk"
            primary key,
    "Name" varchar(64) not null
);

alter table "_Site"
    owner to bcrpro;

create unique index "_Site_Name_uindex"
    on "_Site" ("Name");

grant select on "_Site" to dev_read;

INSERT INTO core."_Site" ("Id", "Name") VALUES (0, 'DEFAULT');
INSERT INTO core."_Site" ("Id", "Name") VALUES (1, 'BVI');
INSERT INTO core."_Site" ("Id", "Name") VALUES (2, 'AU');
INSERT INTO core."_Site" ("Id", "Name") VALUES (3, 'CN');
INSERT INTO core."_Site" ("Id", "Name") VALUES (4, 'TW');
INSERT INTO core."_Site" ("Id", "Name") VALUES (5, 'VN');
INSERT INTO core."_Site" ("Id", "Name") VALUES (6, 'JP');
INSERT INTO core."_Site" ("Id", "Name") VALUES (7, 'MN');
INSERT INTO core."_Site" ("Id", "Name") VALUES (8, 'MY');


'EA0301',
'EA0302',
'EA0303',
'EA0304',
'EA0305',
'EA0306',
'EA0308',
'EA0309'