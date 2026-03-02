alter table trd."_RebateRule"
    rename to "_RebateDirectSchema";

alter table trd."_RebateRuleItem"
    rename to "_RebateDirectSchemaItem";

alter table trd."_RebateRuleTemplate"
    rename to "_RebateBaseSchema";

alter table trd."_RebateRuleTemplateItem"
    rename to "_RebateBaseSchemaItem";

alter table trd."_DirectRebateRule"
    rename to "_RebateDirectRule";

alter table trd."_RebateRuleTemplateBundle"
    rename to "_RebateSchemaBundle";
alter table trd."_BrokerRebateRule"
    rename to "_RebateBrokerRule";
alter table trd."_ClientRebateRule"
    rename to "_RebateClientRule";
rename to "_RebateBrokerRule";
alter table trd."_AgentRebateRule"
    rename to "_RebateAgentRule";

alter table trd."_RebateDirectSchemaItem"
    rename column "RebateRuleId" to "RebateDirectSchemaId";

alter table trd."_RebateDirectRule"
    rename column "RebateRuleId" to "RebateDirectSchemaId";

alter table trd."_RebateBaseSchemaItem"
    rename column "RebateRuleId" to "RebateBaseSchemaId";