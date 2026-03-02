using Bacera.Gateway.Agent;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.ViewModels.Parent;
using HashidsNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway;

public partial class TradingService
{
    private const int MaxCodeCount = 100;
    
    private async Task<(ReferralCode?, Result)> CreateAgentDefaultReferralCodeForClient(long accountId,
        long operatorPartyId, long tenantId, ReferralCode.AgentCreateSpec? agentCreateSpec = null)
    {
        var account = await dbContext.Accounts
            .Where(x => x.Id == accountId)
            .SingleAsync();

        var selfRebateRule = await dbContext.RebateAgentRules
            .Where(x => x.AgentAccountId == accountId)
            .ToResponseModel()
            .SingleAsync();

        var allowAccountTypes = selfRebateRule.Schema
            .Select(x => new RebateClientRule.RebateAllowedAccountTypes
            {
                OptionName = x.OptionName ?? "", AccountType = x.AccountType, Pips = x.Pips, Commission = x.Commission,
            })
            .ToList();

        var spec = new ReferralCode.ClientCreateSpec
        {
            Name = "Default Client Code",
            AllowAccountTypes = allowAccountTypes,
            DistributionType = agentCreateSpec?.DistributionType ?? RebateDistributionTypes.Allocation,
            Language = LanguageTypes.English,
            SiteId = (SiteTypes)account.SiteId
        };
        var item = new ReferralCode
        {
            Name = spec.Name,
            Code = Guid.NewGuid().ToString(),
            PartyId = account.PartyId,
            AccountId = account.Id,
            ServiceType = (int)ReferralServiceTypes.Client,
            Summary = Utils.JsonSerializeObject(spec),
        };

        dbContext.ReferralCodes.Add(item);
        await dbContext.SaveChangesWithAuditAsync(operatorPartyId);

        var hashids = new Hashids("BCRReferralCode", 3, "ABCDEFGHJKLMNOPQRSTUVWXYZ23456789");
        var code = hashids.Encode((int)item.Id);
        item.Code = "RAC" + code + code.Length + Tenancy.GetTenancyInReferCode(tenantId);
        dbContext.ReferralCodes.Update(item);
        await dbContext.SaveChangesAsync();
        centralDbContext.CentralReferralCodes.Add(item.ToCentralReferralCode(tenantId));
        await centralDbContext.SaveChangesAsync();
        return (item, Result.Success(""));
    }

    public async Task<(ReferralCode?, Result)> TryCreateReferralCodeForAgent(long agentUid
        , ReferralCode.AgentCreateSpec spec
        , AccountRoleTypes operatorAccountRole
        , long operatorPartyId
        , long tenantId
    )
    {
        var account = await dbContext.Accounts
            .Where(x => x.Uid == agentUid)
            .FirstOrDefaultAsync();
        if (account == null)
            return (null, Result.Error(ResultMessage.Common.AgentUidNotFound));

        if (account.Role != (int)AccountRoleTypes.Agent)
            return (null, Result.Error(ResultMessage.Account.AccountIsNotAnAgent));

        var selfRebateRule = await dbContext.RebateAgentRules
            .Where(x => x.AgentAccount.Uid == agentUid)
            .ToResponseModel()
            .FirstOrDefaultAsync();
        if (selfRebateRule == null) return (null, Result.Error(ResultMessage.RebateRule.RebateRuleSettingNotExists));

        // Check if the number of referral codes has reached the maximum for Party and Account
        if (MaxCodeCount <= await dbContext.ReferralCodes
                .Where(x => x.PartyId == account.PartyId)
                .Where(x => x.AccountId == account.Id)
                .CountAsync())
            return (null, Result.Error(ResultMessage.Referral.YouHaveReachedTheMaximumNumberOfReferralCodes));

        spec.SiteId = (SiteTypes)account.SiteId;
        if (spec.DistributionType == RebateDistributionTypes.Allocation)
        {
            var levelSetting = await GetCalculatedRebateLevelSetting(account);
            if (!account.IsTopLevelAgent()) // fulfill pips from LevelSettings of RebateAgentRule set by upper agent
            {
                FulFillPipsAndCommissionFromAgentRebateRule(spec.Schema, levelSetting.AllowedAccounts);
            }
        }
        else if (spec.DistributionType == RebateDistributionTypes.LevelPercentage)
        {
            // if (account.IsTopLevelAgent() && spec.PercentageSchema.PercentageSetting.Sum() != 100)
            //     return (null, Result.Error("The sum of the percentage must be 100"));
            // spec.PercentageSchema.PercentageSetting = spec.PercentageSchema.PercentageSetting.Select(x => Math.Round(x / 100, 2)).ToList();
            spec.Schema =
            [
                new RebateLevelSchema
                {
                    AccountType = spec.PercentageSchema.AccountType,
                    OptionName = spec.PercentageSchema.OptionName,
                    Pips = spec.PercentageSchema.GetPips()
                }
            ];
        }

        var item = new ReferralCode
        {
            Name = spec.Name,
            Code = Guid.NewGuid().ToString(),
            PartyId = account.PartyId,
            AccountId = account.Id,
            ServiceType = (int)ReferralServiceTypes.Agent,
            Summary = Utils.JsonSerializeObject(spec),
            IsAutoCreatePaymentMethod = spec.IsAutoCreatePaymentMethod
        };

        await dbContext.ReferralCodes.AddAsync(item);
        await dbContext.SaveChangesWithAuditAsync(operatorPartyId);

        var hashids = new Hashids("BCRReferralCode", 3, "ABCDEFGHJKLMNOPQRSTUVWXYZ23456789");
        var code = hashids.Encode((int)item.Id);

        var codePrefix = operatorAccountRole switch
        {
            AccountRoleTypes.Sales => "RSA",
            AccountRoleTypes.Agent => "RAA",
            _ => throw new ArgumentOutOfRangeException(nameof(operatorAccountRole), operatorAccountRole, null)
        };
        item.Code = codePrefix + code + code.Length + Tenancy.GetTenancyInReferCode(tenantId);
        dbContext.ReferralCodes.Update(item);
        await dbContext.SaveChangesWithAuditAsync(operatorPartyId);
        centralDbContext.CentralReferralCodes.Add(item.ToCentralReferralCode(tenantId));
        await centralDbContext.SaveChangesAsync();

        // If IsAutoCreatePaymentMethod is true, then
        // Create DefaultAutoCreatePaymentMethod configuration with RowId equals this referral code
        // Value copied from default site configuration (RowId == 0)
        if (spec.IsAutoCreatePaymentMethod == 1)
        {
            await CreateDefaultAutoCreatePaymentMethodConfigWithReferralCodeRowIdAsync(item.Id);
            await CreateDefaultAutoCreateWithdrawalPaymentMethodConfigWithReferralCodeRowIdAsync(item.Id);
        }
        
        return (item, Result.Success(""));
    }

    public async Task<(ReferralCode?, Result)> TryCreateReferralCodeForClient(long agentUid
        , ReferralCode.ClientCreateSpec spec
        , AccountRoleTypes operatorAccountRole
        , long operatorPartyId
        , long tenantId
    )
    {
        var account = await dbContext.Accounts.FirstAsync(x => x.Uid == agentUid);
        if (account.Role != (int)AccountRoleTypes.Agent)
            return (null, Result.Error(ResultMessage.Account.AccountIsNotAnAgent));

        var selfRebateRule = await dbContext.RebateAgentRules
            .Where(x => x.AgentAccount.Uid == agentUid)
            .ToResponseModel()
            .FirstOrDefaultAsync();
        if (selfRebateRule == null)
            return (null, Result.Error(ResultMessage.RebateRule.RebateRuleSettingNotExists));

        // Check if the number of referral codes has reached the maximum for Party and Account
        if (MaxCodeCount <= await dbContext.ReferralCodes
                .Where(x => x.PartyId == account.PartyId)
                .Where(x => x.AccountId == account.Id)
                .CountAsync())
            return (null, Result.Error(ResultMessage.Referral.YouHaveReachedTheMaximumNumberOfReferralCodes));

        spec.SiteId = (SiteTypes)account.SiteId;
        if (spec.DistributionType == RebateDistributionTypes.Allocation &&
            !account.IsTopLevelAgent()) // fulfill pips from LevelSettings of RebateAgentRule set by upper agent
        {
            FulFillPipsAndCommissionFromAgentRebateRule(spec.AllowAccountTypes, selfRebateRule.LevelSetting.AllowedAccounts);
        }
        else if (spec.DistributionType == RebateDistributionTypes.LevelPercentage)
        {
            spec.AllowAccountTypes =
            [
                new RebateClientRule.RebateAllowedAccountTypes
                {
                    AccountType = spec.PercentageSchema.AccountType,
                    OptionName = spec.PercentageSchema.OptionName,
                    Pips = spec.PercentageSchema.GetPips()
                }
            ];
        }
        
        var item = new ReferralCode
        {
            Name = spec.Name,
            Code = Guid.NewGuid().ToString(),
            PartyId = account.PartyId,
            AccountId = account.Id,
            ServiceType = (int)ReferralServiceTypes.Client,
            Summary = Utils.JsonSerializeObject(spec),
            IsAutoCreatePaymentMethod = spec.IsAutoCreatePaymentMethod
        };

        await dbContext.ReferralCodes.AddAsync(item);
        await dbContext.SaveChangesAsync();

        var hashids = new Hashids("BCRReferralCode", 3, "ABCDEFGHJKLMNOPQRSTUVWXYZ23456789");
        var code = hashids.Encode((int)item.Id);

        var codePrefix = operatorAccountRole switch
        {
            AccountRoleTypes.Sales => "RSC",
            AccountRoleTypes.Agent => "RAC",
            _ => throw new ArgumentOutOfRangeException(nameof(operatorAccountRole), operatorAccountRole, null)
        };

        item.Code = codePrefix + code + code.Length + Tenancy.GetTenancyInReferCode(tenantId);
        dbContext.ReferralCodes.Update(item);
        await dbContext.SaveChangesWithAuditAsync(operatorPartyId);
        centralDbContext.CentralReferralCodes.Add(item.ToCentralReferralCode(tenantId));
        await centralDbContext.SaveChangesAsync();

        // If IsAutoCreatePaymentMethod is true, then
        // Create DefaultAutoCreatePaymentMethod configuration with RowId equals this referral code
        // Value copied from default site configuration (RowId == 0)
        await CreateDefaultAutoCreatePaymentMethodConfigWithReferralCodeRowIdAsync(item.Id);
        await CreateDefaultAutoCreateWithdrawalPaymentMethodConfigWithReferralCodeRowIdAsync(item.Id);
        
        return (item, Result.Success(""));
    }

    /// <summary>
    /// Creates DefaultAutoCreatePaymentMethod configuration for a referral code
    /// Copies the value from the default site configuration (RowId == 0)
    /// </summary>
    public async Task CreateDefaultAutoCreatePaymentMethodConfigWithReferralCodeRowIdAsync(long referralCodeId)
    {
        try
        {
            // Get default site configuration (RowId == 0) - now supports multiple payment method IDs
            var defaultConfig = await configSvc.GetAsync<List<long>>(
                ConfigCategoryTypes.Public, 
                0, 
                ConfigKeys.DefaultAutoCreatePaymentMethod);
            
            // If default config exists and has values, create the same config for this referral code
            if (defaultConfig != null && defaultConfig.Count > 0)
            {
                await configSvc.SetAsync<List<long>>(
                    ConfigCategoryTypes.Public,
                    referralCodeId,
                    ConfigKeys.DefaultAutoCreatePaymentMethod,
                    defaultConfig,
                    partyId: 1);
            }
            else
            {
                _logger.LogWarning(
                    "No default configuration found for DefaultAutoCreatePaymentMethod. Skipping configuration creation for ReferralCode {ReferralCodeId}",
                    referralCodeId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create DefaultAutoCreatePaymentMethod configuration for referral code {ReferralCodeId}. Referral code creation will continue. Error: {ErrorMessage}", 
                referralCodeId,
                ex.Message);
            // Don't fail referral code creation if config creation fails
        }
    }

    /// <summary>
    /// Creates DefaultAutoCreateWithdrawalPaymentMethod configuration for a referral code
    /// Copies the value from the default site configuration (RowId == 0)
    /// </summary>
    public async Task CreateDefaultAutoCreateWithdrawalPaymentMethodConfigWithReferralCodeRowIdAsync(long referralCodeId)
    {
        try
        {
            // Get default site configuration (RowId == 0) - now supports multiple payment method IDs
            var defaultConfig = await configSvc.GetAsync<List<long>>(
                ConfigCategoryTypes.Public, 
                0, 
                ConfigKeys.DefaultAutoCreateWithdrawalPaymentMethod);
            
            // If default config exists and has values, create the same config for this referral code
            if (defaultConfig != null && defaultConfig.Count > 0)
            {
                await configSvc.SetAsync<List<long>>(
                    ConfigCategoryTypes.Public,
                    referralCodeId,
                    ConfigKeys.DefaultAutoCreateWithdrawalPaymentMethod,
                    defaultConfig,
                    partyId: 1);
            }
            else
            {
                _logger.LogWarning(
                    "No default configuration found for DefaultAutoCreateWithdrawalPaymentMethod. Skipping configuration creation for ReferralCode {ReferralCodeId}",
                    referralCodeId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create DefaultAutoCreateWithdrawalPaymentMethod configuration for referral code {ReferralCodeId}. Referral code creation will continue. Error: {ErrorMessage}", 
                referralCodeId,
                ex.Message);
            // Don't fail referral code creation if config creation fails
        }
    }

    public async Task<Result<List<ReferredUserBasicInfoForAgentViewModel>, Referral.Criteria>>
        QueryUnfinishedReferredUserAsync(long parentUid, Referral.Criteria criteria)
    {
        var account = await dbContext.Accounts
            .Where(x => x.Uid == parentUid)
            .Select(x => new { x.Id, x.PartyId })
            .SingleOrDefaultAsync();
        if (account == null) return new Result<List<ReferredUserBasicInfoForAgentViewModel>, Referral.Criteria>();
        criteria.ReferrerPartyId = account.PartyId;
        criteria.ReferrerAccountId = account.Id;
        criteria.Module = nameof(Auth.User);

        var items = await dbContext.Referrals
            .Where(x => x.ReferredParty.Status == (int)PartyStatusTypes.Active)
            .PagedFilterBy(criteria)
            .ToAgentViewModel()
            .ToListAsync();

        return Result<List<ReferredUserBasicInfoForAgentViewModel>, Referral.Criteria>.Of(items, criteria);
    }

    private static void FulFillPipsAndCommissionFromAgentRebateRule(List<RebateLevelSchema> updateSchemas,
        IReadOnlyCollection<RebateLevelSchema> allowedAccounts)
    {
        foreach (var schema in updateSchemas)
        {
            var allowedAccount = allowedAccounts.FirstOrDefault(x => x.AccountType == schema.AccountType);

            if (allowedAccount == null)
                continue;

            schema.Pips = allowedAccount.Pips;
            schema.Commission = allowedAccount.Commission;
        }
    }

    private static void FulFillPipsAndCommissionFromAgentRebateRule(
        List<RebateClientRule.RebateAllowedAccountTypes> allowedAccountTypes
        , IReadOnlyCollection<RebateLevelSchema> allowedAccounts)
    {
        foreach (var rebateAllowedAccountTypes in allowedAccountTypes)
        {
            var allowedAccount =
                allowedAccounts.FirstOrDefault(x => x.AccountType == rebateAllowedAccountTypes.AccountType);

            if (allowedAccount == null)
                continue;

            rebateAllowedAccountTypes.Pips = allowedAccount.Pips;
            rebateAllowedAccountTypes.Commission = allowedAccount.Commission;
        }
    }
}
//
// private async Task<(ReferralCode?, Result)> CreateClientDefaultReferralCodeForClient(long accountId,
//     long operatorPartyId, long tenantId)
// {
//     var account = await _ctx.Accounts
//         .Where(x => x.Id == accountId)
//         .SingleAsync();
//     var user = await _authCtx.Users
//         .Where(x => x.PartyId == account.PartyId && x.TenantId == _tenancy.GetTenantId())
//         .SingleAsync();
//
//     var spec = new
//     {
//         Name = "Event refer Code",
//         user.Language,
//         SiteId = (SiteTypes)account.SiteId
//     };
//     var item = new ReferralCode
//     {
//         Name = "Event refer Code",
//         Code = Guid.NewGuid().ToString(),
//         PartyId = account.PartyId,
//         AccountId = account.Id,
//         ServiceType = (int)ReferralServiceTypes.Client,
//         Summary = Utils.JsonSerializeObject(spec),
//     };
//
//     await _ctx.ReferralCodes.AddAsync(item);
//     await _ctx.SaveChangesWithAuditAsync(operatorPartyId);
//
//     var hashids = new Hashids("BCRReferralCode", 3, "ABCDEFGHJKLMNOPQRSTUVWXYZ23456789");
//     var code = hashids.Encode((int)item.Id);
//     item.Code = "VCC" + code + code.Length + TenancyResolver.GetTenancyInReferCode(tenantId);
//     await _ctx.SaveChangesAsync();
//     _centralCtx.CentralReferralCodes.Add(item.ToCentralReferralCode(tenantId));
//     await _centralCtx.SaveChangesAsync();
//     return (item, Result.Success(""));
// }