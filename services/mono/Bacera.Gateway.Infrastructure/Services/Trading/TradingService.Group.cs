using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;

namespace Bacera.Gateway;

partial class TradingService
{
    private static TimeSpan CacheExpireHours { get; } = TimeSpan.FromHours(4);

    private static string GetGroupNamePrefixKeywordsKey(string? keywords = "")
        => string.IsNullOrEmpty(keywords)
            ? string.Empty
            : keywords.Length >= 2
                ? keywords.ToUpper()[..2]
                : keywords.ToUpper();

    private string GetGroupNameKey(AccountRoleTypes? type, long? accountUid = 0, string? keywords = "")
    {
        var prefix = $"group_name_list_T:{tenancy.GetTenantId()}_G:{type}_AU:{accountUid}_";
        var prefixKeywordsKey = GetGroupNamePrefixKeywordsKey(keywords);
        return prefix + (string.IsNullOrEmpty(prefixKeywordsKey) ? "All" : $"K:{prefixKeywordsKey}");
    }

    private async Task SetGroupNameCache(List<string> value, AccountRoleTypes? type, long? accountUid = 0,
        string? keywords = "")
    {
        var key = GetGroupNameKey(type, accountUid, keywords);
        await myCache.SetStringAsync(key, JsonConvert.SerializeObject(value), CacheExpireHours);
    }

    private async Task<List<string>?> GetGroupNameCache(AccountRoleTypes? type, long? accountUid = 0,
        string? keywords = "")
    {
        var key = GetGroupNameKey(type, accountUid, keywords);
        var value = await myCache.GetStringAsync(key);
        if (value == null) return null;
        var groupNames = new List<string>();
        if (string.IsNullOrEmpty(value)) return groupNames;
        try
        {
            groupNames = JsonConvert.DeserializeObject<List<string>>(value) ?? new List<string>();
        }
        catch
        {
            // ignored
        }

        return groupNames;
    }

    public async Task<List<string>> GetAllGroupNamesAsync(AccountRoleTypes? type, string keywords = "")
    {
        var prefixKeywordsKey = GetGroupNamePrefixKeywordsKey(keywords);

        var cachedGroupNames = await GetGroupNameCache(type, 0, prefixKeywordsKey);
        if (cachedGroupNames != null && cachedGroupNames.Any())
            return cachedGroupNames.Where(x => x.ToUpper().StartsWith(keywords.ToUpper())).ToList();

        var groupNames = await dbContext.Accounts
            .Where(x => type == null || x.Role == (int)type)
            .Select(x => type == AccountRoleTypes.Agent ? x.Group : x.Code)
            .Where(x => string.IsNullOrEmpty(prefixKeywordsKey) || x.ToUpper().StartsWith(prefixKeywordsKey))
            .ToListAsync();

        await SetGroupNameCache(groupNames, type, 0, prefixKeywordsKey);
        return groupNames;
    }

    public async Task<List<string>> GetAllGroupNamesUnderAccountByUid(long ownerAccountUid, AccountRoleTypes type,
        string keywords = "")
    {
        var prefixKeywordsKey = GetGroupNamePrefixKeywordsKey(keywords);
        var cachedGroupNames = await GetGroupNameCache(type, ownerAccountUid, prefixKeywordsKey);

        if (cachedGroupNames != null && cachedGroupNames.Any())
            return cachedGroupNames.Where(x => x.ToUpper().StartsWith(keywords.ToUpper())).ToList();

        var groupNames = await dbContext.Accounts
            .Where(x => x.Uid == ownerAccountUid)
            .Where(x => x.Role == (int)type)
            .Select(x => type == AccountRoleTypes.Agent ? x.Group : x.Code)
            .Where(x => string.IsNullOrEmpty(prefixKeywordsKey) || x.ToUpper().StartsWith(prefixKeywordsKey))
            .ToListAsync();

        await SetGroupNameCache(groupNames, type, ownerAccountUid, prefixKeywordsKey);
        return groupNames;
    }


    public async Task<(bool, string)> ChangeSalesGroupAsync(long accountId, long parentAccountId, long operatorPartyId)
    {
        var account = await dbContext.Accounts
            .Where(x => x.Id == accountId)
            .SingleOrDefaultAsync();
        if (account == null)
            return (false, "__ACCOUNT_NOT_FOUND__");
        if (account.ReferPath.Length <= 8)
            return (false, "__ACCOUNT_REFER_PATH_INVALID__");

        var salesAccount = await dbContext.Accounts
            .Where(x => x.Id == parentAccountId && x.Role == (int)AccountRoleTypes.Sales)
            .SingleOrDefaultAsync();
        if (salesAccount == null)
            return (false, "__SALES_ACCOUNT_NOT_FOUND__");

        var legacyReferPath = account.ReferPath;

        account.SalesAccountId = salesAccount.Id;
        account.ReferPath = salesAccount.ReferPath + "." + account.Uid;
        account.Level = salesAccount.Level + 1;
        // direct client should assign sales group code
        if (account.Role == (short)AccountRoleTypes.Client)
        {
            account.Group = salesAccount.Group;
        }

        account.AccountLogs.Add(Account.BuildLog(operatorPartyId, "ChangeSales", legacyReferPath, account.ReferPath));
        dbContext.Accounts.Update(account);
        await dbContext.SaveChangesAsync();

        var childAccounts = await dbContext.Accounts
            .Where(x => x.ReferPath.StartsWith(legacyReferPath) && x.Id != account.Id)
            .ToListAsync();

        foreach (var child in childAccounts)
        {
            var childLegacyReferPath = child.ReferPath;
            child.ReferPath = string.Concat(account.ReferPath, child.ReferPath.AsSpan(legacyReferPath.Length));
            child.Level = child.GetLevelFromReferPath();
            if (account.Role != (short)AccountRoleTypes.Sales) child.SalesAccountId = salesAccount.Id;
            child.AccountLogs.Add(Account.BuildLog(operatorPartyId, "ChangeSales", childLegacyReferPath,
                child.ReferPath));
            dbContext.Accounts.Update(child);
        }

        await dbContext.SaveChangesAsync();
        return (true, "");
    }

    public async Task<(bool, string)> ChangeAgentGroupAsync(long accountId, long agentAccountId, long operatorPartyId)
    {
        var account = await dbContext.Accounts
            .Where(x => x.Id == accountId)
            .SingleOrDefaultAsync();
        if (account == null)
            return (false, "__ACCOUNT_NOT_FOUND__");
        
        if (account.ReferPath.Length <= 8)
            return (false, "__ACCOUNT_REFER_PATH_INVALID__");

        var agentAccount = await dbContext.Accounts
            .Include(x => x.SalesAccount)
            .Where(x => x.Id == agentAccountId && x.Role == (int)AccountRoleTypes.Agent)
            .SingleOrDefaultAsync();
        if (agentAccount == null)
            return (false, "__AGENT_ACCOUNT_NOT_FOUND__");

        var legacyReferPath = account.ReferPath;

        account.AgentAccountId = agentAccount.Id;
        account.SalesAccountId = agentAccount.SalesAccountId;
        account.ReferPath = agentAccount.ReferPath + "." + account.Uid;
        account.Level = agentAccount.Level + 1;
        if (account.Role == (int)AccountRoleTypes.Client)
        {
            account.Group = agentAccount.Group;
            account.AccountLogs.Add(
                Account.BuildLog(operatorPartyId, "ChangeAgent", legacyReferPath, account.ReferPath));
            dbContext.Accounts.Update(account);
            await dbContext.SaveChangesAsync();
            return (true, "");
        }

        dbContext.Accounts.Update(account);
        await dbContext.SaveChangesAsync();
        foreach (var child in await dbContext.Accounts.Where(x => x.ReferPath.StartsWith(legacyReferPath))
                     .ToListAsync())
        {
            var childLegacyReferPath = child.ReferPath;
            child.ReferPath = string.Concat(account.ReferPath, child.ReferPath.AsSpan(legacyReferPath.Length));
            child.Level = child.GetLevelFromReferPath();
            child.SalesAccountId = agentAccount.SalesAccountId;
            // var afterJson = JsonConvert.SerializeObject(child);
            child.AccountLogs.Add(Account.BuildLog(operatorPartyId, "ChangeAgent", childLegacyReferPath,
                child.ReferPath));
            dbContext.Accounts.Update(child);
        }

        await dbContext.SaveChangesAsync();
        return (true, "");
    }

    public async Task<(bool, string)> RemoveFromAgentGroupAsync(long accountId)
    {
        var account = await dbContext.Accounts
            .Where(x => x.Id == accountId)
            .Where(x => x.Role == (int)AccountRoleTypes.Agent || x.Role == (int)AccountRoleTypes.Client)
            .Include(x => x.SalesAccount)
            .SingleOrDefaultAsync();
        if (account == null)
            return (false, "__ACCOUNT_NOT_FOUND__");
        if (account.AgentAccountId == null || account.AgentAccountId == account.Id)
            return (false, "__ACCOUNT_HAS_NO_AGENT__");

        var legacyReferPath = account.ReferPath;
        account.ReferPath = account.SalesAccount?.ReferPath + "." + account.Uid;
        account.Level = (account.SalesAccount?.Level ?? 0) + 1;
        account.AgentAccountId = null;
        dbContext.Accounts.Update(account);
        await dbContext.SaveChangesAsync();
        if (account.Role == (int)AccountRoleTypes.Client)
            return (true, "");

        foreach (var child in await dbContext.Accounts.Where(x => x.ReferPath.StartsWith(legacyReferPath))
                     .ToListAsync())
        {
            child.ReferPath = string.Concat(account.ReferPath, child.ReferPath.AsSpan(legacyReferPath.Length));
            dbContext.Accounts.Update(child);
        }

        return (await dbContext.SaveChangesAsync() > 0, "");
    }

}