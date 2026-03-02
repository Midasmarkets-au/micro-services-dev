create table "_Transition"
(
    "Id"        bigint                                 not null
        constraint transitions_pkey
            primary key,
    "ActionId"  integer                                not null
        constraint core_transitions_action_id_foreign
            references "_Action",
    "OnStateId" integer                                not null
        constraint core_transitions_on_state_id_foreign
            references "_State",
    "ToStateId" integer                                not null
        constraint core_transitions_to_state_id_foreign
            references "_State",
    "RoleId"    integer                                not null
        constraint "_Transition__Role_Id_fk"
            references "_Role",
    "CreatedOn" timestamp with time zone default now() not null,
    "UpdatedOn" timestamp with time zone default now() not null
);

alter table "_Transition"
    owner to bcrpro;

create unique index "_Transition_pk"
    on "_Transition" ("RoleId", "OnStateId", "ToStateId", "ActionId");

create index "IX_transitions_action_id"
    on "_Transition" ("ActionId");

create index "IX_transitions_on_state_id"
    on "_Transition" ("OnStateId");

create index "IX_transitions_to_state_id"
    on "_Transition" ("ToStateId");

grant select on "_Transition" to dev_read;

INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (20, 1200, 0, 200, 10, '2023-09-25 22:57:27.391168 +00:00', '2023-09-25 22:57:27.391168 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (30, 1200, 0, 200, 100, '2023-09-25 22:57:27.977382 +00:00', '2023-09-25 22:57:27.977382 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (40, 1205, 200, 205, 10, '2023-09-25 22:57:28.559694 +00:00', '2023-09-25 22:57:28.559694 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (50, 1205, 210, 205, 10, '2023-09-25 22:57:29.140814 +00:00', '2023-09-25 22:57:29.140814 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (56, 1206, 200, 206, 1, '2023-09-25 23:00:34.773543 +00:00', '2023-09-25 23:00:34.773543 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (60, 1210, 0, 210, 10, '2023-09-25 22:57:29.720445 +00:00', '2023-09-25 22:57:29.720445 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (70, 1210, 0, 210, 100, '2023-09-25 22:57:30.307684 +00:00', '2023-09-25 22:57:30.307684 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (80, 1215, 210, 215, 10, '2023-09-25 22:57:30.896212 +00:00', '2023-09-25 22:57:30.896212 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (100, 1220, 210, 220, 10, '2023-09-25 22:57:31.514103 +00:00', '2023-09-25 22:57:31.514103 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (110, 1250, 200, 250, 1, '2023-09-25 22:57:32.101553 +00:00', '2023-09-25 22:57:32.101553 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (115, 1255, 200, 250, 10, '2023-09-25 22:57:41.508526 +00:00', '2023-09-25 22:57:41.508526 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (116, 1265, 200, 250, 10, '2023-09-25 22:57:42.679674 +00:00', '2023-09-25 22:57:42.679674 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (118, 1265, 200, 250, 100, '2023-09-25 23:00:25.606452 +00:00', '2023-09-25 23:00:25.606452 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (120, 1250, 220, 250, 10, '2023-09-25 22:57:32.682497 +00:00', '2023-09-25 22:57:32.682497 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (130, 1300, 0, 300, 10, '2023-09-25 22:57:33.268929 +00:00', '2023-09-25 22:57:33.268929 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (140, 1300, 0, 300, 100, '2023-09-25 22:57:33.851599 +00:00', '2023-09-25 22:57:33.851599 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (150, 1305, 300, 305, 10, '2023-09-25 22:57:34.427981 +00:00', '2023-09-25 22:57:34.427981 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (160, 1305, 300, 305, 100, '2023-09-25 22:57:35.016435 +00:00', '2023-09-25 22:57:35.016435 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (170, 1310, 300, 310, 10, '2023-09-25 22:57:35.597186 +00:00', '2023-09-25 22:57:35.597186 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (175, 1307, 305, 300, 10, '2023-11-17 23:48:22.232801 +00:00', '2023-11-17 23:48:22.232801 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (180, 1330, 310, 330, 10, '2023-09-25 22:57:36.186362 +00:00', '2023-09-25 22:57:36.186362 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (185, 1330, 315, 330, 1, '2024-02-13 20:43:47.680441 +00:00', '2024-02-13 20:43:47.680441 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (190, 1335, 310, 335, 10, '2023-09-25 22:57:36.767105 +00:00', '2023-09-25 22:57:36.767105 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (200, 1350, 330, 350, 10, '2023-09-25 22:57:37.347794 +00:00', '2023-09-25 22:57:37.347794 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (205, 1340, 335, 310, 10, '2023-11-07 21:21:23.149675 +00:00', '2023-11-07 21:21:23.149675 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (210, 1400, 0, 400, 10, '2023-09-25 22:57:23.841485 +00:00', '2023-09-25 22:57:23.841485 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (220, 1400, 0, 400, 100, '2023-09-25 22:57:37.927353 +00:00', '2023-09-25 22:57:37.927353 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (230, 1405, 400, 405, 10, '2023-09-25 22:57:25.047466 +00:00', '2023-09-25 22:57:25.047466 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (235, 1405, 400, 405, 100, '2023-09-25 22:57:26.806388 +00:00', '2023-09-25 22:57:26.806388 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (240, 1420, 400, 420, 10, '2023-09-25 22:57:26.225000 +00:00', '2023-09-25 22:57:26.225000 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (250, 1425, 400, 425, 10, '2023-09-25 22:57:25.637562 +00:00', '2023-09-25 22:57:25.637562 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (255, 1427, 420, 400, 10, '2023-09-25 22:57:25.637562 +00:00', '2023-09-25 22:57:25.637562 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (260, 1430, 420, 430, 10, '2023-09-25 22:57:24.467190 +00:00', '2023-09-25 22:57:24.467190 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (270, 1450, 430, 450, 10, '2023-09-25 22:57:23.250927 +00:00', '2023-09-25 22:57:23.250927 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (280, 1500, 0, 500, 1, '2023-09-25 22:57:38.550539 +00:00', '2023-09-25 22:57:38.550539 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (290, 1505, 500, 505, 1, '2023-09-25 22:57:39.140443 +00:00', '2023-09-25 22:57:39.140443 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (300, 1510, 500, 510, 1, '2023-09-25 22:57:39.721977 +00:00', '2023-09-25 22:57:39.721977 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (310, 1520, 510, 520, 1, '2023-09-25 22:57:40.324749 +00:00', '2023-09-25 22:57:40.324749 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (320, 1550, 520, 550, 1, '2023-09-25 22:57:40.927185 +00:00', '2023-09-25 22:57:40.927185 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (330, 1550, 510, 550, 1, '2023-09-25 22:57:42.104215 +00:00', '2023-09-25 22:57:42.104215 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (345, 1345, 310, 345, 1, '2024-02-13 03:07:50.866426 +00:00', '2024-02-13 03:07:50.866426 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (350, 1350, 345, 350, 1, '2024-02-13 03:21:35.809367 +00:00', '2024-02-13 03:21:35.809367 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (354, 1315, 310, 315, 1, '2024-02-13 20:37:33.377119 +00:00', '2024-02-13 20:37:33.377119 +00:00');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (400, 1650, 600, 650, 10, '2024-01-11 16:25:07.085000 +00:00', '2024-01-11 16:25:12.543000 +00:00');
