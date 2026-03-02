using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway;

/**
 * Rebate related methods
 */
partial class TradingService
{
    public async Task<Result<List<RebateAgentRule.ResponseModel>, RebateAgentRule.Criteria>> AgentRebateRuleQueryAsync(
        RebateAgentRule.Criteria criteria)
    {
        var items = await dbContext.RebateAgentRules
            .PagedFilterBy(criteria)
            .ToResponseModel()
            .ToListAsync();

        foreach (var item in items)
        {
            item.CalculatedLevelSetting = await GetCalculatedRebateLevelSettingById(item.AgentAccountId);
        }

        return Result<List<RebateAgentRule.ResponseModel>, RebateAgentRule.Criteria>.Of(items, criteria);
    }

    public async Task<Result<List<RebateClientRule.ResponseModel>, RebateClientRule.Criteria>>
        ClientRebateRuleQueryAsync(RebateClientRule.Criteria criteria)
    {
        var items = await dbContext.RebateClientRules
            .PagedFilterBy(criteria)
            .ToResponseModel()
            .ToListAsync();
        return Result<List<RebateClientRule.ResponseModel>, RebateClientRule.Criteria>.Of(items, criteria);
    }

    public async Task<RebateAgentRule.ResponseModel> AgentRebateRuleGetAsync(long ruleId) =>
        await dbContext.RebateAgentRules
            .ToResponseModel()
            .FirstOrDefaultAsync(x => x.Id.Equals(ruleId)) ?? new RebateAgentRule.ResponseModel();

    public async Task<RebateAgentRule.ResponseModel> AgentRebateRuleGetForAgentAsync(long ruleId, long agentUid) =>
        await dbContext.RebateAgentRules
            .Where(x => x.AgentAccount.ReferPath.Contains(agentUid.ToString()))
            .ToResponseModel()
            .FirstOrDefaultAsync(x => x.Id.Equals(ruleId)) ?? new RebateAgentRule.ResponseModel();

    public async Task<RebateAgentRule.ResponseModel> AgentRebateRuleGetForRepAsync(long ruleId, long repUid) =>
        await dbContext.RebateAgentRules
            .Where(x => x.AgentAccount.ReferPath.Contains(repUid.ToString()))
            .ToResponseModel()
            .FirstOrDefaultAsync(x => x.Id.Equals(ruleId)) ?? new RebateAgentRule.ResponseModel();

    public async Task<RebateAgentRule.ResponseModel> AgentRebateRuleGetForSalesAsync(long ruleId, long salesUid) =>
        await dbContext.RebateAgentRules
            .Where(x => x.AgentAccount.ReferPath.Contains(salesUid.ToString()))
            .ToResponseModel()
            .FirstOrDefaultAsync(x => x.Id.Equals(ruleId)) ?? new RebateAgentRule.ResponseModel();

    public async Task<RebateAgentRule.ResponseModel> AgentRebateRuleGetByAccountIdAsync(long accountId) =>
        await dbContext.RebateAgentRules
            .Where(x => x.AgentAccountId.Equals(accountId))
            .ToResponseModel()
            .FirstOrDefaultAsync() ?? new RebateAgentRule.ResponseModel();

    public async Task<RebateAgentRule> AgentRebateRuleUpdateAsync(long ruleId,
        RebateAgentRule.UpdateSpec spec,
        long operatorPartyId = 1)
    {
        var item = await dbContext.RebateAgentRules
            .Include(x => x.AgentAccount)
            .FirstOrDefaultAsync(x => x.Id.Equals(ruleId)) ?? new RebateAgentRule();
        if (item.IsEmpty()) return item;

        var setting = new RebateAgentRule.RebateLevelSetting
        {
            Language = item.GetLevelSetting().Language,
            AllowedAccounts = spec.Schema,
        };

        var log = Account.BuildLog(operatorPartyId, "UpdateRebateRule"
            , ""
            , "");

        item.Schema = Utils.JsonSerializeObject(spec.Schema);
        item.LevelSetting = Utils.JsonSerializeObject(setting);
        item.AgentAccount.AccountLogs.Add(log);
        item.UpdatedOn = DateTime.UtcNow;
        dbContext.RebateAgentRules.Update(item);

        await dbContext.SaveChangesWithAuditAsync(operatorPartyId);
        return item;
    }

    public async Task<RebateAgentRule> AgentRebateRuleLevelSettingUpdateAsync(long ruleId, RebateLevelSchema spec, long operatorPartyId = 1)
    {
        var item = await dbContext.RebateAgentRules
            .Include(x => x.AgentAccount)
            .FirstOrDefaultAsync(x => x.Id == ruleId) ?? new RebateAgentRule();
        if (item.IsEmpty()) return item;

        // Only Top Level Agent can update level setting
        // if (!item.AgentAccount.IsTopLevelAgent())
        //     return new RebateAgentRule();

        var levelSetting = item.GetLevelSetting();
        var allowedAccountIndex = levelSetting.AllowedAccounts.FindIndex(x => x.AccountType == spec.AccountType);
        var log = Account.BuildLog(operatorPartyId, "UpdateRebateLevelSetting", "", JsonConvert.SerializeObject(spec));
        if (allowedAccountIndex < 0)
        {
            levelSetting.AllowedAccounts.Add(spec);
        }
        else
        {
            log.Before = JsonConvert.SerializeObject(levelSetting.AllowedAccounts[allowedAccountIndex]);
            levelSetting.AllowedAccounts[allowedAccountIndex] = spec;
        }

        item.AgentAccount.AccountLogs.Add(log);
        item.LevelSetting = JsonConvert.SerializeObject(levelSetting);
        item.UpdatedOn = DateTime.UtcNow;
        dbContext.RebateAgentRules.Update(item);
        await dbContext.SaveChangesWithAuditAsync(operatorPartyId);
        return item;
    }

    public async Task<RebateAgentRule> AgentRebateRuleLevelSettingClearAsync(long ruleId, long operatorPartyId = 1)
    {
        var item = await dbContext.RebateAgentRules
            .Include(x => x.AgentAccount)
            .FirstOrDefaultAsync(x => x.Id == ruleId) ?? new RebateAgentRule();
        if (item.IsEmpty()) return item;

        item.AgentAccount.AccountLogs.Add(Account.BuildLog(operatorPartyId, "ClearRebateLevelSetting",
            item.LevelSetting, ""));
        item.LevelSetting = JsonConvert.SerializeObject(new { });
        item.UpdatedOn = DateTime.UtcNow;
        dbContext.RebateAgentRules.Update(item);
        await dbContext.SaveChangesWithAuditAsync(operatorPartyId);
        return item;
    }


    public async Task<RebateAgentRule> AgentRebateRuleSchemaUpdateAsync(long ruleId, RebateLevelSchema spec,
        long operatorPartyId = 1)
    {
        var item = await dbContext.RebateAgentRules
            .Include(x => x.AgentAccount)
            .FirstOrDefaultAsync(x => x.Id == ruleId) ?? new RebateAgentRule();
        if (item.IsEmpty()) return item;

        var schema = RebateLevelSchema.ListFromJson(item.Schema);
        if (schema == null) return new RebateAgentRule();

        var allowedAccountIndex = schema.FindIndex(x => x.AccountType == spec.AccountType);
        var log = Account.BuildLog(operatorPartyId, "UpdateRebateRuleSchema", "", JsonConvert.SerializeObject(spec));
        if (allowedAccountIndex < 0)
        {
            log.Before = "";
            schema.Add(spec);
        }
        else
        {
            log.Before = JsonConvert.SerializeObject(schema[allowedAccountIndex]);
            schema[allowedAccountIndex] = spec;
        }

        item.Schema = JsonConvert.SerializeObject(schema);
        item.AgentAccount.AccountLogs.Add(log);
        item.UpdatedOn = DateTime.UtcNow;

        // var allowedAccountsForLevelSetting =
        //     RebateLevelSchema.BuildDefaultAllowedAccountTypesForRebateLevelSetting(schema);

        // var levelSettings = item.GetLevelSetting();
        // levelSettings.AllowedAccounts = allowedAccountsForLevelSetting;
        // item.LevelSetting = JsonConvert.SerializeObject(levelSettings);

        dbContext.RebateAgentRules.Update(item);
        await dbContext.SaveChangesWithAuditAsync(operatorPartyId);

        // await UpdateRebateRuleLevelSettingRemainByAgentId(item.AgentAccountId, operatorPartyId);
        return item;
    }

    public async Task<RebateClientRule.ResponseModel> ClientRebateRuleGetAsync(long ruleId) =>
        await dbContext.RebateClientRules
            .ToResponseModel()
            .FirstOrDefaultAsync(x => x.Id.Equals(ruleId)) ?? new RebateClientRule.ResponseModel();

    // public async Task<ClientRebateRule.ResponseModel> ClientRebateRuleGetForAgentAsync(long ruleId, long agentUid) =>
    //     await _ctx.ClientRebateRules
    //         .Where(x => x.ClientAccount.AgentAccountId > 0 && x.ClientAccount.AgentAccount!.Uid == agentUid)
    //         .ToResponseModel()
    //         .FirstOrDefaultAsync(x => x.Id.Equals(ruleId)) ?? new ClientRebateRule.ResponseModel();
    //
    // public async Task<ClientRebateRule.ResponseModel> ClientRebateRuleGetForSalesAsync(long ruleId, long salesUid) =>
    //     await _ctx.ClientRebateRules
    //         .Where(x => x.ClientAccount.SalesAccountId > 0 && x.ClientAccount.SalesAccount!.Uid == salesUid)
    //         .ToResponseModel()
    //         .FirstOrDefaultAsync(x => x.Id.Equals(ruleId)) ?? new ClientRebateRule.ResponseModel();
    //
    // public async Task<ClientRebateRule.ResponseModel> ClientRebateRuleGetByAccountIdAsync(long accountId) =>
    //     await _ctx.ClientRebateRules
    //         .ToResponseModel()
    //         .FirstOrDefaultAsync(x => x.ClientAccount.Equals(accountId)) ?? new ClientRebateRule.ResponseModel();

    public async Task<RebateClientRule> ClientRebateRuleDistributionUpdateAsync(long ruleId,
        RebateClientRule.UpdateSpec spec,
        long operatorPartyId = 1)
    {
        var item = await dbContext.RebateClientRules
            .Include(x => x.ClientAccount)
            .FirstOrDefaultAsync(x => x.Id == ruleId) ?? new RebateClientRule();
        if (item.IsEmpty()) return item;

        item.ClientAccount.AccountLogs.Add(Account.BuildLog(operatorPartyId, "UpdateRebateDistributionForClient"
            , (Enum.GetName(typeof(RebateDistributionTypes), item.DistributionType) ?? "") + $"ID:{item.Id}"
            , (Enum.GetName(typeof(RebateDistributionTypes), spec.DistributionType) ?? "") + $"ID:{spec.RebateDirectSchemaId}"));

        item.DistributionType = (short)spec.DistributionType;
        if (spec.ShouldUpdateRebateDirectSchemaId())
        {
            item.RebateDirectSchemaId = spec.RebateDirectSchemaId;
        }

        item.UpdatedOn = DateTime.UtcNow;
        dbContext.RebateClientRules.Update(item);
        await dbContext.SaveChangesWithAuditAsync(operatorPartyId);

        try
        {
            await accManageSvc.SetPipAndCommissionTagAsync(item.ClientAccountId);
        }
        catch
        {
            // ignored
        }

        return item;
    }

    public async Task<RebateAgentRule> AgentRebateRuleCreateAsync(RebateAgentRule.CreateSpec spec, long operatorPartyId = 1)
    {
        var item = new RebateAgentRule
        {
            AgentAccountId = spec.AgentAccountId,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            Schema = Utils.JsonSerializeObject(spec.Schema),
            LevelSetting = Utils.JsonSerializeObject(spec.LevelSetting),
        };

        // assign parent rule from agent group
        // var group = _ctx.Groups
        //     .Where(x => x.Type == (int)AccountGroupTypes.Agent)
        //     .FirstOrDefault(x => x.Accounts.Any(a => a.Id == spec.AgentAccountId));
        // if (group != null)
        // {
        //     var parentRule =
        //         await _ctx.RebateAgentRules.FirstOrDefaultAsync(x => x.AgentAccountId == group.OwnerAccountId);
        //     if (parentRule != null)
        //         item.ParentId = parentRule.Id;
        // }

        await dbContext.RebateAgentRules.AddAsync(item);
        await dbContext.SaveChangesWithAuditAsync(operatorPartyId);
        return item;
    }

    public async Task<RebateDirectRule> DirectRebateRuleLookupAsync(long sourceAcctId, long targetAcctId)
        => await dbContext.RebateDirectRules
            .Where(x => x.SourceTradeAccountId == sourceAcctId)
            .Where(x => x.TargetAccountId == targetAcctId)
            .AsNoTracking()
            .FirstOrDefaultAsync() ?? new RebateDirectRule();

    /// <summary>
    /// Create direct rebate rule. If client rebate rule not exists, create it.
    /// </summary>
    /// <param name="sourceAcctId"></param>
    /// <param name="targetAcctId"></param>
    /// <param name="rebateRuleId"></param>
    /// <param name="operatorPartyId"></param>
    /// <returns></returns>
    public async Task<RebateDirectRule> DirectRebateRuleCreate(long sourceAcctId, long targetAcctId,
        long rebateRuleId, long operatorPartyId = 1)
    {
        var item = new RebateDirectRule
        {
            SourceTradeAccountId = sourceAcctId,
            TargetAccountId = targetAcctId,
            RebateDirectSchemaId = rebateRuleId,
            CreatedBy = operatorPartyId,
            CreatedOn = DateTime.UtcNow,
        };
        await dbContext.RebateDirectRules.AddAsync(item);
        await dbContext.SaveChangesWithAuditAsync(operatorPartyId);

        var clientRebateRule = await dbContext.RebateClientRules
            .Where(x => x.ClientAccountId == sourceAcctId)
            .SingleOrDefaultAsync();
        if (clientRebateRule != null)
            return item;

        clientRebateRule = new RebateClientRule
        {
            ClientAccountId = sourceAcctId, DistributionType = (int)RebateDistributionTypes.Direct,
        };
        await dbContext.RebateClientRules.AddAsync(clientRebateRule);
        await dbContext.SaveChangesWithAuditAsync(operatorPartyId);

        return item;
    }

    public async Task<RebateDirectRule> DirectRebateRuleUpdateRuleId(long id,
        long rebateRuleId, long operatorPartyId = 1)
    {
        var item = await dbContext.RebateDirectRules
            .SingleAsync(x => x.Id == id);
        item.RebateDirectSchemaId = rebateRuleId;
        item.UpdatedOn = DateTime.UtcNow;

        dbContext.RebateDirectRules.Update(item);
        await dbContext.SaveChangesWithAuditAsync(operatorPartyId);
        return item;
    }

    public async Task<Result<List<RebateDirectRule.ClientResponseModel>, RebateDirectRule.Criteria>> DirectRebateRuleQuery(
        RebateDirectRule.Criteria criteria)
    {
        var items = await dbContext.RebateDirectRules
            .PagedFilterBy(criteria)
            .ToClientResponseModel()
            .ToListAsync();
        return Result<List<RebateDirectRule.ClientResponseModel>, RebateDirectRule.Criteria>.Of(items, criteria);
    }

    public async Task<RebateDirectRule> DirectRebateRuleGet(long id) =>
        await dbContext.RebateDirectRules
            .FirstOrDefaultAsync(x => x.Id == id) ?? new RebateDirectRule();

    public async Task<bool> DirectRebateRuleDelete(long id, long operatorPartyId = 1)
    {
        var item = await dbContext.RebateDirectRules
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item == null) return false;
        dbContext.RebateDirectRules.Remove(item);
        return await dbContext.SaveChangesWithAuditAsync(operatorPartyId) > 0;
    }

    public async Task<RebateAgentRule.RebateLevelSetting> GetCalculatedRebateLevelSettingById(long accountId)
    {
        var account = await dbContext.Accounts.SingleOrDefaultAsync(x => x.Id == accountId);
        if (account == null) return new RebateAgentRule.RebateLevelSetting();

        return await GetCalculatedRebateLevelSetting(account);
    }

    public async Task<RebateAgentRule.RebateLevelSetting> GetCalculatedRebateLevelSettingByUid(long accountUid)
    {
        var account = await dbContext.Accounts.SingleOrDefaultAsync(x => x.Uid == accountUid);
        if (account == null) return new RebateAgentRule.RebateLevelSetting();

        return await GetCalculatedRebateLevelSetting(account);
    }

    private async Task<RebateAgentRule.RebateLevelSetting> GetCalculatedRebateLevelSetting(Account account)
    {
        var uids = account.ReferPathUids;
        var parentRules = await dbContext.RebateAgentRules
            .Where(x => uids.Contains(x.AgentAccount.Uid))
            .OrderBy(x => x.AgentAccount.Level)
            .ToResponseModel()
            .ToListAsync();

        var selfRule = parentRules.FirstOrDefault(x => x.AgentAccountId == account.Id);
        if (selfRule == null) return new RebateAgentRule.RebateLevelSetting();

        if (account.IsTopLevelAgent()) return selfRule.LevelSetting;

        var lastRuleHasLevelSetting = parentRules.LastOrDefault(x => !string.IsNullOrEmpty(x.LevelSettingJson) && x.LevelSettingJson != "{}");
        if (lastRuleHasLevelSetting == null)
        {
            BcrLog.Slack($"No account in path has level setting. AccountId: {account.Id}, Tid: {_tenantId}");
            return new RebateAgentRule.RebateLevelSetting();
        }

        var baseLevelSettings = lastRuleHasLevelSetting.LevelSetting;
        var resultAllowedAccounts = selfRule.Schema.ToList();

        foreach (var allowedAccount in resultAllowedAccounts)
        {
            allowedAccount.Percentage = 100;
            var baseAllowedAccount = baseLevelSettings.AllowedAccounts.FirstOrDefault(x => x.AccountType == allowedAccount.AccountType);
            if (baseAllowedAccount == null) continue;

            var levelSchemas = parentRules.Skip(1)
                .Select(x => x.Schema.FirstOrDefault(s => s.AccountType == allowedAccount.AccountType))
                .Where(x => x != null)
                .Select(x => x!)
                .ToList();

            var isPercentageZero = levelSchemas.Any(x => x.Percentage == 100);
            var summedItems = levelSchemas
                .SelectMany(x => x.Items)
                .GroupBy(x => x.CategoryId)
                .Select(x => new RebateLevelSchemaItem(x.Key,
                    Math.Max(0,
                        baseAllowedAccount.Items.First(i => i.CategoryId == x.Key).Rate - x.Sum(item => item.Rate)))
                )
                .ToList();

            allowedAccount.Items = summedItems;
            allowedAccount.Percentage = isPercentageZero ? 0 : allowedAccount.Percentage;
        }

        var result = selfRule.LevelSetting;
        result.AllowedAccounts = resultAllowedAccounts;
        return result;
    }
}