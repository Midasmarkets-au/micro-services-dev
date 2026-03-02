INSERT INTO core."_Action" ("Id", "Name") VALUES (1206, 'TransferFail') on conflict do nothing ;
INSERT INTO core."_State" ("Id", "Name") VALUES (206, 'TransferFailed') on conflict do nothing;
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (56, 1206, 200, 206, 1, DEFAULT, DEFAULT) on conflict do nothing;