create table "_Action"
(
    "Id"   integer     not null
        constraint actions_pkey
            primary key,
    "Name" varchar(64) not null
);

alter table "_Action"
    owner to bcrpro;

grant select on "_Action" to dev_read;

INSERT INTO core."_Action" ("Id", "Name") VALUES (0, 'Initial');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1200, 'TransferCreate');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1205, 'TransferCancel');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1206, 'TransferFail');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1210, 'TransferCreateWithApprove');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1215, 'TransferReject');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1220, 'TransferApprove');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1250, 'TransferComplete');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1255, 'TransferWithDepositComplete');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1265, 'TransferWithWithdrawalApprove');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1300, 'DepositCreate');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1305, 'DepositCancel');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1310, 'DepositExecutePayment');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1320, 'DepositCentralApprove');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1325, 'DepositCentralReject');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1330, 'DepositTenantApprove');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1335, 'DepositTenantReject');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1350, 'DepositComplete');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1400, 'WithdrawalCreate');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1405, 'WithdrawalCancel');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1410, 'WithdrawalCentralApprove');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1415, 'WithdrawalCentralReject');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1420, 'WithdrawalTenantApprove');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1425, 'WithdrawalTenantReject');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1430, 'WithdrawalExecutePayment');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1450, 'WithdrawalComplete');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1500, 'RebateCreate');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1505, 'RebateCancel');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1510, 'RebateHold');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1520, 'RebateRelease');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1550, 'RebateComplete');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1340, 'DepositTenantRejectRestore');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1307, 'DepositCancelRestore');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1427, 'WithdrawalTenantRefund');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1600, 'RefundCreated');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1650, 'RefundCompleted');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1345, 'DepositCallbackCompleted');
INSERT INTO core."_Action" ("Id", "Name") VALUES (1315, 'DepositCallbackTimeOut');
