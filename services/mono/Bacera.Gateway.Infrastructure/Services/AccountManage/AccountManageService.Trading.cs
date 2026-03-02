using Bacera.Gateway.Auth;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Vendor.MetaTrader;
using Bacera.Gateway.Vendor.MetaTrader.Models;
using Bacera.Gateway.ViewModels.Parent;
using Bacera.Gateway.Web;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway.Services.AccountManage;

public partial class AccountManageService
{
    public async Task<(bool, string)> CheckAccountNumberAsync(long accountNumber)
    {
        var inCrm = await centralCtx.CentralAccounts.AnyAsync(x =>
            x.AccountNumber == accountNumber || x.Uid == accountNumber);

        if (inCrm) return (false, "Account Number already in CRM");

        // ** TODO **: comment temporarily, need to check if the account number already exists in MT5/MT4
        //foreach (var serviceId in pool.GetServiceNameDict().Select(x => x.Key))
        //{
        //    var platform = pool.GetPlatformByServiceId(serviceId);
        //    if (platform == PlatformTypes.MetaTrader5)
        //    {
        //        await using var ctx = pool.CreateCentralMT5DbContextAsync(serviceId);
        //        var inMt5 = await ctx.Mt5Users.AnyAsync(x => x.Login == (ulong)accountNumber);
        //        if (inMt5) return (false, "Account Number not in CRM but already in MT5");
        //    }
        //    else if (platform == PlatformTypes.MetaTrader4)
        //    {
        //        await using var ctx = pool.CreateCentralMT4DbContextAsync(serviceId);
        //        var inMt4 = await ctx.Mt4Users.AnyAsync(x => x.Login == (int)accountNumber);
        //        if (inMt4) return (false, "Account Number not in CRM but already in MT4");
        //    }
        //}

        return (true, string.Empty);
    }

    public async Task<(bool, string)> ChangeAccountNumberAsync(long accountId, long accountNumber)
    {
        var (valid, vlMsg) = await CheckAccountNumberAsync(accountNumber);
        if (!valid) return (false, vlMsg);
        await tenantDbCon.ExecuteAsync($"""
                                        update trd."_Account" set "AccountNumber" = {accountNumber} where "Id" = {accountId};
                                        update trd."_TradeAccount" set "AccountNumber" = {accountNumber} where "Id" = {accountId};
                                        update trd."_TradeRebate" set "AccountNumber" = {accountNumber} where "AccountId" = {accountId};
                                        update trd."_AccountReport" set "AccountNumber" = {accountNumber} where "AccountId" = {accountId};
                                        """);

        await centralDbCon.ExecuteAsync($"""
                                         update trd."_CentralAccount" set "AccountNumber" = {accountNumber} where "AccountId" = {accountId};
                                         """);
        return (true, string.Empty);
    }

    // public async Task<(bool, string)> CreateAccountAsync()
    public async Task<string?> GetDemoTradeAccountDefaultGroupAsync(long serviceId, AccountTypes accountType,
        CurrencyTypes currencyId)
    {
        var optionString = await tenantCtx.TradeServices
            .Where(x => x.Id == serviceId)
            .Select(x => x.Configuration)
            .FirstOrDefaultAsync();
        if (optionString == null) return null;

        if (!TradeServiceOptions.TryParse(optionString, out var options))
            return null;

        var groups = options.Groups ?? [];
        if (groups.Count == 0) return null;

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

    public async Task<long> CreateTradeDemoAccountAsync(long partyId, int serviceId, AccountTypes type,
        string name, string email, string password, double initialBalance, CurrencyTypes currencyId, int leverage,
        string group)
    {
        TradeAccount? result;

        try
        {
            result = await apiSvc.CreateAccountAsync(serviceId, name, password, leverage, group, null);
            await apiSvc.ChangeBalance(serviceId, result.AccountNumber, (decimal)initialBalance,
                "Initial balance for demo account");
        }
        catch (Exception e)
        {
            BcrLog.Slack($"CreateTradeDemoAccountAsync_{partyId}_{serviceId}, msg: {e.Message}");
            return -1;
        }

        var demoAccount = TradeDemoAccount.Build(partyId, serviceId, result.AccountNumber, email: email);
        demoAccount.Leverage = leverage;
        demoAccount.Balance = initialBalance;
        demoAccount.Type = (short)type;
        demoAccount.CurrencyId = (short)currencyId;
        tenantCtx.TradeDemoAccounts.Add(demoAccount);
        await tenantCtx.SaveChangesAsync();
        return result.AccountNumber;
    }


    public async Task<(bool, string)> CreateClientAsync(long partyId, long parentAccountId, CurrencyTypes currencyId,
        FundTypes fundType,
        AccountTypes? accountType = AccountTypes.Standard,
        SiteTypes? siteId = null, bool hasTradeAccount = false, string? referCode = "",
        long operatorPartyId = 1, long tenantId = 0, int? tradeServiceId = null, long? accountNumber = null)
    {
        var user = await userSvc.GetPartyAsync(partyId);

        var parentAccount = await tenantCtx.Accounts.Where(x => x.Id == parentAccountId)
            .Select(x => new
                { x.ReferPath, x.Role, x.Level, x.Group, x.SiteId, x.SalesAccountId, x.AgentAccountId, x.Id })
            .SingleOrDefaultAsync();
        if (parentAccount == null) return (false, ResultMessage.Account.ParentAccountNotFound);

        ReferralCode? referralCode;
        if (!string.IsNullOrEmpty(referCode))
        {
            referralCode = await tenantCtx.ReferralCodes.SingleOrDefaultAsync(x => x.Code == referCode);
            if (referralCode == null) return (false, ResultMessage.Referral.ReferralCodeNotExist);
        }
        else
        {
            referralCode =
                await tenantCtx.ReferralCodes.FirstOrDefaultAsync(x =>
                    x.AccountId == parentAccountId && x.IsDefault == 1);
        }

        long? accountUid = null;
        if (accountNumber is > 0)
        {
            var (valid, _) = await CheckAccountNumberAsync((long)accountNumber);
            if (valid)
            {
                accountUid = accountNumber;
            }
        }

        accountUid ??= tradeServiceId == null
            ? await Utils.GenerateUniqueIdAsync(AccountNumberAndUidValidator)
            : await GenerateMt4AccountNumber((int)tradeServiceId, Enum.GetName(currencyId) ?? "USD");


        var account = new Account
        {
            Role = (short)AccountRoleTypes.Client,
            PartyId = partyId,
            Uid = accountUid.Value,
            ReferPath = $"{parentAccount.ReferPath}.{accountUid}",
            Level = parentAccount.Level + 1,
            Type = (short)accountType!,
            Group = parentAccount.Group,
            Permission = parentAccount.Group == "TMTM" ? "01111" : "11111",
            Code = string.Empty,
            SalesAccountId = parentAccount.Role == (short)AccountRoleTypes.Sales
                ? parentAccount.Id
                : parentAccount.SalesAccountId,
            AgentAccountId = parentAccount.Role == (short)AccountRoleTypes.Agent ? parentAccount.Id : null,
            Name = user.GuessNativeName(),
            CurrencyId = (short)currencyId,
            FundType = (short)fundType,
            HasTradeAccount = hasTradeAccount,
            HasLevelRule = (int)HasLevelRuleTypes.HasNoLevelRule,
            SiteId = siteId == null ? parentAccount.SiteId : (int)siteId,
            SearchText = string.Empty,
            ReferCode = referralCode?.Code,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            AccountNumber = accountUid.Value,
        };

        tenantCtx.Accounts.Add(account);
        await tenantCtx.SaveChangesAsync();
        centralCtx.CentralAccounts.Add(account.ToCentralAccount(tenantId));
        await centralCtx.SaveChangesAsync();

        // assign rebate client rule

        var distributionType = RebateDistributionTypes.Direct;
        if (referralCode != null)
        {
            var refHistory = Referral.Build(referralCode.Id, referralCode.PartyId, account.PartyId, referralCode.Code,
                nameof(Account), account.Id);
            tenantCtx.Referrals.Add(refHistory);
            await tenantCtx.SaveChangesAsync();

            var spec = referralCode.TryGetClientCreateSpec();
            if (siteId == null && spec?.SiteId != null)
            {
                account.SiteId = (int)spec.SiteId;
            }

            var schema = spec?.AllowAccountTypes;
            if (schema != null)
            {
                distributionType = RebateDistributionTypes.Allocation;
            }

            if (spec?.DistributionType == RebateDistributionTypes.LevelPercentage)
            {
                distributionType = RebateDistributionTypes.LevelPercentage;
            }
        }

        var clientRule = new RebateClientRule
        {
            Schema = "{}",
            ClientAccountId = account.Id,
            DistributionType = (short)distributionType,
        };

        tenantCtx.Accounts.Update(account);
        tenantCtx.RebateClientRules.Add(clientRule);
        await tenantCtx.SaveChangesAsync();

        // await CreateClientDefaultReferralCodeForClient(account.Id, operatorPartyId, tenantId);
        return (true, account.Id.ToString());
    }

    public async Task<bool> SetClientDirectSchemaId(long accountId, long directSchemaId)
    {
        var clientRule = await tenantCtx.RebateClientRules
            .Where(x => x.ClientAccountId == accountId)
            .SingleOrDefaultAsync();

        if (clientRule == null) return false;

        clientRule.RebateDirectSchemaId = directSchemaId;
        tenantCtx.RebateClientRules.Update(clientRule);
        await tenantCtx.SaveChangesAsync();
        await SetPipAndCommissionTagAsync(accountId);
        return true;
    }

    public async Task CreateOrUpdateKycFormAsync(long accountId)
    {
        var account = await tenantCtx.Accounts
            .Where(x => x.Id == accountId)
            .Select(x => new { x.PartyId, x.AccountNumber })
            .FirstOrDefaultAsync();
        if (account == null) return;

        var dict = await tenantCtx.VerificationItems
            .Where(x => x.Verification.PartyId == account.PartyId &&
                        x.Verification.Type == (int)VerificationTypes.Verification)
            .Select(x => new { x.Category, x.Content })
            .ToDictionaryAsync(x => x.Category, x => x.Content);

        var verification = VerificationDTOV2.FromDictionary(dict);

        var user = await userSvc.GetPartyAsync(account.PartyId);

        var kycForm = new KycFormViewModel
        {
            AccountNumber = account.AccountNumber.ToString(),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Citizen = user.Citizen,
            Email = user.EmailRaw,
            PriorName = verification.Info?.PriorName,
            Birthday = user.Birthday,
            Address = user.Address,
            Industry = verification.Financial?.Industry ?? "",
            Occupation = verification.Financial?.Position ?? "",
            AnnualIncome = verification.Financial?.Income ?? "",
            NetWorth = verification.Financial?.Investment ?? "",
            SourceOfFunds = verification.Financial?.Fund.FirstOrDefault() ?? "",
            Bg1 = verification.Financial?.Bg1 ?? "",
            Bg2 = verification.Financial?.Bg2 ?? "",
            Exp1 = verification.Financial?.Exp1 ?? "",
            Exp2 = verification.Financial?.Exp2 ?? "",
            Exp3 = verification.Financial?.Exp3 ?? "",
            Exp4 = verification.Financial?.Exp4 ?? "",
            Exp5 = verification.Financial?.Exp5 ?? "",
            Exp1Employer = verification.Financial?.Exp1Employer ?? "",
            Exp1Position = verification.Financial?.Exp1Position ?? "",
            Exp1Remuneration = verification.Financial?.Exp1Remuneration ?? "",
            Exp2More = verification.Financial?.Exp2More ?? "",
            Exp3More = verification.Financial?.Exp3More ?? "",
            Exp4More = verification.Financial?.Exp4More ?? "",
            Exp5More = verification.Financial?.Exp5More ?? "",
            IdType = verification.Info?.IdType.ToString() ?? "",
            IdNumber = verification.Info?.IdNumber ?? "",
            IdIssueOn = verification.Info?.IdIssuedOn,
            IdExpireOn = verification.Info?.IdExpireOn,
            StaffPartyId = 1,
        };

        var item = await tenantCtx.Verifications
            .Where(x => x.PartyId == account.PartyId && x.Type == (int)VerificationTypes.KycForm)
            .Include(x => x.VerificationItems)
            .FirstOrDefaultAsync();
        var vItem = new VerificationItem
        {
            Content = Utils.JsonSerializeObject(kycForm),
            Category = VerificationCategoryTypes.KycForm,
        };
        if (item == null)
        {
            var form = new Verification
            {
                PartyId = account.PartyId,
                Type = (short)VerificationTypes.KycForm,
                Status = (int)VerificationStatusTypes.AwaitingReview,
                Note = string.Empty,
                VerificationItems = new List<VerificationItem> { vItem },
            };
            tenantCtx.Verifications.Add(form);
        }
        else
        {
            item.VerificationItems.Add(vItem);
            item.Status = (int)VerificationStatusTypes.AwaitingReview;
            item.UpdatedOn = DateTime.UtcNow;
            tenantCtx.Verifications.Update(item);
        }

        await tenantCtx.SaveChangesAsync();
    }

    public async Task<TradeAccount.CreatedTradeAccountInfo> CreateTradeAccountAsync(
        long accountId,
        int tradeServiceId,
        int leverage,
        string tradeServerGroup,
        CurrencyTypes currency,
        string comment = ""
    )
    {
        var password = Utils.GenerateSimplePassword();
        var passwordInvestor = Utils.GenerateSimplePassword();
        var passwordPhone = new Random().Next(1099, 9800).ToString();
        
        var account = await tenantCtx.Accounts
            .Include(x => x.TradeAccount)
            // .Include(x => x.AccountTags)
            .Where(x => x.HasTradeAccount == false)
            .Where(x => x.Id == accountId)
            .FirstOrDefaultAsync();
        if (account is not { TradeAccount: null }) return new TradeAccount.CreatedTradeAccountInfo();

        var parentAccounts = await tenantCtx.Accounts
            .Where(x => x.Id == account.SalesAccountId || x.Id == account.AgentAccountId)
            .ToListAsync();

        var accountNumber = account.AccountNumber;
        var user = await tenantCtx.Parties
            .Where(x => x.Id == account.PartyId)
            .ToTenantDetailModel()
            .SingleAsync();

        var salesAccount = parentAccounts.FirstOrDefault(x => x.Role == (int)AccountRoleTypes.Sales);
        comment = salesAccount?.Code ?? comment;

        var agentAccount = parentAccounts.FirstOrDefault(x => x.Role == (int)AccountRoleTypes.Agent);
        var agent = agentAccount?.Uid ?? 0;
        
        // Fix double-escaping issue: Ensure group name is unescaped before creating request
        var unescapedGroup = tradeServerGroup?.Replace("\\\\", "\\") ?? string.Empty;
        
        var request = new CreateAccountRequest
        {
            Login = accountNumber,
            Email = user.EmailRaw,
            Country = user.CountryCode.ToLower(),
            Leverage = leverage,
            Password = password,
            PasswordInvestor = passwordInvestor,
            Name = Utils.ToUnicode(user.GetUserName()),
            Phone = user.PhoneNumberRaw,
            PasswordPhone = passwordPhone,
            Status = "RE",
            Group = unescapedGroup,
            Address = user.Address,
            Id = user.IdNumber,
            Comment = $"{comment} {account.Group}",
            Agent = agent,
            Zipcode = account.Group,
        };
        var result = await apiSvc.CreateAccountAsync(tradeServiceId, request);

        if (result.AccountNumber == 0)
        {
            BcrLog.Slack($"CreateTradeAccountForAccountAsync_Failed: {tradeServiceId}, {request}");
            return new TradeAccount.CreatedTradeAccountInfo();
        }

        var tradeAccount = TradeAccount.Build(account.Id, currency);
        tradeAccount.ServiceId = tradeServiceId;
        tradeAccount.AccountNumber = result.AccountNumber;
        tradeAccount.TradeAccountStatus = new TradeAccountStatus
        {
            Balance = 0,
            Leverage = leverage,
            Group = tradeServerGroup,
            ReadOnlyCode = passwordInvestor,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            Currency = Enum.GetName(typeof(CurrencyTypes), currency) ?? "",
        };
        account.HasTradeAccount = true;
        account.TradeAccount = tradeAccount;
        account.AccountNumber = result.AccountNumber;
        account.ServiceId = result.ServiceId;
        try
        {
            await AddAccountTagsAsync(account.Id, AccountTagTypes.DailyConfirmEmail, AccountTagTypes.NewRebateTest);
        }
        catch (Exception e)
        {
            BcrLog.Slack($"CreateTradeAccountForAccountAsync_AddAccountTag_Error: {e.Message}");
        }

        tenantCtx.Accounts.Update(account);
        await tenantCtx.SaveChangesAsync();
        
        // ✅ 新增：保存初始密码到TradeAccountPassword表（加密存储）
        await SaveInitialTradePasswordsAsync(
            account.Id,
            result.AccountNumber,
            tradeServiceId,
            password,
            passwordInvestor,
            passwordPhone
        );
        
        var tenantTradeAccount = await centralCtx.CentralAccounts.FirstOrDefaultAsync(x =>
                x.AccountId == account.Id && _tenantId == x.TenantId);
        if (tenantTradeAccount == null)
        {
            var centralAccount = CentralAccount.Build(_tenantId, account.ServiceId, account.Id, accountNumber);
            await centralCtx.CentralAccounts.AddAsync(centralAccount);
        }
        else
        {
            tenantTradeAccount.AccountNumber = tradeAccount.AccountNumber;
            tenantTradeAccount.ServiceId = tradeServiceId;
            centralCtx.CentralAccounts.Update(tenantTradeAccount);
        }

        await centralCtx.SaveChangesAsync();
        await tenantCtx.SaveChangesAsync();
        return new TradeAccount.CreatedTradeAccountInfo
        {
            AccountId = account.Id,
            AccountNumber = result.AccountNumber,
            Password = password,
            PasswordInvestor = passwordInvestor,
            PasswordPhone = passwordPhone,
        };
    }

    private async Task<long> GenerateMt4AccountNumber(int serviceId, string currencyCode)
    {
        var prefix = await apiSvc.GetAccountPrefix(serviceId, currencyCode);
        prefix = prefix > 0 ? prefix : 55000000;
        while (true)
        {
            // Calculate random range based on trailing zeros in prefix
            var zeros = prefix.ToString().Length - prefix.ToString().TrimEnd('0').Length;
            var mmax = (int)Math.Pow(10, zeros) - 1;
            var number = (long)new Random().Next(0, mmax);
            number += prefix;
            //var exists = await apiSvc.IsAccountNumberExists(serviceId, number) ||
            // await AccountNumberAndUidValidator(number);

            // ✅ Check PostgreSQL database instead of calling MT5 API
            // This avoids unnecessary MT5 api calls during CreateClientAsync() in AutoAccountCreate entry point
            var exists = await AccountNumberAndUidValidator(number);
            
            // ✅ Check MySQL MT5 database for account existence if service is MT5
            // This validates that the account number doesn't exist in MT5 database
            if (!exists)
            {
                var platform = pool.GetPlatformByServiceId(serviceId);
                if (platform == PlatformTypes.MetaTrader5 || platform == PlatformTypes.MetaTrader5Demo)
                {
                    var existsInMt5 = await CheckMt5AccountExistsInMySqlAsync(serviceId, number);
                    if (existsInMt5)
                    {
                        continue; // Account exists in MT5, try another number
                    }
                }
                
                return number;
            }
        }
    }

    /// <summary>
    /// Checks if an MT5 account exists in MySQL database by querying the mt5_accounts table
    /// </summary>
    /// <param name="serviceId">The trade service ID</param>
    /// <param name="accountNumber">The account number (Login) to check</param>
    /// <returns>True if account exists in MT5 MySQL database, false otherwise</returns>
    private async Task<bool> CheckMt5AccountExistsInMySqlAsync(int serviceId, long accountNumber)
    {
        try
        {
            await using var ctx = pool.CreateCentralMT5DbContextAsync(serviceId);
            var exists = await ctx.Mt5Accounts
                .Where(x => x.Login == (ulong)accountNumber)
                .AnyAsync();
            return exists;
        }
        catch (Exception ex)
        {
            // Log error but don't fail account generation if MySQL check fails
            // This ensures account creation can proceed even if MT5 database is temporarily unavailable
            logger.LogWarning(ex, 
                "Failed to check MT5 account existence in MySQL for service {ServiceId}, account {AccountNumber}. " +
                "Proceeding with account generation.", serviceId, accountNumber);
            return false; // Return false to allow account generation to proceed
        }
    }

    public async Task CreateDefaultWalletForAccount(long tradeAccountId)
    {
        var account = await tenantCtx.Accounts
            .Where(x => x.Id == tradeAccountId)
            .Select(x => new { x.PartyId, x.FundType, x.CurrencyId })
            .SingleAsync();

        var exists = await tenantCtx.Wallets
            .Where(x => x.PartyId == account.PartyId && x.CurrencyId == account.CurrencyId &&
                        x.FundType == account.FundType)
            .AnyAsync();
        if (exists) return;

        var isFirstWallet = await tenantCtx.Wallets.AnyAsync(x => x.PartyId == account.PartyId); 
        var wallet = Wallet.Build(account.PartyId, (CurrencyTypes)account.CurrencyId, (FundTypes)account.FundType, isFirstWallet);
        
        tenantCtx.Wallets.Add(wallet);
        await tenantCtx.SaveChangesAsync();
    }

    public async Task ConcurrentUpdateBalanceByPartyId(long partyId)
    {
        var cacheKey = $"account-balance-update-T:{_tenantId}-P:{partyId}";
        var cacheValue = await cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cacheValue))
            return;

        var ctx = pool.CreateTenantDbContext(_tenantId);

        var ids = await ctx.TradeAccounts
            .Where(x => x.IdNavigation.PartyId == partyId)
            .Select(x => x.Id)
            .ToListAsync();
        await ctx.DisposeAsync();

        if (ids.Count == 0)
            return;

        try
        {
            await Task.WhenAll(ids.Select(id => ConcurrentTryUpdateTradeAccountStatus(id)));
        }
        catch
        {
            // ignored
        }

        await cache.SetStringAsync(cacheKey, string.Join(',', ids), TimeSpan.FromSeconds(30));
    }

    public async Task ConcurrentTryUpdateTradeAccountStatus(long accountId, bool fromApi = false)
    {
        await using var ctx = pool.CreateTenantDbContext(_tenantId);
        try
        {
            var account = ctx.TradeAccounts
                .Include(x => x.TradeAccountStatus)
                .FirstOrDefault(x => x.Id == accountId);

            if (account == null)
                return;

            var platform = pool.GetPlatformByServiceId(account.ServiceId);
            var status = platform switch
            {
                PlatformTypes.MetaTrader5 => fromApi
                    ? await GetMt5AccountApiBalance(account.ServiceId, account.AccountNumber)
                    : await GetMt5AccountBalance(account.ServiceId, account.AccountNumber),
                PlatformTypes.MetaTrader4 => await GetMt4AccountBalance(account.ServiceId, account.AccountNumber),
                _ => null
            };

            if (status == null && account.TradeAccountStatus == null)
                return;

            if (status == null && account.TradeAccountStatus != null)
            {
                account.TradeAccountStatus.Equity = 0;
                account.TradeAccountStatus.Balance = 0;
                account.TradeAccountStatus.Credit = 0;
                account.TradeAccountStatus.Margin = 0;
                account.TradeAccountStatus.MarginFree = 0;
                account.TradeAccountStatus.MarginLevel = 0;
            }
            else if (status != null && account.TradeAccountStatus == null)
            {
                account.TradeAccountStatus = new TradeAccountStatus
                {
                    Equity = status.Equity,
                    Balance = status.Balance,
                    Credit = status.Credit,
                    Group = status.Group,
                    Margin = status.Margin,
                    MarginFree = status.MarginFree,
                    MarginLevel = status.MarginLevel
                };
            }
            else if (status != null && account.TradeAccountStatus != null)
            {
                account.TradeAccountStatus.Equity = status.Equity;
                account.TradeAccountStatus.Balance = status.Balance;
                account.TradeAccountStatus.Credit = status.Credit;
                account.TradeAccountStatus.Group = status.Group;
                account.TradeAccountStatus.Margin = status.Margin;
                account.TradeAccountStatus.MarginFree = status.MarginFree;
                account.TradeAccountStatus.MarginLevel = status.MarginLevel;
            }

            account.LastSyncedOn = DateTime.UtcNow;
            account.UpdatedOn = DateTime.UtcNow;
            ctx.TradeAccounts.Update(account);
            await ctx.SaveChangesAsync();
        }
        catch (Exception e)
        {
            // BcrLog.Slack($"TradeAccountUpdateBalance fail. {e.Message}");
            logger.LogInformation("TradeAccountUpdateBalance fail. {e}", e);
        }
    }

    private async Task<TradeAccountStatus?> GetMt5AccountBalance(int serviceId, long accountNumber)
    {
        await using var ctx = pool.CreateCentralMT5DbContextAsync(serviceId);
        var group = await ctx.Mt5Users
            .Where(x => x.Login == (ulong)accountNumber)
            .Select(x => x.Group)
            .FirstOrDefaultAsync();

        var mtAccount = await ctx.Mt5Accounts
            .Where(x => x.Login == (ulong)accountNumber)
            .Select(x => new TradeAccountStatus
            {
                Equity = x.Equity,
                Margin = x.Margin,
                MarginLevel = x.MarginLevel,
                MarginFree = x.MarginFree,
                Credit = x.Credit,
                Balance = x.Balance,
                Leverage = (int)x.MarginLeverage,
                Group = group
            })
            .FirstOrDefaultAsync();

        return mtAccount;
    }

    private async Task<TradeAccountStatus?> GetMt5AccountApiBalance(int serviceId, long accountNumber)
    {
        using var scope = provider.CreateTenantScope(_tenantId);
        var tradingApi = scope.ServiceProvider.GetRequiredService<ITradingApiService>();
        var info = await tradingApi.GetAccountInfoAsync(serviceId, accountNumber);
        var result = new TradeAccountStatus();
        info.ApplyToTradeAccountStatus(result);
        return result;
    }

    private async Task<TradeAccountStatus?> GetMt4AccountBalance(int serviceId, long accountNumber)
    {
        await using var ctx = pool.CreateCentralMT4DbContextAsync(serviceId);
        return await ctx.Mt4Users
            .Where(x => x.Login == (int)accountNumber)
            .Select(x => new TradeAccountStatus
            {
                Balance = x.Balance, PrevBalance = x.Prevbalance, PrevMonthBalance = x.Prevmonthbalance,
                Credit = x.Credit, Equity = x.Equity, Group = x.Group
            })
            .FirstOrDefaultAsync();
    }


    private async Task<bool> AccountNumberAndUidValidator(long uid)
    {
        var result = await tenantCtx.Accounts.AnyAsync(x => x.Uid == uid || x.AccountNumber == uid);
        if (result) return true;
        return await centralCtx.CentralAccounts.AnyAsync(x => x.Uid == uid || x.AccountNumber == uid);
    }
    
    /// <summary>
    /// 保存交易账户的初始密码（加密存储）
    /// 在创建交易账户时调用，永久保留初始密码供管理员查看
    /// </summary>
    private async Task SaveInitialTradePasswordsAsync(
        long accountId,
        long accountNumber,
        int serviceId,
        string mainPassword,
        string investorPassword,
        string phonePassword)
    {
        try
        {
            // 获取加密服务
            var encryptionService = provider.GetService<ITradePasswordEncryptionService>();
            if (encryptionService == null)
            {
                logger.LogError("ITradePasswordEncryptionService not found, cannot save initial passwords for account {AccountId}", accountId);
                return;
            }
            
            // 加密密码
            var encryptedMainPassword = encryptionService.Encrypt(mainPassword);
            var encryptedInvestorPassword = encryptionService.Encrypt(investorPassword);
            var encryptedPhonePassword = encryptionService.Encrypt(phonePassword);
            
            // 创建密码记录
            var passwordRecord = new TradeAccountPassword
            {
                AccountId = accountId,
                AccountNumber = accountNumber,
                ServiceId = serviceId,
                InitialMainPassword = encryptedMainPassword,
                InitialInvestorPassword = encryptedInvestorPassword,
                InitialPhonePassword = encryptedPhonePassword,
                MainPasswordChangedCount = 0,
                InvestorPasswordChangedCount = 0,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };
            
            tenantCtx.TradeAccountPasswords.Add(passwordRecord);
            await tenantCtx.SaveChangesAsync();
            
            logger.LogInformation("Successfully saved initial passwords for account {AccountId}, account number {AccountNumber}", 
                accountId, accountNumber);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save initial passwords for account {AccountId}. Passwords will not be available for admin viewing.", 
                accountId);
            // 不抛出异常，避免影响账户创建流程
            // 管理员将无法查看初始密码，但账户仍然正常创建
        }
    }
}