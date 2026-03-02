INSERT INTO core."_Action" ("Id", "Name") VALUES (1275, 'TransferWithWithdrawalCancel') ON CONFLICT DO NOTHING;

INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId") VALUES (115, 1265, 200, 250, 10 ) ON CONFLICT DO NOTHING;
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId") VALUES (118, 1265, 200, 250, 100) ON CONFLICT DO NOTHING;