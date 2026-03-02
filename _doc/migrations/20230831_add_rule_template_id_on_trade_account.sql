alter table trd."_TradeAccount"
    add "RebateRuleTemplateId" bigint;

alter table trd."_TradeAccount"
    add constraint "_TradeAccount__RebateRuleTemplate_Id_fk"
        foreign key ("RebateRuleTemplateId") references trd."_RebateRuleTemplate";
