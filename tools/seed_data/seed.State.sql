create table "_State"
(
    "Id"   integer     not null
        constraint states_pkey
            primary key,
    "Name" varchar(64) not null
);

alter table "_State"
    owner to bcrpro;

grant select on "_State" to dev_read;

INSERT INTO core."_State" ("Id", "Name") VALUES (0, 'Initialed');
INSERT INTO core."_State" ("Id", "Name") VALUES (200, 'TransferCreated');
INSERT INTO core."_State" ("Id", "Name") VALUES (205, 'TransferCanceled');
INSERT INTO core."_State" ("Id", "Name") VALUES (206, 'TransferFailed');
INSERT INTO core."_State" ("Id", "Name") VALUES (210, 'TransferAwaitingApproval');
INSERT INTO core."_State" ("Id", "Name") VALUES (215, 'TransferRejected');
INSERT INTO core."_State" ("Id", "Name") VALUES (220, 'TransferApproved');
INSERT INTO core."_State" ("Id", "Name") VALUES (250, 'TransferCompleted');
INSERT INTO core."_State" ("Id", "Name") VALUES (300, 'DepositCreated');
INSERT INTO core."_State" ("Id", "Name") VALUES (305, 'DepositCanceled');
INSERT INTO core."_State" ("Id", "Name") VALUES (306, 'DepositFailed');
INSERT INTO core."_State" ("Id", "Name") VALUES (310, 'DepositPaymentCompleted');
INSERT INTO core."_State" ("Id", "Name") VALUES (320, 'DepositCentralApproved');
INSERT INTO core."_State" ("Id", "Name") VALUES (325, 'DepositCentralRejected');
INSERT INTO core."_State" ("Id", "Name") VALUES (330, 'DepositTenantApproved');
INSERT INTO core."_State" ("Id", "Name") VALUES (335, 'DepositTenantRejected');
INSERT INTO core."_State" ("Id", "Name") VALUES (350, 'DepositCompleted');
INSERT INTO core."_State" ("Id", "Name") VALUES (400, 'WithdrawalCreated');
INSERT INTO core."_State" ("Id", "Name") VALUES (405, 'WithdrawalCanceled');
INSERT INTO core."_State" ("Id", "Name") VALUES (406, 'WithdrawalFailed');
INSERT INTO core."_State" ("Id", "Name") VALUES (410, 'WithdrawalCentralApproved');
INSERT INTO core."_State" ("Id", "Name") VALUES (415, 'WithdrawalCentralRejected');
INSERT INTO core."_State" ("Id", "Name") VALUES (420, 'WithdrawalTenantApproved');
INSERT INTO core."_State" ("Id", "Name") VALUES (425, 'WithdrawalTenantRejected');
INSERT INTO core."_State" ("Id", "Name") VALUES (430, 'WithdrawalPaymentCompleted');
INSERT INTO core."_State" ("Id", "Name") VALUES (450, 'WithdrawalCompleted');
INSERT INTO core."_State" ("Id", "Name") VALUES (500, 'RebateCreated');
INSERT INTO core."_State" ("Id", "Name") VALUES (505, 'RebateCanceled');
INSERT INTO core."_State" ("Id", "Name") VALUES (510, 'RebateOnHold');
INSERT INTO core."_State" ("Id", "Name") VALUES (520, 'RebateReleased');
INSERT INTO core."_State" ("Id", "Name") VALUES (550, 'RebateCompleted');
INSERT INTO core."_State" ("Id", "Name") VALUES (600, 'RefundCreated');
INSERT INTO core."_State" ("Id", "Name") VALUES (650, 'RefundCompleted');
INSERT INTO core."_State" ("Id", "Name") VALUES (345, 'DepositCallbackCompleted');
INSERT INTO core."_State" ("Id", "Name") VALUES (315, 'DepositCallbackTimedOut');
INSERT INTO core."_State" ("Id", "Name") VALUES (590, 'RebateTradeClosedLessThanOneMinute');
INSERT INTO core."_State" ("Id", "Name") VALUES (700, 'WalletAdjustCreated');
INSERT INTO core."_State" ("Id", "Name") VALUES (750, 'WalletAdjustCompleted');
