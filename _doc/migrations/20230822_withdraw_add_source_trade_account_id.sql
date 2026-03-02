alter table acct."_Withdrawal"
    add "SourceTradeAccountId" bigint;

alter table acct."_Withdrawal"
    add constraint "_Withdrawal__TradeAccount_Id_fk"
        foreign key ("SourceTradeAccountId") references trd."_TradeAccount";
        
INSERT INTO core."_Action" ("Id", "Name") VALUES (1265, 'TransferWithWithdrawalApprove');
INSERT INTO core."_Transition" ("Id", "ActionId", "OnStateId", "ToStateId", "RoleId", "CreatedOn", "UpdatedOn") VALUES (115, 1265, 200, 250, 10, now(), now());