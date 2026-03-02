using Bacera.Gateway.Auth;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.DTO;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.AccountManage;
using Bacera.Gateway.Vendor.MetaTrader.Models;
using Bacera.Gateway.Web.EventHandlers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Services;

public class AutoCreateAccountService(
    TenantDbContext tenantCtx,
    IMediator mediator,
    ITenantGetter tenantGetter,
    MyDbContextPool pool,
    AccountManageService acctManageSvc,
    ConfigService configSvc,
    AuthDbContext authCtx,
    PaymentMethodService paymentMethodSvc,
    ISendMessageService sendMessageSvc,
    UserService userSvc)
{
    private readonly long _tenantId = tenantGetter.GetTenantId();
    private static readonly string[] ExcludedCountryForIdCheck = ["TW", "MY"];

    public async Task<(bool, string)> TryAutoCreateTradeAccountFromPartyAsync(long partyId)
    {
        if (_tenantId == 10005) return (true, "JP auto create not available");

        var party = await userSvc.GetPartyAsync(partyId);
        if (party.Id == 0) return (false, "Party not found");

        var (isVerificationValid, msg, openAccountInfo) = await ValidateVerificationForAutoCreateAsync(party);
        if (!isVerificationValid) return (false, msg);
        if (openAccountInfo == null) return (false, "Open account info not found");

        var application = CreateDefaultAccountApplicationAsync(party, openAccountInfo);
        return await TryAutoCreateTradeAccountFromApplicationAsync(application);
    }

    public async Task<(bool, string)> TryAutoCreateTradeAccountFromApplicationAsync(Application application)
    {
        if (_tenantId == 10005) return (true, "JP auto create not available");
        if (application.Supplement == null) return (false, "Application supplement not found");

        var maxTradeAccountCount = await GetMaxTradeAccountCountAsync();
        if (maxTradeAccountCount <= await OpenTradeAccountCountAsync(application.PartyId))
            return (false, "Max trade account count reached");

        var openAccountInfo = AutoOpenTradeAccountDTO.ClientAccount.FromApplicationSupplementJson(application.Supplement);
        if (string.IsNullOrWhiteSpace(openAccountInfo.ReferCode))
            return (false, "No ReferCode");

        var party = await userSvc.GetPartyAsync(application.PartyId);

        /* 当前端和后端CreateApplication都加验证后, 自动创建USC Account这里的验证已经多余 */
        // Validate USC Account: Only one USC account per user email
        //if (openAccountInfo.CurrencyId == CurrencyTypes.USC)
        //{
        //    if (string.IsNullOrWhiteSpace(party.Email))
        //        return (false, "User email is required for USC account creation");

        //    // Check if user already has a USC account (by email), one user can only have one USC account
        //    var hasUscAccount = await acctManageSvc.HasUscAccountByEmailAsync(party.Email);
        //    if (hasUscAccount)
        //        return (false, $"User email {party.Email} already has a USC account. Only one USC account per user email is allowed.");
        //}

        if (!ExcludedCountryForIdCheck.Contains(party.CountryCode.ToUpper()) && party.IsIdExpired()) return (false, "User id expired");

        var (isAutoSwitchOn, msg0) = await IsAutoCreateSwitchOnAsync(party, openAccountInfo);
        if (!isAutoSwitchOn) return (false, msg0);

        var (result, msg1, accountId) = await AutoCreateAccountAsync(party, openAccountInfo);
        if (!result) return (false, msg1);

        application.Status = (short)ApplicationStatusTypes.Completed;
        application.UpdatedOn = DateTime.UtcNow;
        application.ReferenceId = accountId;
        if (application.Id != 0)
        {
            tenantCtx.Applications.Update(application);
        }
        else
        {
            tenantCtx.Applications.Add(application);
        }

        await tenantCtx.SaveChangesAsync();
        return (true, "Account created");
    }

    private async Task<(bool, string, long)> AutoCreateAccountAsync(User.TenantDetailModel party,
        AutoOpenTradeAccountDTO.ClientAccount openAccountInfo)
    {
        if (openAccountInfo.CurrencyId == CurrencyTypes.AUD) return (false, "AUD account not supported for auto create", -1);

        var referCode = await tenantCtx.ReferralCodes
            .Where(x => x.Code == openAccountInfo.ReferCode)
            .Select(x => new { x.Id, x.Status, x.AccountId, x.ServiceType, x.Summary })
            .SingleOrDefaultAsync();
        if (referCode == null) return (false, "Refer code not found", -1);

        if (referCode.Status != (short)ReferralCodeStatusTypes.Active)
            return (false, "Refer code not active", -1);

        if (referCode.ServiceType != (int)ReferralServiceTypes.Client)
            return (false, "Refer code not for client", -1);

        var parentAccountId = referCode.AccountId;
        var levelSchema = Utils.JsonDeserializeObjectWithDefault<ReferralCode.ClientCreateSpec>(referCode.Summary).AllowAccountTypes;
        var accountType = levelSchema.FirstOrDefault(x => x.AccountType == openAccountInfo.AccountType);
        if (accountType == null) return (false, "Account type not found", -1);
        if (string.IsNullOrEmpty(accountType.OptionName)) return (false, "Option name not found", -1);

        var (mtGroup, directSchemaId) = await GetMtGroupAndSchemaAsync(party.Id, openAccountInfo.ServiceId, accountType, openAccountInfo.CurrencyId);
        if (string.IsNullOrWhiteSpace(mtGroup) || directSchemaId == null) return (false, "Meta trade group or schema not found", -1);

        var (clientRes, clientMsg) = await acctManageSvc.CreateClientAsync(party.Id, parentAccountId, openAccountInfo.CurrencyId,
            AutoOpenTradeAccountDTO.ClientAccount.FundType, openAccountInfo.AccountType, referCode: openAccountInfo.ReferCode,
            tradeServiceId: openAccountInfo.ServiceId, tenantId: _tenantId);

        if (!clientRes) return (false, clientMsg, -1);

        var accountId = long.Parse(clientMsg);

        // If found DefaultAutoCreatePaymentMethod with above referral code, then create the default payment methods for the account (Deposit)
        var defaultPaymentMethodIds = await configSvc.GetAsync<List<long>>(
            ConfigCategoryTypes.Public,
            referCode.Id,
            ConfigKeys.DefaultAutoCreatePaymentMethod);
            
        if (defaultPaymentMethodIds != null && defaultPaymentMethodIds.Count > 0)
        {
            // Enable each payment method in the list
            foreach (var paymentMethodId in defaultPaymentMethodIds)
            {
                if (paymentMethodId > 0)
                {
                    await paymentMethodSvc.EnableAccountAccessByMethodIdAsync(accountId, paymentMethodId);
                }
            }
        }
        
        // If found DefaultAutoCreateWithdrawalPaymentMethod with above referral code, then create the default payment methods for the account (Withdrawal)
        var defaultWithdrawalPaymentMethodIds = await configSvc.GetAsync<List<long>>(
            ConfigCategoryTypes.Public,
            referCode.Id,
            ConfigKeys.DefaultAutoCreateWithdrawalPaymentMethod);
            
        if (defaultWithdrawalPaymentMethodIds != null && defaultWithdrawalPaymentMethodIds.Count > 0)
        {
            // Enable each payment method in the list
            foreach (var paymentMethodId in defaultWithdrawalPaymentMethodIds)
            {
                if (paymentMethodId > 0)
                {
                    await paymentMethodSvc.EnableAccountAccessByMethodIdAsync(accountId, paymentMethodId);
                }
            }
        }

        await acctManageSvc.AddAccountTagsAsync(accountId, AccountTagTypes.AutoCreate);
        await acctManageSvc.AddAccountLogAsync(accountId, "AutoCreatedBySystem", "0", clientMsg, operatorPartyId: 1);

        var setSchemaRes = await acctManageSvc.SetClientDirectSchemaId(accountId, directSchemaId.Value);
        if (!setSchemaRes) return (false, "Set schema failed", -1);

        await mediator.Publish(new AccountCreatedEvent(accountId));

        var info = await acctManageSvc.CreateTradeAccountAsync(accountId, openAccountInfo.ServiceId, openAccountInfo.Leverage,
            mtGroup, openAccountInfo.CurrencyId, "Auto created");

        await acctManageSvc.CreateOrUpdateKycFormAsync(accountId);

        if (1 == await OpenTradeAccountCountAsync(party.Id))
        {
            await EnableAccountPaymentAccessesAsync(accountId, party);
        }

        await SendReadOnlyNoticeAsync(accountId);
        await acctManageSvc.CreateDefaultWalletForAccount(accountId);
        await mediator.Publish(new TradeAccountCreatedEvent(accountId, info.Password, info.PasswordInvestor, info.PasswordPhone));
        await sendMessageSvc.SendEventToManagerAsync(_tenantId, EventNotice.Build("__ACCOUNT_AUTO_CREATED__", accountId));
        return (true, "Account Auto Created", accountId);
    }

    private async Task<(bool, string)> IsAutoCreateSwitchOnAsync(User.TenantDetailModel party, AutoOpenTradeAccountDTO.ClientAccount info)
    {
        var autoAcEnabled = await configSvc.GetAsync<ApplicationConfigure.BoolValue>(ConfigCategoryTypes.Public, 0,
            ConfigKeys.AutoCreateTradeAccountEnabled);
        if (autoAcEnabled?.Value is null or false) return (false, "Tenant auto create trade account is disabled");

        var isSiteEnabled = await configSvc.GetAsync<ApplicationConfigure.BoolValue>(ConfigCategoryTypes.Public, party.SiteId,
            ConfigKeys.AutoCreateTradeAccountEnabled);
        if (isSiteEnabled?.Value is null or false) return (false, "Site auto create trade account is disabled");

        var agentAccount = await tenantCtx.ReferralCodes
            .Where(x => x.Code == info.ReferCode)
            .Select(x => x.Account)
            .Select(x => new { x.Id, x.SalesAccountId })
            .SingleOrDefaultAsync();
        if (agentAccount == null) return (false, "Agent account not found");

        var isAgentAutoAcEnabled = await configSvc.GetAsync<ApplicationConfigure.BoolValue>(ConfigCategoryTypes.Account, agentAccount.Id,
            ConfigKeys.AutoCreateTradeAccountEnabled);
        if (isAgentAutoAcEnabled?.Value == false) return (false, "Agent auto create trade account is disabled");

        if (agentAccount.SalesAccountId != null)
        {
            var isSalesAutoAcEnabled = await configSvc.GetAsync<ApplicationConfigure.BoolValue>(ConfigCategoryTypes.Account,
                agentAccount.SalesAccountId.Value, ConfigKeys.AutoCreateTradeAccountEnabled);
            if (isSalesAutoAcEnabled?.Value == false) return (false, "Sales auto create trade account is disabled");
        }

        return (true, "");
    }

    /// <summary>
    /// Get the maximum trade account count from configuration
    /// </summary>
    /// <returns>Maximum trade account count (default: 5)</returns>
    private async Task<int> GetMaxTradeAccountCountAsync()
    {
        var config = await configSvc.GetAsync<ApplicationConfigure.IntValue>(
            ConfigCategoryTypes.Public, 
            0, 
            ConfigKeys.MaxTradeAccountCount);
        
        return config?.Value ?? 5; // Default to 5 if not configured
    }

    private async Task SendReadOnlyNoticeAsync(long accountId)
    {
        await Task.Delay(0);
    }

    private async Task<(string?, long?)> GetMtGroupAndSchemaAsync(long partyId, int serviceId, RebateClientRule.RebateAllowedAccountTypes schema, CurrencyTypes currencyId)
    {
        var platform = pool.GetPlatformByServiceId(serviceId);
        var party = await userSvc.GetPartyAsync(partyId);

        switch (platform)
        {
            case PlatformTypes.MetaTrader4 when _tenantId == 10000:
            {
                var configString = await configSvc.GetRawValueAsync(ConfigCategoryTypes.Public, 0,
                    ConfigKeys.Mt4GroupAndSchemaNameForAutoOpenAccount);

                if (string.IsNullOrWhiteSpace(configString)) return (null, null);
                var mt4Config = AutoOpenTradeAccountDTO.Mt4GroupAndSchemaConfig.Build(configString);
                return mt4Config.GetGroupAndSchema(schema.OptionName, schema.Pips ?? 0, schema.Commission ?? 0);
            }
            case PlatformTypes.MetaTrader4 when _tenantId == 10004:
            {
                var configString = await configSvc.GetRawValueAsync(ConfigCategoryTypes.Public, party.SiteId,
                    ConfigKeys.Mt4GroupAndSchemaNameForAutoOpenAccount);

                if (string.IsNullOrWhiteSpace(configString)) return (null, null);
                var mt4Config = AutoOpenTradeAccountDTO.Mt4GroupAndSchemaConfig.Build(configString);
                return mt4Config.GetGroupAndSchema(schema.OptionName, schema.Pips ?? 0, schema.Commission ?? 0);
            }
            case PlatformTypes.MetaTrader5 when _tenantId == 10000:
            {
                var configString = await configSvc.GetRawValueAsync(ConfigCategoryTypes.Public, 0,
                    ConfigKeys.Mt5GroupAndSchemaNameForAutoOpenAccount);

                if (string.IsNullOrWhiteSpace(configString)) return (null, null);
                var mt5Config = AutoOpenTradeAccountDTO.Mt5GroupAndSchemaConfig.Build(configString);
                return mt5Config.GetGroupAndSchema(schema.OptionName, schema.Pips ?? 0, schema.Commission ?? 0, currencyId);
            }

            case PlatformTypes.MetaTrader5 when _tenantId == 10004:
            {
                var configString = await configSvc.GetRawValueAsync(ConfigCategoryTypes.Public, party.SiteId,
                    ConfigKeys.Mt5GroupAndSchemaNameForAutoOpenAccount);

                if (string.IsNullOrWhiteSpace(configString)) return (null, null);
                var mt5Config = AutoOpenTradeAccountDTO.Mt5GroupAndSchemaConfig.Build(configString);
                return mt5Config.GetGroupAndSchema(schema.OptionName, schema.Pips ?? 0, schema.Commission ?? 0, currencyId);
            }
        }

        return (null, null);
    }

    private async Task<(bool, string, AutoOpenTradeAccountDTO.ClientAccount?)> ValidateVerificationForAutoCreateAsync(User.TenantDetailModel party)
    {
        var verification = await tenantCtx.Verifications
            .Where(x => x.PartyId == party.Id && x.Type == (short)VerificationTypes.Verification)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();
        if (verification == null) return (false, "Verification not found", null);

        var itemStrings = await tenantCtx.VerificationItems
            .Where(x => x.VerificationId == verification.Id)
            .ToDictionaryAsync(x => x.Category, x => x.Content);

        var infoString = itemStrings.GetValueOrDefault(VerificationCategoryTypes.Info);
        if (string.IsNullOrWhiteSpace(infoString)) return (false, "Verification Info not found", null);

        if (!VerificationInfoDTO.TryParse(infoString, out var info)) return (false, "Verification Info not valid", null);

        var user = await authCtx.Users.SingleAsync(x => x.TenantId == _tenantId && x.PartyId == party.Id);
        user.Address = info.Address;
        user.Birthday = info.Birthday;
        user.IdNumber = info.IdNumber;
        user.IdType = info.IdType;
        user.IdIssuer = info.IdIssuer;
        // user.CCC = info.Ccc;
        // user.PhoneNumber = info.Phone;
        user.IdIssuedOn = info.IdIssuedOn ?? DateOnly.MinValue;
        user.IdExpireOn = info.IdExpireOn ?? DateOnly.MinValue;
        authCtx.Users.Update(user);
        await authCtx.SaveChangesAsync();
        await userSvc.UpdateSearchAsync(new User.Criteria { PartyId = party.Id, TenantId = _tenantId });

        var docItemString = itemStrings.GetValueOrDefault(VerificationCategoryTypes.Document);
        if (string.IsNullOrWhiteSpace(docItemString)) return (false, "Document not found", null);

        var docItems = VerificationDocumentMedium.FromJson(docItemString);
        var validTypes = party.SiteId switch
        {
            (int)SiteTypes.China => VerificationDocumentTypes.ValidForCNAutoCreate,
            _ => VerificationDocumentTypes.ValidForAutoCreate,
        };

        var validAuth = validTypes.All(x => docItems.Any(y => y.DocumentType == x));
        if (!validAuth) return (false, "Verification documents not valid", null);

        var startInfo = itemStrings.GetValueOrDefault(VerificationCategoryTypes.Started);
        if (string.IsNullOrWhiteSpace(startInfo)) return (false, "Verification started not found", null);

        if (!AutoOpenTradeAccountDTO.ClientAccount.TryParse(startInfo, out var openAccountInfo))
            return (false, "Verification started not valid", null);

        return (true, "Verification valid", openAccountInfo);
    }

    private static Application CreateDefaultAccountApplicationAsync(User.TenantDetailModel user,
        AutoOpenTradeAccountDTO.ClientAccount openAccountInfo)
    {
        var supplement = ApplicationSupplement.Build(AccountRoleTypes.Client, FundTypes.Ips,
            accountType: openAccountInfo.AccountType, leverage: openAccountInfo.Leverage,
            currency: openAccountInfo.CurrencyId, platform: openAccountInfo.Platform, serviceId: openAccountInfo.ServiceId,
            referCode: user.ReferCode);

        var application = new Application
        {
            PartyId = user.PartyId,
            Type = (short)ApplicationTypes.TradeAccount,
            Status = (short)ApplicationStatusTypes.AwaitingApproval,
            UpdatedOn = DateTime.UtcNow,
            CreatedOn = DateTime.UtcNow,
            Supplement = supplement.ToJson(),
        };

        // tenantCtx.Applications.Add(application);
        // await tenantCtx.SaveChangesAsync();
        return application;
    }

    private async Task EnableAccountPaymentAccessesAsync(long accountId, User.TenantDetailModel party)
    {
        var paymentMethodIds = await tenantCtx.Accounts
            .Where(x => x.Id != accountId)
            .Where(x => x.PartyId == party.Id && x.Status == 0 && x.Role == (int)AccountRoleTypes.Client)
            .OrderByDescending(x => x.Id)
            .SelectMany(x => x.AccountPaymentMethodAccesses)
            .Select(x => x.PaymentMethodId)
            .Distinct()
            .ToListAsync();

        if (paymentMethodIds.Count == 0)
            paymentMethodIds = await configSvc.GetAsync<List<long>>(ConfigCategoryTypes.Public, party.SiteId,
                                   ConfigKeys.PaymentAccessNamesForAutoOpenAccount)
                               ?? await configSvc.GetAsync<List<long>>(ConfigCategoryTypes.Public, 0,
                                   ConfigKeys.PaymentAccessNamesForAutoOpenAccount);
        
        if (paymentMethodIds == null || paymentMethodIds.Count == 0) return;

        foreach (var paymentMethodId in paymentMethodIds)
        {
            await paymentMethodSvc.EnableAccountAccessByMethodIdAsync(accountId, paymentMethodId);
        }
    }

    private async Task<int> OpenTradeAccountCountAsync(long partyId)
    {
        var count = await tenantCtx.Accounts
            .Where(x => x.Role == (int)AccountRoleTypes.Client)
            .Where(x => x.PartyId == partyId && x.AccountNumber > 0 && x.Status == 0)
            .CountAsync();
        return count;
    }
}