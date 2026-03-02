using System.Text;
using Bacera.Gateway.Connection;
using Bacera.Gateway.Context;
using Bacera.Gateway.ViewModels.Parent;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway.Services.AccountManage;

public partial class AccountManageService(
    TenantDbContext tenantCtx,
    CentralDbContext centralCtx,
    TenantDbConnection tenantDbCon,
    CentralDbConnection centralDbCon,
    IMyCache cache,
    ITenantGetter getter,
    MyDbContextPool pool,
    TagService tagSvc,
    ITradingApiService apiSvc,
    IServiceProvider provider,
    ILogger<AccountManageService> logger,
    ReferralCodeService referralCodeSvc,
    UserService userSvc,
    ConfigService configSvc)
{
    private readonly long _tenantId = getter.GetTenantId();

    public Task<bool> AccountExistByIdAsync(long accountId, long? partyId = null)
        => QueryById(accountId).Where(x => partyId == null || x.PartyId == partyId).AnyAsync();

    public Task<bool> AccountHasCompleteDepositByIdAsync(long accountId) =>
        tenantCtx.Deposits.AnyAsync(x =>
            x.TargetAccountId == accountId && x.IdNavigation.StateId == (int)StateTypes.DepositCompleted);

    public Task<long> DemoAccountCountAsync(long partyId)
        => tenantCtx.TradeDemoAccounts
            .Where(x => x.PartyId == partyId && x.ExpireOn > DateTime.UtcNow)
            .LongCountAsync();

    public Task<long> AccountCreatedDepositCountAsync(long accountId)
        => tenantCtx.Deposits
            .Where(x => x.TargetAccountId == accountId)
            .Where(x => x.IdNavigation.StateId == (int)StateTypes.DepositCreated)
            .LongCountAsync();

    public Task<CurrencyTypes> GetAccountCurrencyIdAsync(long accountId)
        => QueryById(accountId).Select(x => (CurrencyTypes)x.CurrencyId).SingleAsync();

    public async Task<long> GetAccountIdByUidAsync(long uid)
    {
        var key = CacheKeys.GetAccountUidToIdHashKey(_tenantId);
        var value = await cache.HGetStringAsync(key, uid.ToString());
        if (value != null) return long.Parse(value);

        var id = await tenantCtx.Accounts
            .Where(x => x.Uid == uid)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();
        if (id == 0) return 0;

        await cache.HSetStringAsync(key, uid.ToString(), id.ToString(), TimeSpan.FromDays(7));
        return id;
    }

    public async Task<bool> IsAccountBelongToPartyAsync(long partyId, long accountId, bool includeExtraRelation = false)
    {
        var hKey = CacheKeys.GetPartyIdByAccountIdHashKey(_tenantId);
        var value = await cache.HGetStringAsync(hKey, accountId.ToString());
        if (value != null) return long.Parse(value) == partyId;

        var extraAccountKey = CacheKeys.GetPartyExtraRelationAccountKey(_tenantId, partyId, accountId);
        bool extraResult;
        if (includeExtraRelation)
        {
            extraResult = await cache.GetStringAsync(extraAccountKey) == "1";
            if (extraResult) return true;
        }

        var result = await tenantCtx.Accounts.Where(x => x.PartyId == partyId && x.Id == accountId).AnyAsync();
        if (result)
        {
            await cache.HSetStringAsync(hKey, accountId.ToString(), partyId.ToString(), TimeSpan.FromDays(7));
            return true;
        }

        if (!includeExtraRelation) return false;

        var accountIds = await tenantCtx.Accounts
            .Where(x => x.PartyId == partyId && x.Status == 0)
            .Select(x => x.Id)
            .ToListAsync();

        extraResult = await tenantCtx.AccountExtraRelations
            .Where(x => accountIds.Contains(x.ParentAccountId) && x.ChildAccountId == accountId)
            .AnyAsync();

        if (!extraResult) return false;

        await cache.SetStringAsync(extraAccountKey, "1", TimeSpan.FromDays(1));
        return true;
    }

    public Task<bool> IsAccountBelongToParentAsync(long parentUid, long childUid) =>
        tenantCtx.Accounts.AnyAsync(x => x.ReferPath.Contains(childUid.ToString()));

    public async Task<long> GetPartyIdByAccountIdAsync(long accountId, bool fromDb = false)
    {
        if (fromDb) return await QueryById(accountId).Select(x => x.PartyId).SingleOrDefaultAsync();

        var hKey = CacheKeys.GetPartyIdByAccountIdHashKey(_tenantId);
        var value = await cache.HGetStringAsync(hKey, accountId.ToString());
        if (value != null) return long.Parse(value);

        var partyId = await QueryById(accountId).Select(x => x.PartyId).SingleOrDefaultAsync();
        if (partyId == 0) return 0;

        await cache.HSetStringAsync(hKey, accountId.ToString(), partyId.ToString(), TimeSpan.FromDays(7));
        return partyId;
    }


    public async Task<string?> GetUserDefaultSelfReferCodeAsync(long partyId)
    {
            
        var isMLM = await userSvc.HasRoleAsync(partyId, UserRoleTypesString.MLM);
        long agentAccountId;
        if (isMLM)
        {
            agentAccountId = await tenantCtx.Accounts
                .Where(x => x.PartyId == partyId && x.Role == (int)AccountRoleTypes.Agent)
                .Where(x => x.AgentAccountId != null)
                .OrderBy(x => x.Id)
                .Select(x => x.AgentAccountId!.Value)
                .FirstOrDefaultAsync();
        }
        else
        {
            agentAccountId = await tenantCtx.Accounts
                .Where(x => x.PartyId == partyId && x.Role == (int)AccountRoleTypes.Agent)
                .OrderBy(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            if (agentAccountId == 0)
            {
                var party = await userSvc.GetPartyAsync(partyId);
                if (!string.IsNullOrEmpty(party.ReferCode)) return party.ReferCode;
                
                agentAccountId = await tenantCtx.Accounts
                    .Where(x => x.PartyId == partyId && x.Role == (int)AccountRoleTypes.Client)
                    .Where(x => x.AgentAccountId != null)
                    .OrderBy(x => x.Id)
                    .Select(x => x.AgentAccountId!.Value)
                    .FirstOrDefaultAsync();
            }
        }

        if (agentAccountId == 0) return null;

        var code = await tenantCtx.ReferralCodes
                       .Where(x => x.ServiceType == (int)ReferralServiceTypes.Client)
                       .Where(x => x.AccountId == agentAccountId && x.IsDefault == 1)
                       .OrderBy(x => x.Id)
                       .Select(x => x.Code)
                       .FirstOrDefaultAsync()
                   ?? await tenantCtx.ReferralCodes
                       .Where(x => x.ServiceType == (int)ReferralServiceTypes.Client)
                       .Where(x => x.AccountId == agentAccountId)
                       .OrderBy(x => x.Id)
                       .Select(x => x.Code)
                       .FirstOrDefaultAsync();
        return code;
    }

    public async Task<List<ParentLevelAccountViewModel>> GetParentLevelAccountAsync(long parentUid, long childUid)
    {
        if (parentUid == childUid) return [];

        var accounts = await tenantCtx.Accounts
            .Where(x => x.Uid == childUid || x.Uid == parentUid)
            .Select(x => new { x.Level, x.ReferPath, x.Uid })
            .ToListAsync();
        var parent = accounts.First(x => x.Uid == parentUid);
        var childReferPath = accounts.First(x => x.Uid == childUid).ReferPath;

        var startIndex = childReferPath.IndexOf(parent.Uid.ToString(), StringComparison.Ordinal) +
                         parent.Uid.ToString().Length + 1;
        var path = childReferPath[startIndex..];
        var uids = path.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList();

        var items = await tenantCtx.Accounts
            .Where(x => uids.Contains(x.Uid))
            .OrderBy(x => x.Level)
            .ToParentLevelViewModel(parent.Level)
            .ToListAsync();

        return items;
    }

    public async Task<List<Account.ClientPageModel>> GetIncompleteAccountForPartyAsync(long partyId)
    {
        var items = await tenantDbCon.ToListAsync<Account.ClientPageModel>(
            $"""
             select cast("Supplement" ->> 'currencyId' as int)  as "CurrencyId",
                    cast("Supplement" ->> 'fundType' as int)    as "FundType",
                    cast("Supplement" ->> 'role' as smallint)   as "Role",
                    cast("Supplement" ->> 'serviceId' as int)   as "ServiceId",
                    -1                                          as "Status",
                    cast("Supplement" ->> 'accountType' as smallint) as "Type",
                    cast("Supplement" ->> 'leverage' as int)    as "Leverage",
                    "CreatedOn"
             from trd."_Application"
             where "PartyId" = {partyId}
               and "Type" = {(short)ApplicationTypes.TradeAccount}
               and "Status" = {(short)ApplicationStatusTypes.AwaitingApproval}
             """
        );
        return items;
    }

    public async Task UpdateAccountSearchText(long id)
    {
        var account = await tenantCtx.Accounts
            .Include(x => x.Party)
            .ThenInclude(x => x.Wallets)
            .Include(x => x.Tags)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (account == null) return;

        var user = account.Party;
        var searchTextBuilder = new StringBuilder();
        var roleName = account.Role switch
        {
            (int)AccountRoleTypes.Client => "Client",
            (int)AccountRoleTypes.Agent => "Agent",
            (int)AccountRoleTypes.Sales => "Sales",
            (int)AccountRoleTypes.Rep => "Rep",
            _ => string.Empty
        };

        searchTextBuilder.Append($"{account.Id}")
            .Append($",{account.Uid}")
            .Append($",{account.AccountNumber}")
            .Append($",{account.PartyId}")
            .Append($",{account.Name}")
            .Append($",{roleName}")
            .Append($",{account.Code}")
            .Append($",{account.Group}")
            .Append($",{user.Id}")
            .Append($",{user.Uid}")
            .Append($",{user.FirstName} {user.LastName}")
            .Append($",{user.LastName} {user.FirstName}")
            .Append($",{user.NativeName}")
            .Append($",{user.LastLoginIp}")
            .Append($",{user.RegisteredIp}")
            .Append($",{user.Email}")
            .Append($",{user.PhoneNumber}")
            .Append($",{user.Wallets
                .FirstOrDefault(x => x.CurrencyId == account.CurrencyId && x.FundType == account.FundType)?.Id}");

        foreach (var tag in account.Tags)
        {
            searchTextBuilder.Append($",{tag.Name}");
        }

        account.SearchText = searchTextBuilder.ToString();
        tenantCtx.Accounts.Update(account);
        await tenantCtx.SaveChangesAsync();
    }

    public async Task<bool> SetAsDefaultParentAccountAsync(long id)
    {
        var role = await QueryById(id).Select(x => x.Role).SingleAsync();
        var tagName = role switch
        {
            (int)AccountRoleTypes.Agent => AccountTagTypes.DefaultAgentAccount,
            (int)AccountRoleTypes.Sales => AccountTagTypes.DefaultSalesAccount,
            _ => null
        };
        if (tagName == null) return false;

        var partyId = await GetPartyIdByAccountIdAsync(id);
        if (partyId == 0) return false;

        var accountIds = await tenantCtx.Accounts
            .Where(x => x.PartyId == partyId && x.Role == role)
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var accountId in accountIds)
        {
            await RemoveAccountTagsAsync(accountId, tagName);
        }

        await AddAccountTagsAsync(id, tagName);
        return true;
    }

    private IQueryable<Account> QueryById(long id) => tenantCtx.Accounts.Where(x => x.Id == id);

    /// <summary>
    /// Check if user has an existing USC account by email (excluding inactivated accounts)
    /// One user can only have one USC account per email
    /// </summary>
    public async Task<bool> HasUscAccountByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var existingUscAccountCount = await tenantCtx.Accounts
            .Where(x => x.CurrencyId == (int)CurrencyTypes.USC)
            .Where(x => x.Party.Email.ToLower() == email.ToLower())
            .Where(x => x.Status != (int)AccountStatusTypes.Inactivated) // Exclude inactivated accounts
            .CountAsync();

        return existingUscAccountCount > 0;
    }

    public async Task TryAddAccountingTransitionLogForAccount(string action, int from, int to, long operatorPartyId,
        long accountId)
    {
        // check if from and to in StateTypes:
        if (accountId == 0) return;
        try
        {
            if (!Enum.IsDefined(typeof(StateTypes), from) || !Enum.IsDefined(typeof(StateTypes), to))
                return;

            tenantCtx.AccountLogs.Add(Account.BuildLog(accountId, operatorPartyId, action
                , Enum.GetName((StateTypes)from) ?? from.ToString()
                , Enum.GetName((StateTypes)to) ?? to.ToString()));

            await tenantCtx.SaveChangesAsync();
        }
        catch
        {
            // ignored
        }
    }
}