using System.Security.Claims;
using System.Text;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class TradingService
{
    private async Task<bool> AccountNumberAndUidValidator(long uid)
    {
        var result = await dbContext.Accounts.AnyAsync(x => x.Uid == uid || x.AccountNumber == uid);
        if (result) return true;
        return await centralDbContext.CentralAccounts.AnyAsync(x => x.Uid == uid || x.AccountNumber == uid);
    }

    // public async Task<(bool, string)> CreateClientAccountAsync(long partyId, long parentAccountId,
    //     CurrencyTypes currencyId, FundTypes fundType, AccountTypes? accountType = AccountTypes.Standard,
    //     SiteTypes? siteId = null, bool hasTradeAccount = false, string? referCode = "", long operatorPartyId = 1,
    //     long tenantId = 0)
    // {
    //     var user = authDbContext.Users.FirstOrDefault(x => x.PartyId == partyId && x.TenantId == tenantId);
    //     if (user == null)
    //         return (false, ResultMessage.User.UserNotFound);
    //
    //     var parentAccount = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == parentAccountId);
    //     if (parentAccount == null)
    //         return (false, ResultMessage.Account.ParentAccountNotFound);
    //
    //     ReferralCode? referralCode = null;
    //     if (!string.IsNullOrEmpty(referCode))
    //     {
    //         referralCode = await dbContext.ReferralCodes
    //             .Where(x => x.Code == referCode)
    //             .FirstOrDefaultAsync();
    //
    //         if (referralCode == null)
    //         {
    //             return (false, ResultMessage.Referral.ReferralCodeNotExist);
    //         }
    //     }
    //
    //     var accountUid = await Utils.GenerateUniqueIdAsync(AccountNumberAndUidValidator);
    //     // var accountNumber = await GenerateMt4AccountNumber()
    //     var account = new Account
    //     {
    //         Role = (short)AccountRoleTypes.Client,
    //         PartyId = partyId,
    //         Uid = accountUid,
    //         ReferPath = $"{parentAccount.ReferPath}.{accountUid}",
    //         Level = parentAccount.Level + 1,
    //         Type = (short)accountType!,
    //         Group = parentAccount.Group,
    //         Code = string.Empty,
    //         SalesAccountId = parentAccount.Role == (short)AccountRoleTypes.Sales
    //             ? parentAccount.Id
    //             : parentAccount.SalesAccountId,
    //         AgentAccountId = parentAccount.Role == (short)AccountRoleTypes.Agent ? parentAccount.Id : null,
    //         Name = user.GuessUserName(),
    //         CurrencyId = (short)currencyId,
    //         FundType = (short)fundType,
    //         HasTradeAccount = hasTradeAccount,
    //         HasLevelRule = (int)HasLevelRuleTypes.HasNoLevelRule,
    //         SiteId = siteId == null ? parentAccount.SiteId : (int)siteId,
    //         SearchText = string.Empty,
    //         CreatedOn = DateTime.UtcNow,
    //         UpdatedOn = DateTime.UtcNow,
    //     };
    //
    //     dbContext.Accounts.Add(account);
    //     await dbContext.SaveChangesAsync();
    //     centralDbContext.CentralAccounts.Add(account.ToCentralAccount(tenantId));
    //     await centralDbContext.SaveChangesAsync();
    //
    //     // assign rebate client rule
    //     referralCode ??= await dbContext.ReferralCodes
    //         .Where(x => x.AccountId == parentAccountId)
    //         .Where(x => x.IsDefault == 1)
    //         .FirstOrDefaultAsync();
    //
    //     var distributionType = RebateDistributionTypes.Direct;
    //     if (referralCode != null)
    //     {
    //         var refHistory = Referral.Build(referralCode.Id, referralCode.PartyId, account.PartyId,
    //             referralCode.Code, nameof(Account), account.Id);
    //         await dbContext.Referrals.AddAsync(refHistory);
    //         await dbContext.SaveChangesAsync();
    //
    //         account.ReferCode = referralCode.Code;
    //
    //         var spec = referralCode.TryGetClientCreateSpec();
    //         if (siteId == null && spec?.SiteId != null)
    //         {
    //             account.SiteId = (int)spec.SiteId;
    //         }
    //
    //         var schema = spec?.AllowAccountTypes;
    //         if (schema != null)
    //         {
    //             distributionType = RebateDistributionTypes.Allocation;
    //         }
    //         
    //         if (spec?.DistributionType == RebateDistributionTypes.LevelPercentage)
    //         {
    //             distributionType = RebateDistributionTypes.LevelPercentage;
    //         }
    //     }
    //
    //     var clientRule = new RebateClientRule
    //     {
    //         Schema = "{}",
    //         ClientAccountId = account.Id,
    //         DistributionType = (short)distributionType,
    //     };
    //
    //     dbContext.Accounts.Update(account);
    //     dbContext.RebateClientRules.Add(clientRule);
    //     await dbContext.SaveChangesAsync();
    //
    //     // await CreateClientDefaultReferralCodeForClient(account.Id, operatorPartyId, tenantId);
    //     return (true, account.Id.ToString());
    // }

    public async Task<(bool, string)> CreateAgentAccountAsync(long partyId, string group, long parentAccountId,
        CurrencyTypes currencyId, FundTypes fundType, AccountTypes? accountType = AccountTypes.Standard,
        SiteTypes? siteId = null, bool hasTradeAccount = false, string? referCode = "", long operatorPartyId = 1,
        long tenantId = 0)
    {
        var user = authDbContext.Users.FirstOrDefault(x => x.TenantId == tenantId && x.PartyId == partyId);
        if (user == null)
            return (false, ResultMessage.User.UserNotFound);

        var parentAccount = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == parentAccountId);
        if (parentAccount == null)
            return (false, ResultMessage.Account.ParentAccountNotFound);

        var isGroupNameExist = await dbContext.Accounts
            .Where(x => x.Role == (short)AccountRoleTypes.Agent || x.Role == (short)AccountRoleTypes.Sales)
            .Where(x => x.Group == group)
            .AnyAsync();

        if (isGroupNameExist)
        {
            return (false, ResultMessage.Account.GroupNameAlreadyExist);
        }

        var isTopAgent = parentAccount.Role == (short)AccountRoleTypes.Sales;
        if (!isTopAgent)
        {
            var parentRebateAgentRule =
                await dbContext.RebateAgentRules.FirstOrDefaultAsync(x => x.AgentAccountId == parentAccountId);
            if (parentRebateAgentRule == null)
            {
                return (false, ResultMessage.RebateRule.ParentAgentRuleNotExist);
            }
        }
        else if (string.IsNullOrEmpty(referCode))
        {
            return (false, "__TOP_AGENT_MUST_HAVE_REFERRAL_CODE__");
        }

        ReferralCode? referralCode = null;
        if (!string.IsNullOrEmpty(referCode))
        {
            referralCode = await dbContext.ReferralCodes
                .Where(x => x.AccountId == parentAccountId)
                .Where(x => x.Code == referCode)
                .FirstOrDefaultAsync();

            if (referralCode == null)
            {
                return (false, ResultMessage.Referral.ReferralCodeNotExist);
            }
        }

        var accountUid = await Utils.GenerateUniqueIdAsync(AccountNumberAndUidValidator);

        var account = new Account
        {
            Role = (short)AccountRoleTypes.Agent,
            PartyId = partyId,
            Uid = accountUid,
            ReferPath = $"{parentAccount.ReferPath}.{accountUid}",
            Level = parentAccount.Level + 1,
            Type = (short)accountType!,
            Group = group,
            Code = string.Empty,
            SalesAccountId = isTopAgent ? parentAccount.Id : parentAccount.SalesAccountId,
            Name = user.GuessUserName(),
            CurrencyId = (short)currencyId,
            FundType = (short)fundType,
            HasTradeAccount = hasTradeAccount,
            HasLevelRule = (int)HasLevelRuleTypes.HasLevelRule,
            SiteId = siteId == null ? parentAccount.SiteId : (int)siteId,
            SearchText = string.Empty,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };

        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();
        centralDbContext.CentralAccounts.Add(account.ToCentralAccount(tenantId));
        await centralDbContext.SaveChangesAsync();

        account.AgentAccountId = isTopAgent ? null : parentAccount.Id;

        // assign rebate client rule
        referralCode ??= await dbContext.ReferralCodes
            .Where(x => x.AccountId == parentAccountId)
            .Where(x => x.IsDefault == 1)
            .FirstOrDefaultAsync();

        // RebateAgentRule.RebateLevelSetting? rebateLevelSetting = null;
        ReferralCode.AgentCreateSpec? agentCreateSpec = null;
        if (referralCode != null)
        {
            var refHistory = Referral.Build(referralCode.Id, referralCode.PartyId, account.PartyId, referralCode.Code,
                nameof(Account), account.Id);
            await dbContext.Referrals.AddAsync(refHistory);
            await dbContext.SaveChangesAsync();
            account.ReferCode = referralCode.Code;
            agentCreateSpec = referralCode.TryGetAgentCreateSpec();
        }

        if (siteId == null && agentCreateSpec?.SiteId != null)
        {
            account.SiteId = (int)agentCreateSpec.SiteId;
        }

        var rebateLevelSchema = new List<RebateLevelSchema>();
        var levelSetting = new RebateAgentRule.RebateLevelSetting
            { Language = agentCreateSpec?.Language ?? LanguageTypes.English };
        if (agentCreateSpec != null)
        {
            if (agentCreateSpec.DistributionType == RebateDistributionTypes.Allocation)
            {
                rebateLevelSchema = agentCreateSpec.Schema;
                if (isTopAgent)
                {
                    levelSetting.AllowedAccounts = rebateLevelSchema;
                }
            }
            else if (agentCreateSpec.DistributionType == RebateDistributionTypes.LevelPercentage)
            {
                rebateLevelSchema = agentCreateSpec.Schema;
                levelSetting.DistributionType = RebateDistributionTypes.LevelPercentage;
                levelSetting.PercentageSetting = agentCreateSpec.PercentageSchema.PercentageSetting;
            }
            else
            {
                levelSetting = new RebateAgentRule.RebateLevelSetting();
            }
        }
        else
        {
            var levelSettingFromParents = await FindRebateLevelSettingsFromUpperLevelAgent(account);
            rebateLevelSchema = levelSettingFromParents.AllowedAccounts;
        }

        var rule = new RebateAgentRule
        {
            AgentAccountId = account.Id,
            ParentId = null,
            Schema = JsonConvert.SerializeObject(rebateLevelSchema),
            LevelSetting = agentCreateSpec?.DistributionType == RebateDistributionTypes.LevelPercentage || isTopAgent
                ? JsonConvert.SerializeObject(levelSetting)
                : "{}",
        };

        dbContext.RebateAgentRules.Add(rule);
        dbContext.Accounts.Update(account);
        await dbContext.SaveChangesAsync();
        // if (!isTopAgent)
        // {
        //     await UpdateRebateRuleLevelSettingRemainByAgentId(account.Id);
        // }

        await CreateAgentDefaultReferralCodeForClient(account.Id, operatorPartyId, tenantId, agentCreateSpec);
        if (agentCreateSpec?.DistributionType == RebateDistributionTypes.LevelPercentage && !isTopAgent)
        {
            var spec = new ReferralCode.AgentCreateSpec
            {
                Name = "Default IB Code",
                Language = agentCreateSpec.Language,
                DistributionType = RebateDistributionTypes.LevelPercentage,
                PercentageSchema = agentCreateSpec.PercentageSchema,
                Schema = [],
                SiteId = agentCreateSpec.SiteId,
            };
            await TryCreateReferralCodeForAgent(account.Uid, spec, AccountRoleTypes.Agent, operatorPartyId, _tenantId);
        }

        return (true, account.Id.ToString());
    }

    public async Task<(bool, string)> CreateSalesAccountAsync(long partyId, string group, string code,
        long parentAccountId, CurrencyTypes currencyId, long tenantId = 0)
    {
        var user = authDbContext.Users.FirstOrDefault(x => x.PartyId == partyId && x.TenantId == tenantId);
        if (user == null)
            return (false, ResultMessage.User.UserNotFound);

        var parentAccount = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == parentAccountId);
        if (parentAccount == null)
            return (false, ResultMessage.Account.ParentAccountNotFound);

        var isGroupNameExist = await dbContext.Accounts
            .Where(x => x.Role == (short)AccountRoleTypes.Agent || x.Role == (short)AccountRoleTypes.Sales)
            .Where(x => x.Group == group)
            .AnyAsync();

        if (isGroupNameExist)
        {
            return (false, ResultMessage.Account.GroupNameAlreadyExist);
        }

        var isCodeExist = await dbContext.Accounts
            .Where(x => x.Role == (short)AccountRoleTypes.Sales)
            .Where(x => x.Code == code)
            .AnyAsync();

        if (isCodeExist)
        {
            return (false, ResultMessage.Account.CodeAlreadyExist);
        }

        var accountUid = await Utils.GenerateUniqueIdAsync(AccountNumberAndUidValidator);
        var accountReferPath = $"{parentAccount.ReferPath}.{accountUid}";
        var level = parentAccount.Level + 1;

        var account = new Account
        {
            Role = (short)AccountRoleTypes.Sales,
            PartyId = partyId,
            Uid = accountUid,
            ReferPath = accountReferPath,
            Level = level,
            Type = (short)AccountTypes.Standard,
            Group = group,
            Code = code,
            SalesAccountId = null,
            AgentAccountId = null,
            Name = user.GuessUserName(),
            CurrencyId = (short)currencyId,
            FundType = (short)FundTypes.Wire,
            HasTradeAccount = false,
            HasLevelRule = (int)HasLevelRuleTypes.HasNoLevelRule,
            SiteId = parentAccount.SiteId,
            SearchText = string.Empty,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };

        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();
        centralDbContext.CentralAccounts.Add(account.ToCentralAccount(tenantId));
        await centralDbContext.SaveChangesAsync();

        account.SalesAccountId = account.Id;

        dbContext.Accounts.Update(account);
        await dbContext.SaveChangesAsync();

        return (true, account.Id.ToString());
    }

    public async Task<(bool, string)> CreateRepAccountAsync(long partyId, string group, string code, long tenantId = 0)
    {
        var user = authDbContext.Users.FirstOrDefault(x => x.PartyId == partyId && x.TenantId == tenantId);
        if (user == null)
            return (false, ResultMessage.User.UserNotFound);

        var isGroupNameExist = await dbContext.Accounts
            .Where(x => x.Role == (short)AccountRoleTypes.Agent || x.Role == (short)AccountRoleTypes.Sales)
            .Where(x => x.Group == group)
            .AnyAsync();

        if (isGroupNameExist)
        {
            return (false, ResultMessage.Account.GroupNameAlreadyExist);
        }

        var isCodeExist = await dbContext.Accounts
            .Where(x => x.Role == (short)AccountRoleTypes.Sales)
            .Where(x => x.Code == code)
            .AnyAsync();

        if (isCodeExist)
        {
            return (false, ResultMessage.Account.CodeAlreadyExist);
        }

        var accountUid = await Utils.GenerateUniqueIdAsync(AccountNumberAndUidValidator);

        var accountReferPath = $".{accountUid}";
        const int level = 1;

        var account = new Account
        {
            Role = (short)AccountRoleTypes.Rep,
            PartyId = partyId,
            Uid = accountUid,
            ReferPath = accountReferPath,
            Level = level,
            Type = (short)AccountTypes.Standard,
            Group = group,
            Code = code,
            SalesAccountId = null,
            AgentAccountId = null,
            Name = user.GuessUserName(),
            CurrencyId = 840,
            FundType = (short)FundTypes.Wire,
            HasTradeAccount = false,
            HasLevelRule = (int)HasLevelRuleTypes.HasNoLevelRule,
            SiteId = 1,
            SearchText = string.Empty,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        };

        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();
        centralDbContext.CentralAccounts.Add(account.ToCentralAccount(tenantId));
        await centralDbContext.SaveChangesAsync();

        return (true, account.Id.ToString());
    }


    /// <summary>
    /// Find what an agent account can play
    /// Can only be used for legacy data, make sure top Ib at least has the level settings !!!!!!!!
    /// </summary>
    /// <param name="account"></param>
    /// <returns></returns>
    private async Task<RebateAgentRule.RebateLevelSetting> FindRebateLevelSettingsFromUpperLevelAgent(Account account)
    {
        var referPathUids = account.ReferPathUids;
        var parentAgentUids = referPathUids.Skip(2).Take(referPathUids.Count - 2).ToList();

        var accounts = await dbContext.Accounts
            .Where(x => parentAgentUids.Contains(x.Uid))
            .Select(x => new { x.Uid, x.Id })
            .ToListAsync();

        var parentAgentIds = parentAgentUids.Select(uid => accounts.Single(x => x.Uid == uid).Id).ToList();

        var rebateAgentRules = await dbContext.RebateAgentRules
            .Where(x => parentAgentIds.Contains(x.AgentAccountId))
            .Select(x => x.ToResponseModel())
            .ToListAsync();

        var validRebateLevelSetting = parentAgentIds
            .Select(x => rebateAgentRules.SingleOrDefault(r => r.AgentAccountId == x))
            .Where(x => x != null)
            .Where(x => x!.LevelSetting.AllowedAccounts.Any(s => !s.IsEmpty()))
            .Select(x => x!.LevelSetting)
            .Last();

        return validRebateLevelSetting;
    }

    // public async Task<long> UpdateAccountSearchText(Account.Criteria criteria)
    // {
    //     criteria.Page = 1;
    //     criteria.Size = 1000;
    //     criteria.SortField = "Id";
    //     criteria.SortFlag = false;
    //     criteria.IncludeClosed = true;
    //     var key = CacheKeys.GetPartyIdByAccountIdHashKey(_tenantId);
    //     var total = 0;
    //     while (true)
    //     {
    //         var accounts = await dbContext.Accounts
    //             .Include(x => x.Party)
    //             .PagedFilterBy(criteria)
    //             .ToListAsync();
    //
    //         if (accounts.Count == 0)
    //             break;
    //
    //         var wallets = await dbContext.Wallets
    //             .Where(x => accounts.Select(y => y.PartyId).Contains(x.PartyId))
    //             .Select(x => new { x.PartyId, x.FundType, x.CurrencyId, x.Id })
    //             .ToListAsync();
    //
    //         criteria.Page++;
    //         foreach (var account in accounts)
    //         {
    //             var wallet = wallets
    //                 .Where(x => x.Id == account.PartyId)
    //                 .FirstOrDefault(x => x.CurrencyId == account.CurrencyId && x.FundType == account.FundType);
    //
    //             var extraText = wallet == null ? "" : $"{wallet.Id},";
    //             account.UpdateSearchText(extraText);
    //             account.Party.UpdateSearchText();
    //             dbContext.Accounts.Update(account);
    //             dbContext.Parties.Update(account.Party);
    //             await myCache.HSetDeleteByKeyFieldAsync(key, account.Id.ToString());
    //             total++;
    //         }
    //
    //         await dbContext.SaveChangesAsync();
    //     }
    //
    //     return total;
    // }


    public async Task<bool> IsParentAccountBelongToParty(long partyId, long parentAccountUid)
    {
        var hashKey = CacheKeys.GetPartyAccountUidsHashKey(tenancy.GetTenantId());
        var value = await myCache.HGetStringAsync(hashKey, partyId.ToString());
        if (value == parentAccountUid.ToString())
            return true;

        var exist = await dbContext.Accounts
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Uid == parentAccountUid)
            .AnyAsync();
        if (!exist) return false;

        await myCache.HSetStringAsync(hashKey, partyId.ToString(), parentAccountUid.ToString(), TimeSpan.FromDays(1));
        return true;
    }
}