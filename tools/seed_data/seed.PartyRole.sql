create table "_Role"
(
    "Id"   integer     not null
        constraint "_Role_pk"
            primary key,
    "Name" varchar(32) not null
);

alter table "_Role"
    owner to bcrpro;

grant select on "_Role" to dev_read;

INSERT INTO core."_Role" ("Id", "Name") VALUES (1, 'System');
INSERT INTO core."_Role" ("Id", "Name") VALUES (2, 'Super Admin');
INSERT INTO core."_Role" ("Id", "Name") VALUES (10, 'Tenant Admin');
INSERT INTO core."_Role" ("Id", "Name") VALUES (100, 'Sales');
INSERT INTO core."_Role" ("Id", "Name") VALUES (110, 'Rep');
INSERT INTO core."_Role" ("Id", "Name") VALUES (200, 'Broker');
INSERT INTO core."_Role" ("Id", "Name") VALUES (300, 'IB');
INSERT INTO core."_Role" ("Id", "Name") VALUES (400, 'Client');
INSERT INTO core."_Role" ("Id", "Name") VALUES (1000, 'Guest');
