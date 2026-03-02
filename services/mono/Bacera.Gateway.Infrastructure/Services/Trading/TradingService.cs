using Bacera.Gateway.Agent;
using Bacera.Gateway.Auth;
using Bacera.Gateway.Connection;
using Bacera.Gateway.Context;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Vendor.MetaTrader.Models;
using Bacera.Gateway.ViewModels.Parent;
using Bacera.Gateway.ViewModels.Tenant;
using Bacera.Gateway.Web.BackgroundJobs.Hosting;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using LinqKit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RazorEngineCore;

namespace Bacera.Gateway;

public partial class TradingService(
    ITenantGetter tenancy,
    IMyCache myCache,
    TenantDbContext dbContext,
    AuthDbContext authDbContext,
    ITradingApiService apiService,
    ITenantGetter tenancyResolver,
    CentralDbContext centralDbContext,
    AccountManageService accManageSvc,
    IStorageService storageSvc,
    IServiceProvider serviceProvider,
    MyDbContextPool myDbContextPool,
    TenantDbConnection dbConnection,
    ConfigService configSvc,
    ILogger<TradingService> logger)
{
    private readonly ILogger<TradingService> _logger = logger ?? new NullLogger<TradingService>();
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private readonly long _tenantId = tenancyResolver.GetTenantId();
    private readonly string _nameKey = CacheKeys.GetBlackedUserNameHashKey();
    private readonly string _phoneKey = CacheKeys.GetBlackedUserPhoneHashKey();
    private readonly string _emailKey = CacheKeys.GetBlackedUserEmailHashKey();
    private readonly string _idNumberKey = CacheKeys.GetBlackedUserIdNumberHashKey();
    private readonly string _ipKey = CacheKeys.GetBlackedIpHashKey();


    public async Task<Account> AccountGetAsync(long id) =>
        await dbContext.Accounts
            .Include(x => x.TradeAccount)
            .ThenInclude(x => x!.TradeAccountStatus)
            .Include(x => x.RebateClientRule)
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.Id == id) ?? Account.Empty();

    public async Task<Account> AccountGetByUidAsync(long uid) =>
        await dbContext.Accounts
            .Include(x => x.TradeAccount)
            .ThenInclude(x => x!.TradeAccountStatus)
            .FirstOrDefaultAsync(x => x.Uid == uid) ?? Account.Empty();

    /**
     * Get Account with TradeAccount for a Party
     */
    public async Task<Account.ClientResponseModel> AccountGetForPartyAsync(long uid, long partyId) =>
        await dbContext.Accounts
            .Where(x => x.PartyId.Equals(partyId))
            .Where(x => x.Uid.Equals(uid))
            .ToClientResponseModels()
            .FirstOrDefaultAsync() ?? new Account.ClientResponseModel();

    /**
     * Get Account with TradeAccount for a Party
     */
    public async Task<Account.ClientResponseModel> AccountClientResponseModelGetForPartyAsync(long uid, long partyId) =>
        await dbContext.Accounts
            .Where(x => x.PartyId.Equals(partyId))
            .Where(x => x.Uid.Equals(uid))
            .ToClientResponseModels(partyId)
            .FirstOrDefaultAsync() ?? new Account.ClientResponseModel();

    public async Task<TradeAccount> TradeAccountGetAsync(long accountId) =>
        await dbContext.TradeAccounts
            .Include(x => x.TradeAccountStatus)
            .FirstOrDefaultAsync(x => x.Id.Equals(accountId)) ?? new TradeAccount();

    public async Task<TradeAccount> TradeAccountGetByUidAsync(long accountUid) =>
        await dbContext.TradeAccounts
            .Include(x => x.TradeAccountStatus)
            .FirstOrDefaultAsync(x => x.IdNavigation.Uid.Equals(accountUid)) ?? new TradeAccount();

    public async Task<TradeAccount.ClientResponseModel> TradeAccountGetForPartyAsync(long uid, long partyId) =>
        await dbContext.TradeAccounts
            .Where(x => x.IdNavigation.PartyId.Equals(partyId))
            .Where(x => x.IdNavigation.Uid.Equals(uid))
            .ToClientResponseModels()
            .FirstOrDefaultAsync() ?? new TradeAccount.ClientResponseModel();

    public async Task<Result<List<Account>, Account.Criteria>> AccountQueryAsync(Account.Criteria criteria)
    {
        var items = await dbContext.Accounts
            .Include(x => x.TradeAccount)
            .ThenInclude(x => x!.TradeAccountStatus)
            .PagedFilterBy(criteria).ToListAsync();
        return Result<List<Account>, Account.Criteria>.Of(items, criteria);
    }

    public async Task<Result<List<AccountViewModel>, Account.Criteria>> AccountQueryForTenantAsync(
        Account.Criteria criteria, long? partyId = null, bool hideUserEmail = false)
    {
        // *** When Code is provided, find the account by Code and filter by SalesUid (using ReferPath to include sales itself) *** //
        var originalCodeUid = criteria.CodeUid;
        if (!string.IsNullOrEmpty(criteria.CodeUid))
        {
            var salesAccount = await dbContext.Accounts
                .Where(x => x.Code == criteria.CodeUid)
                .Select(x => new { x.Uid })
                .FirstOrDefaultAsync();

            if (salesAccount != null)
            {
                // Set SalesUid to filter by ReferPath (includes sales account itself and all children)
                criteria.SalesUid = salesAccount.Uid;
                // Clear Code so it doesn't filter by CodeUid anymore
                criteria.CodeUid = null;
            }
            else
            {
                // If account with Code not found, set SalesUid to invalid value to return no results
                //criteria.Code = null;
                criteria.SalesUid = null;
            }
        }

        var items = await dbContext.Accounts
            .PagedFilterBy(criteria)
            .ToTenantViewModel(partyId, hideUserEmail)
            .ToListAsync();

        // await FulfillUserForAccountViewModel(items);
        // await FulfillAccountHasComment(items);
        // await FulfillUserHasComment(items);
        await FulfillAccountWizard(items);
        await FulfillAccountConfigurations(items);

        foreach (var item in items)
        {
            var splitIp = item.User.LastLoginIp.Split('.');
            var count = splitIp.Length;

            item.IsInIpBlackList = (count >= 1 && await myCache.HGetStringAsync(_ipKey, $"{splitIp[0]}") == "1")
                                   || (count >= 2 &&
                                       await myCache.HGetStringAsync(_ipKey, $"{splitIp[0]}.{splitIp[1]}") == "1")
                                   || (count >= 3 && await myCache.HGetStringAsync(_ipKey,
                                       $"{splitIp[0]}.{splitIp[1]}.{splitIp[2]}") == "1")
                                   || await myCache.HGetStringAsync(_ipKey, item.User.LastLoginIp) == "1";
            item.IsInUserBlackList = await myCache.HGetStringAsync(_nameKey, item.User.NativeName) == "1"
                                     || await myCache.HGetStringAsync(_phoneKey, item.User.Phone) == "1"
                                     || await myCache.HGetStringAsync(_emailKey, item.User.Email) == "1"
                                     || await myCache.HGetStringAsync(_idNumberKey, item.User.IdNumber) == "1";
        }

        // *** Rollback above criterial change *** //
        if (originalCodeUid != null && criteria.Code == null)
        {
            criteria.CodeUid = originalCodeUid;
            criteria.SalesUid = null;
        }

        return Result<List<AccountViewModel>, Account.Criteria>.Of(items, criteria);
    }

    public async Task<Result<List<Account.ClientResponseModel>, Account.Criteria>> AccountQueryForClientAsync(
        Account.Criteria criteria, long partyId)
    {
        var results = await dbContext.Accounts
            .PagedFilterBy(criteria)
            .ToClientResponseModels(partyId)
            .ToListAsync();

        await FulfillAccountConfigurations(results);
        return Result<List<Account.ClientResponseModel>, Account.Criteria>.Of(results, criteria);
    }

    public async Task<List<AccountViewModel>> ParentAccountsGetForTenantAsync(long accountId, bool hideEmail = false)
    {
        var account = await dbContext.Accounts.SingleOrDefaultAsync(x => x.Id == accountId);
        if (account == null)
            return new List<AccountViewModel>();
        var uids = account.ReferPathUids;
        var results = await dbContext.Accounts
            .Where(x => uids.Contains(x.Uid))
            .OrderBy(x => x.Level)
            .ToTenantViewModel(1, hideEmail)
            .ToListAsync();

        // await FulfillUserForAccountViewModel(results);
        await FulfillAccountHasComment(results);
        // await FulfillUserHasComment(results);
        await FulfillAccountWizard(results);
        await FulfillAccountConfigurations(results);
        return results;
    }

    public async Task<Result<List<AccountForParentViewModel>, Account.Criteria>> AccountQueryForParentAsync(
        Account.Criteria criteria, long partyId, int parentLevel, bool hideUserEmail)
    {
        var items = await dbContext.Accounts
            .PagedFilterBy(criteria)
            .Include(x => x.Tags)
            .ToParentViewModel(partyId)
            .ToListAsync();

        await FulfillUserOfAccountForParentViewModel(items, hideUserEmail);
        items.ForEach(x => x.ParentLevel = parentLevel);
        var result = Result<List<AccountForParentViewModel>, Account.Criteria>.Of(items, criteria);
        return result;
    }

    public async Task<Result<List<TradeAccount>, TradeAccount.Criteria>> TradeAccountQueryAsync(
        TradeAccount.Criteria criteria)
    {
        var items = await dbContext.TradeAccounts
            .Include(x => x.TradeAccountStatus)
            .PagedFilterBy(criteria).ToListAsync();
        return Result<List<TradeAccount>, TradeAccount.Criteria>.Of(items, criteria);
    }

    public async Task<Result<List<TradeAccount.ClientResponseModel>, TradeAccount.Criteria>>
        TradeAccountForClientQueryAsync(
            TradeAccount.Criteria criteria)
    {
        var items = await dbContext.TradeAccounts
            .PagedFilterBy(criteria)
            .ToClientResponseModels()
            .ToListAsync();
        return Result<List<TradeAccount.ClientResponseModel>, TradeAccount.Criteria>.Of(items, criteria);
    }

    public async Task<Result<List<TradeDemoAccount>, TradeDemoAccount.Criteria>> TradeDemoAccountQueryAsync(
        TradeDemoAccount.Criteria criteria)
    {
        var items = await dbContext.TradeDemoAccounts.PagedFilterBy(criteria).ToListAsync();
        return Result<List<TradeDemoAccount>, TradeDemoAccount.Criteria>
            .Of(items, criteria);
    }

    public async Task<Result<List<TradeDemoAccount.ClientResponseModel>, TradeDemoAccount.Criteria>>
        TradeDemoAccountForClientQueryAsync(
            TradeDemoAccount.Criteria criteria)
    {
        var items = await dbContext.TradeDemoAccounts.PagedFilterBy(criteria).ToListAsync();
        var data = items.Select(TradeDemoAccount.ClientResponseModel.Build).ToList();
        return Result<List<TradeDemoAccount.ClientResponseModel>, TradeDemoAccount.Criteria>.Of(data, criteria);
    }

    public async Task<bool> DeleteDemoAccountAsync(long id)
    {
        var account = await dbContext.TradeDemoAccounts.SingleOrDefaultAsync(x => x.Id == id);
        if (account == null) return false;
        dbContext.TradeDemoAccounts.Remove(account);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<TradeService?> GetServiceByPlatformAsync(PlatformTypes platform)
        => await dbContext.TradeServices
            .OrderBy(x => x.Priority)
            .FirstOrDefaultAsync(x => x.Platform == (short)platform);

    public async Task<TradeService> GetServiceByIdAsync(int id)
        => await dbContext.TradeServices
            .FirstOrDefaultAsync(x => x.Id == id) ?? new TradeService();

    public string? GetDemoTradeAccountDefaultGroup(TradeService service, AccountTypes accountType,
        CurrencyTypes currencyId)
    {
        var options = service.GetOptions<TradeServiceOptions>();
        var groups = options.Groups ?? new List<string>();
        if (!groups.Any())
            return null;

        var defaultGroup = groups.First();
        // Fix double-escaping for Groups list as well
        if (!string.IsNullOrEmpty(defaultGroup))
        {
            defaultGroup = defaultGroup.Replace("\\\\", "\\");
        }

        var defaultGroupKey = $"{(int)accountType}_{(int)currencyId}";
        if (options.DefaultGroup.TryGetValue(defaultGroupKey, out var valueByDefaultGroupKey))
        {
            defaultGroup = valueByDefaultGroupKey;
        }
        else if (options.DefaultGroup.TryGetValue("default", out var defaultValue))
        {
            defaultGroup = defaultValue;
        }

        // Fix double-escaping issue: PostgreSQL stores backslashes as \\ in JSON,
        // but sometimes they get double-escaped. Unescape them here.
        if (!string.IsNullOrEmpty(defaultGroup))
        {
            defaultGroup = defaultGroup.Replace("\\\\", "\\");
        }

        return defaultGroup;
    }

    // public async Task<TradeAccount> CreateTradeAccountForAccountAsync(
    //     long accountId,
    //     int tradeServiceId,
    //     string password,
    //     string passwordInvestor,
    //     string passwordPhone,
    //     int leverage,
    //     string tradeServerGroup,
    //     CurrencyTypes currency,
    //     string comment = ""
    // )
    // {
    //     var account = await dbContext.Accounts
    //         .Include(x => x.TradeAccount)
    //         .Include(x => x.Tags)
    //         .Where(x => x.HasTradeAccount == false)
    //         .Where(x => x.Id == accountId)
    //         .FirstOrDefaultAsync();
    //     if (account is not { TradeAccount: null }) return new TradeAccount();
    //
    //     var parentAccounts = await dbContext.Accounts
    //         .Where(x => x.Id == account.SalesAccountId || x.Id == account.AgentAccountId)
    //         .ToListAsync();
    //
    //     var accountNumber = await GenerateMt4AccountNumber(tradeServiceId, Enum.GetName(currency) ?? "USD");
    //     var user = await authDbContext.Users.SingleAsync(x => x.PartyId == account.PartyId && x.TenantId == _tenantId);
    //
    //     var salesAccount = parentAccounts.FirstOrDefault(x => x.Role == (int)AccountRoleTypes.Sales);
    //     comment = salesAccount?.Code ?? comment;
    //
    //     var agentAccount = parentAccounts.FirstOrDefault(x => x.Role == (int)AccountRoleTypes.Agent);
    //     var agent = agentAccount?.Uid ?? 0;
    //     var request = new CreateAccountRequest
    //     {
    //         Login = accountNumber,
    //         Email = user.Email ?? string.Empty,
    //         Country = user.CountryCode.ToLower(),
    //         Leverage = leverage,
    //         Password = password,
    //         PasswordInvestor = passwordInvestor,
    //         Name = user.GuessUserName(),
    //         Phone = user.PhoneNumber ?? string.Empty,
    //         PasswordPhone = passwordPhone,
    //         Status = "RE",
    //         Group = tradeServerGroup,
    //         Address = user.Address,
    //         Id = user.IdNumber,
    //         Comment = $"{comment} {account.Group}",
    //         Agent = agent,
    //         Zipcode = account.Group,
    //     };
    //     var result = await apiService.CreateAccountAsync(tradeServiceId, request);
    //
    //     if (result.AccountNumber == 0)
    //     {
    //         _logger.LogError("Failed to create account: [{ServiceId}], {@Request} {@Response}", tradeServiceId, request,
    //             request);
    //         return new TradeAccount();
    //     }
    //
    //     var tradeAccount = TradeAccount.Build(account.Id, currency);
    //     tradeAccount.ServiceId = tradeServiceId;
    //     tradeAccount.AccountNumber = result.AccountNumber;
    //     tradeAccount.TradeAccountStatus = new TradeAccountStatus
    //     {
    //         Balance = 0,
    //         Leverage = leverage,
    //         Group = tradeServerGroup,
    //         ReadOnlyCode = passwordInvestor,
    //         CreatedOn = DateTime.UtcNow,
    //         UpdatedOn = DateTime.UtcNow,
    //         Currency = Enum.GetName(typeof(CurrencyTypes), currency) ?? "",
    //     };
    //     account.HasTradeAccount = true;
    //     account.TradeAccount = tradeAccount;
    //     account.AccountNumber = result.AccountNumber;
    //     account.ServiceId = result.ServiceId;
    //     try
    //     {
    //         await accManageSvc.AddAccountTagsAsync(account.Id, AccountTagTypes.DailyConfirmEmail, AccountTagTypes.NewRebateTest);
    //     }
    //     catch (Exception e)
    //     {
    //         BcrLog.Slack($"CreateTradeAccountForAccountAsync_AddAccountTag_Error: {e.Message}");
    //     }
    //
    //     dbContext.Accounts.Update(account);
    //     await dbContext.SaveChangesAsync();
    //     var tenantTradeAccount =
    //         await centralDbContext.CentralAccounts.FirstOrDefaultAsync(x =>
    //             x.AccountId == account.Id && tenancy.GetTenantId() == x.TenantId);
    //     if (tenantTradeAccount == null)
    //     {
    //         await centralDbContext.CentralAccounts.AddAsync(CentralAccount.Build(tenancy.GetTenantId(),
    //             account.TradeAccount.ServiceId, account.Id, tradeAccount.AccountNumber));
    //     }
    //     else
    //     {
    //         tenantTradeAccount.AccountNumber = tradeAccount.AccountNumber;
    //         tenantTradeAccount.ServiceId = tradeServiceId;
    //         centralDbContext.CentralAccounts.Update(tenantTradeAccount);
    //     }
    //
    //     await centralDbContext.SaveChangesAsync();
    //     await dbContext.SaveChangesAsync();
    //     return account.TradeAccount;
    // }

    public async Task<TradeDemoAccount> TradeDemoAccountCreateAsync(long partyId, int serviceId, AccountTypes type,
        string name, string email,
        string password, double initialBalance, CurrencyTypes currencyId, int leverage, string group)
    {
        // Use Redis atomic counter to generate unique account numbers
        // This eliminates race condition and iteration - much simpler!
        var accountNumber = await GetNextDemoAccountNumberAsync(serviceId, Enum.GetName(currencyId) ?? "USD");
        
        if (accountNumber == 0)
        {
            _logger.LogError("Failed to generate demo account number for service {ServiceId}", serviceId);
            return new TradeDemoAccount();
        }
        
        TradeAccount? result = null;
        const int maxRetries = 3; // Retry for MT5 errors only
        
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation("Attempting to create demo account {AccountNumber} for service {ServiceId} (attempt {Attempt})", 
                    accountNumber, serviceId, attempt + 1);
                
                result = await apiService.CreateAccountAsync(serviceId, name, password, leverage, group, accountNumber);
                
                if (result.AccountNumber > 0)
                {
                    _logger.LogInformation("Successfully created demo account {AccountNumber}", result.AccountNumber);
                    break; // Success!
                }
            }
            catch (Exception ex)
            {
                // Check if this is an "Account already exists" error (should be extremely rare now)
                if (ex.Message.Contains("3004") || ex.Message.Contains("Account already exists"))
                {
                    _logger.LogWarning("Demo account {AccountNumber} already exists (rare case). Getting new number and retrying...", 
                        accountNumber);
                    
                    // Get a fresh account number and retry
                    accountNumber = await GetNextDemoAccountNumberAsync(serviceId, Enum.GetName(currencyId) ?? "USD");
                    
                    if (accountNumber == 0)
                    {
                        throw new Exception("Failed to generate new demo account number");
                    }
                    
                    if (attempt < maxRetries - 1)
                    {
                        continue;
                    }
                }
                
                // For other errors or last attempt, rethrow
                throw;
            }
        }
        
        if (result == null || result.AccountNumber == 0)
        {
            _logger.LogError("Failed to create demo account for service {ServiceId}", serviceId);
            return new TradeDemoAccount();
        }

        // *** Adjust Precision for USC *** //
        initialBalance = initialBalance.ToScaledFromCents();

        var demoAccount = TradeDemoAccount.Build(partyId, serviceId, result.AccountNumber, email: email);
        demoAccount.Leverage = leverage;
        demoAccount.Balance = initialBalance;
        demoAccount.Type = (short)type;
        demoAccount.CurrencyId = (short)currencyId;
        await dbContext.TradeDemoAccounts.AddAsync(demoAccount);
        await dbContext.SaveChangesAsync();

        await apiService.ChangeBalance(serviceId, result.AccountNumber, (decimal)initialBalance,
            "Initial balance for demo account");

        return demoAccount;
    }

    public async Task<Tuple<bool, string>> TradeAccountChangeBalanceAndUpdateStatus(long accountId, decimal amount,
        string comment = "")
    {
        var account = await TradeAccountGetAsync(accountId);
        if (account.IsEmpty()) return Tuple.Create(false, string.Empty);
        
        var result = await apiService.ChangeBalance(account.ServiceId, account.AccountNumber, amount, comment);
        if (false == result.Item1) return result;

        try
        {
            var response = await apiService.GetAccountBalanceAndLeverage(account.ServiceId, account.AccountNumber);
            if (!response.Item1) return result;

            if (account.TradeAccountStatus == null) return result;

            account.TradeAccountStatus.Leverage = response.Item2;
            account.TradeAccountStatus.Balance = response.Item3;
            dbContext.TradeAccounts.Update(account);
            await dbContext.SaveChangesAsync();
        }
        catch
        {
            // ignored
        }

        return result;
        
    }

    // public async Task<Tuple<bool, string>> TradeAccountChangeBalance(long accountId, decimal amount,
    //     string comment = "")
    // {
    //     var account = await TradeAccountGetAsync(accountId);
    //     if (account.IsEmpty()) return Tuple.Create(false, string.Empty);
    //     return await apiService.ChangeBalance(account.ServiceId, account.AccountNumber, amount, comment);
    // }

    public async Task<Tuple<bool, double>> TradeAccountCheckBalance(long accountId)
    {
        var account = await TradeAccountGetAsync(accountId);
        if (account.IsEmpty())
            return Tuple.Create(false, 0d);

        if (account.TradeAccountStatus == null)
            return Tuple.Create(false, 0d);

        var response = await apiService.GetAccountBalanceAndLeverage(account.ServiceId, account.AccountNumber);
        if (!response.Item1) return Tuple.Create(false, 0d);

        if (response.Item2 == account.TradeAccountStatus.Leverage
            && Math.Abs(response.Item3 - account.TradeAccountStatus.Balance) < 0.01)
            return Tuple.Create(true, response.Item3);

        account.TradeAccountStatus.Leverage = response.Item2;
        account.TradeAccountStatus.Balance = response.Item3;
        dbContext.TradeAccounts.Update(account);
        await dbContext.SaveChangesAsync();

        return Tuple.Create(true, response.Item3);
    }

    public async Task<Tuple<bool, string>> TradeAccountChangeCreditAsync(long accountId, decimal amount,
        string comment = "")
    {
        var account = await TradeAccountGetAsync(accountId);
        if (account.IsEmpty()) return Tuple.Create(false, string.Empty);
        return await apiService.ChangeCredit(account.ServiceId, account.AccountNumber, amount, comment);
    }

    public async Task<string> GetMetaTradeGroupAndSymbolInfo(int serviceId, string group, string symbol,
        int transId)
    {
        return await apiService.GetGroupAndSymbols(serviceId, group, symbol, transId);
    }

    /// <summary>
    /// Get Account Balance and Leverage; ! balance is in decimal 
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    public async Task<Tuple<bool, int, double>> GetTradeAccountBalanceAndLeverageFromServer(long accountId)
    {
        var account = await TradeAccountGetAsync(accountId);
        if (account.IsEmpty()) return Tuple.Create(false, 0, 0d);

        try
        {
            var response = await apiService.GetAccountBalanceAndLeverage(account.ServiceId, account.AccountNumber);
            return response;
        }
        catch
        {
            return Tuple.Create(false, 0, 0d);
        }
    }

    public async Task<bool> TradeAccountChangePassword(long accountId, string password, long operatorPartyId = 1)
    {
        var account = await dbContext.Accounts.SingleOrDefaultAsync(x => x.Id == accountId);
        if (account == null || account.Status != (short)AccountStatusTypes.Activate)
        {
            _logger.LogInformation("TradeAccountChangePassword: Account not found {AccountId}", accountId);
            return false;
        }

        var result = await apiService.ChangePasswordAsync(account.ServiceId, account.AccountNumber, password);
        if (!result)
        {
            _logger.LogInformation(
                "TradeAccountChangePassword_Change_password_service_failed {AccountNumber} - {Password}",
                account.AccountNumber, password);
            return false;
        }

        account.AccountLogs.Add(Account.BuildLog(operatorPartyId, "ChangePassword", "******", "******"));
        account.UpdatedOn = DateTime.UtcNow;
        dbContext.Accounts.Update(account);
        await dbContext.SaveChangesWithAuditAsync(operatorPartyId);
        return result;
    }

    public async Task<bool> TradeAccountChangeLeverage(long accountId, int leverage, long operatorPartyId = 1)
    {
        var account = await dbContext.Accounts
            .Include(x => x.TradeAccountStatus)
            .SingleOrDefaultAsync(x => x.Id == accountId);
        if (account == null) return false;
        if (account.TradeAccountStatus == null) return true;

        var result = await apiService.ChangeLeverageAsync(account.ServiceId, account.AccountNumber, leverage);
        if (!result) return false;

        var leverageResult = leverage;
        try
        {
            var (success, leverageResApi, balanceResult) =
                await apiService.GetAccountBalanceAndLeverage(account.ServiceId, account.AccountNumber);
            account.TradeAccountStatus.Balance = balanceResult;
        }
        catch (Exception e)
        {
            // BcrLog.Slack($"TradeAccountChangeLeverage_GetAccountBalanceAndLeverage_Error: {e.Message}");
            logger.LogInformation(e, "TradeAccountChangeLeverage_GetAccountBalanceAndLeverage_Error");
            leverageResult = leverage;
        }


        account.AccountLogs.Add(Account.BuildLog(operatorPartyId, "ChangeLeverage",
            account.TradeAccountStatus.Leverage.ToString(),
            leverageResult.ToString()));

        account.TradeAccountStatus.Leverage = leverageResult;
        account.UpdatedOn = DateTime.UtcNow;
        dbContext.Accounts.Update(account);
        await dbContext.SaveChangesWithAuditAsync(operatorPartyId);
        return true;
    }

    public async Task<int> TradeDemoAccountCountAsync(long partyId, int serviceId) =>
        await dbContext.TradeDemoAccounts
            .Where(x => x.PartyId.Equals(partyId) && x.ServiceId.Equals(serviceId))
            .Where(x => x.ExpireOn > DateTime.UtcNow)
            .CountAsync();

    public async Task<long> TradeAccountLookupByUidAsync(long uid)
    {
        return await dbContext.Accounts
            .Where(x => x.Uid == uid)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<Account> AccountLookupForParentAsync(long parentAccountUid, long uid)
        => await dbContext.Accounts
               .Where(x => x.Uid == uid)
               .Where(x => x.ReferPath.Contains(parentAccountUid.ToString()))
               .FirstOrDefaultAsync()
           ?? Account.Empty();

    public async Task<bool> IsAgentUidExists(long agentAccountUid, long partyId)
        => await dbContext.Accounts
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Uid == agentAccountUid)
            .Where(x => x.Role == (short)AccountRoleTypes.Agent)
            //.Where(x => x.Status == (short)AccountStatusTypes.Activate)
            .AnyAsync();

    public async Task<bool> IsSalesUidExists(long salesAccountUid, long partyId)
        => await dbContext.Accounts
            .Where(x => x.PartyId == partyId)
            .Where(x => x.Uid == salesAccountUid)
            .Where(x => x.Role == (short)AccountRoleTypes.Sales)
            //.Where(x => x.Status == (short)AccountStatusTypes.Activate)
            .AnyAsync();

    public async Task<int> GetAccountCountAsync()
        => await dbContext.Accounts.CountAsync();

    public async Task<bool> AccountExistsForPartyAsync(long accountId, long partyId) =>
        await dbContext.Accounts.Where(x => x.Id == accountId)
            .Where(x => x.PartyId == partyId)
            .AnyAsync();

    /// <summary>
    /// Get account id by uid and party id
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="partyId"></param>
    /// <returns>bool isExists, long AccountId</returns>
    public async Task<Tuple<bool, long>> AccountUidExistsForPartyAsync(long uid, long partyId)
    {
        var result = await dbContext.Accounts.Where(x => x.Uid.Equals(uid))
            .Where(x => x.PartyId == partyId)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();
        return new Tuple<bool, long>(result > 0, result);
    }

    private async Task FulfillUserOfAccountForParentViewModel(List<AccountForParentViewModel> accounts,
        bool hideUserEmail = true)
    {
        await Task.Delay(0);
        // var partyIds = accounts
        //     .Select(x => x.PartyId)
        //     .Where(x => x > 0)
        //     .Distinct()
        //     .ToList();
        //
        // var users = await authDbContext.Users
        //     .Where(x => partyIds.Contains(x.PartyId) && x.TenantId == _tenantId)
        //     .ToParentViewModel()
        //     .ToListAsync();

        foreach (var item in accounts)
        {
            // var user = users.FirstOrDefault(x => x.PartyId == item.PartyId);
            // if (user == null) continue;
            // item.User = user;
            item.User.EmailRaw = hideUserEmail ? HideEmail(item.User.EmailRaw) : item.User.EmailRaw;
        }
    }

    private static string HideEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return string.Empty;

        var atIndex = email.IndexOf('@');
        if (atIndex == -1)
            return email; // No '@' found, return the original email

        var firstCharacter = email[0];
        var lastCharacterBeforeAt = email[atIndex - 1];

        return $"{firstCharacter}****************{lastCharacterBeforeAt}{email[atIndex..]}";
    }

    private async Task FulfillAccountHasComment(List<AccountViewModel> accounts)
    {
        var accountIds = accounts
            .SelectMany(x => new List<long> { x.Id, x.AgentAccount.Id })
            .Where(x => x > 0)
            .Distinct()
            .ToList();

        var comments = await dbContext.Comments
            .Where(x => accountIds.Contains(x.RowId))
            .Where(x => x.Type == (int)CommentTypes.Account)
            .Select(x => new
            {
                Id = x.RowId
            })
            .ToListAsync();

        foreach (var item in accounts)
        {
            item.HasComment = comments.Any(x => x.Id == item.Id);
            item.AgentAccount.HasComment = comments.Any(x => x.Id == item.AgentAccount.Id);
        }
    }

    // private async Task FulfillUserHasComment(List<AccountViewModel> accounts)
    // {
    //     var rowIds = accounts
    //         .SelectMany(x => new List<long> { x.User.PartyId, x.AgentAccount.User.PartyId })
    //         .Where(x => x > 0)
    //         .Distinct()
    //         .ToList();
    //
    //     var hasCommentsDictionary = await dbContext.Comments
    //         .Where(x => rowIds.Contains(x.RowId) && x.Type == (int)CommentTypes.User)
    //         .GroupBy(x => x.RowId)
    //         .ToDictionaryAsync(g => g.Key, g => g.Any());
    //
    //     foreach (var item in accounts)
    //     {
    //         item.User.HasComment = hasCommentsDictionary.Any(x => x.Key == item.User.PartyId && x.Value);
    //         item.AgentAccount.User.HasComment = hasCommentsDictionary.Any(x => x.Key == item.AgentAccount.User.PartyId && x.Value);
    //     }
    // }

    private async Task FulfillAccountWizard(List<AccountViewModel> accounts)
    {
        var aids = accounts
            .Select(x => x.Id)
            .Where(x => x > 0)
            .Distinct()
            .ToList();

        var supplements = await dbContext.Supplements
            .Where(x => x.Type == (int)SupplementTypes.AccountWizard)
            .Where(x => aids.Contains(x.RowId))
            .ToListAsync();

        foreach (var item in accounts)
        {
            var supplement = supplements.FirstOrDefault(x => x.RowId == item.Id);
            if (supplement == null) continue;
            item.Wizard = Supplement.AccountWizard.FromJson(supplement.Data);
        }
    }

    private async Task FulfillAccountConfigurations<T>(List<T> accounts)
        where T : class, ICanFulfillConfigurations
    {
        var ids = accounts
            .Select(x => x.Id)
            .Where(x => x > 0)
            .Distinct()
            .ToList();

        var configurations = await dbContext.Configurations
            .Where(x => x.Category == nameof(Account))
            .Where(x => ids.Contains(x.RowId))
            .ToListAsync();

        foreach (var account in accounts)
        {
            account.Configurations = configurations.Where(x => x.RowId == account.Id).ToList();
        }
    }

    /// <summary>
    /// Get next demo account number using Redis atomic counter
    /// This method ALWAYS syncs with DB MAX value and uses Redis INCR for atomic increment
    /// Handles Redis flush by always checking DB to pick the bigger value
    /// Eliminates race condition and iteration - much faster!
    /// </summary>
    private async Task<long> GetNextDemoAccountNumberAsync(int serviceId, string currencyCode)
    {
        var prefix = await apiService.GetAccountPrefix(serviceId, currencyCode);
        prefix = prefix > 0 ? prefix : 53000000;
        
        // Redis key to store the current max account number for this service
        var redisKey = $"demo_account_max:service_{serviceId}";
        
        try
        {
            // ALWAYS query database MAX to handle Redis flush scenarios
            var maxFromDb = await dbContext.TradeDemoAccounts
                .Where(x => x.ServiceId == serviceId)
                .MaxAsync(x => (long?)x.AccountNumber) ?? 0;
            
            _logger.LogDebug("MAX from database for service {ServiceId}: {MaxFromDb}", serviceId, maxFromDb);
            
            // Get current value from Redis
            var redisValue = await myCache.GetStringAsync(redisKey);
            long maxFromRedis = 0;
            
            if (!string.IsNullOrEmpty(redisValue) && long.TryParse(redisValue, out maxFromRedis))
            {
                _logger.LogDebug("Current Redis value for service {ServiceId}: {MaxFromRedis}", serviceId, maxFromRedis);
            }
            else
            {
                _logger.LogInformation("Redis counter not found for service {ServiceId}, initializing...", serviceId);
            }
            
            // Choose the max value:
            // - If accounts exist in DB or Redis: use their max (continue sequence)
            // - If no accounts exist: start from prefix - 1 (so INCR gives exactly prefix)
            long maxAccountNumber;
            if (maxFromDb > 0 || maxFromRedis > 0)
            {
                maxAccountNumber = Math.Max(maxFromDb, maxFromRedis);
            }
            else
            {
                maxAccountNumber = prefix - 1;
            }
            
            _logger.LogDebug("Using max value {MaxValue} for service {ServiceId} (DB: {DbMax}, Redis: {RedisMax}, Prefix: {Prefix})",
                maxAccountNumber, serviceId, maxFromDb, maxFromRedis, prefix);
            
            // Update Redis with the current max (in case DB has higher value after Redis flush)
            await myCache.SetStringAsync(redisKey, maxAccountNumber.ToString(), TimeSpan.FromDays(365));
            
            // Atomically increment the counter in Redis using INCR command
            // This is the key part - INCR is atomic and thread-safe across all instances!
            var db = myCache.GetDatabase();
            var nextNumber = await db.StringIncrementAsync(redisKey);
            
            _logger.LogInformation("Generated demo account number {AccountNumber} for service {ServiceId}", 
                nextNumber, serviceId);
            
            // Set expiry again to keep it fresh
            await db.KeyExpireAsync(redisKey, TimeSpan.FromDays(365));
            
            return nextNumber;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating demo account number for service {ServiceId}. Falling back to timestamp-based number.", serviceId);
            
            // Fallback: if Redis fails, use prefix + timestamp for uniqueness
            // This ensures we still can create accounts even if Redis is down
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 1000000;
            return prefix + timestamp;
        }
    }

    private async Task<long> GenerateMt4AccountNumber(int serviceId, string currencyCode)
    {
        // if (!await apiService.IsAccountNumberExists(serviceId, defaultUid)
        //     && !await dbContext.Accounts.AnyAsync(x => x.AccountNumber == defaultUid))
        // {
        //     return defaultUid;
        // }

        var prefix = await apiService.GetAccountPrefix(serviceId, currencyCode);
        prefix = prefix > 0 ? prefix : 55000000;
        while (true)
        {
            // how many ending 0s in prefix
            var zeros = prefix.ToString().Length - prefix.ToString().TrimEnd('0').Length;
            // zero number of 9
            var mmax = (int)Math.Pow(10, zeros) - 1;
            var number = (long)new Random().Next(0, mmax);
            number += prefix;
            var exists = await apiService.IsAccountNumberExists(serviceId, number) ||
                         await AccountNumberAndUidValidator(number);
            if (!exists)
            {
                return number;
            }
        }
    }
}