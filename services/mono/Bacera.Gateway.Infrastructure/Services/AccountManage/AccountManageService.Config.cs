using Bacera.Gateway.Core.Types;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services.AccountManage;

public partial class AccountManageService
{
    public async Task<List<int>> GetAccountAvailableLeverages(long accountId)
    {
        var account = await tenantCtx.Accounts
            .Where(x => x.Id == accountId)
            .Select(x => new { x.Id, x.PartyId, x.SiteId, PartySiteId = x.Party.SiteId })
            .SingleAsync();

        return await configSvc.GetAsync<List<int>>(nameof(Account), account.Id, ConfigKeys.LeverageAvailable)
               ?? await configSvc.GetAsync<List<int>>(nameof(Party), account.PartyId, ConfigKeys.LeverageAvailable)
               ?? await configSvc.GetAsync<List<int>>(nameof(Public), account.PartySiteId, ConfigKeys.LeverageAvailable)
               ?? await configSvc.GetAsync<List<int>>(nameof(Public), 0, ConfigKeys.LeverageAvailable)
               ?? [];
    }

    public async Task<List<AccountTypes>> GetAvailableAccountTypes(long partyId)
    {
        var items = await configSvc.GetAsync<List<AccountTypes>>(nameof(Party), partyId, ConfigKeys.AccountTypeAvailable);
        if (items != null) return items;
        var party = await userSvc.GetPartyAsync(partyId);
        var isMLM = await userSvc.HasRoleAsync(partyId, UserRoleTypesString.MLM);
        if (isMLM)
        {
            return await configSvc.GetAsync<List<AccountTypes>>(nameof(Public), party.SiteId, ConfigKeys.AccountTypeAvailable)
                   ?? await configSvc.GetAsync<List<AccountTypes>>(nameof(Public), 0, ConfigKeys.AccountTypeAvailable)
                   ?? [];
        }
        return await referralCodeSvc.GetAccountTypesInReferralCodeAsync(party.ReferCode)
               ?? await configSvc.GetAsync<List<AccountTypes>>(nameof(Public), party.SiteId, ConfigKeys.AccountTypeAvailable)
               ?? await configSvc.GetAsync<List<AccountTypes>>(nameof(Public), 0, ConfigKeys.AccountTypeAvailable)
               ?? [];
    }
    
    public async Task<List<AccountTypes>> GetDemoAvailableAccountTypes(long partyId)
    {
        var items = await configSvc.GetAsync<List<AccountTypes>>(nameof(Party), partyId, ConfigKeys.DemoAccountTypeAvailable);
        if (items != null) return items;
        
        var configTypes = await configSvc.GetAsync<List<AccountTypes>>(nameof(Public), 0, ConfigKeys.DemoAccountTypeAvailable)?? new List<AccountTypes>();
        
        var party = await userSvc.GetPartyAsync(partyId);
        var partyTypes = await referralCodeSvc.GetAccountTypesInReferralCodeAsync(party.ReferCode)?? [];

        return configTypes.Union(partyTypes).ToList();
    }

    public async Task<List<TradeService.ClientTradingPlatformAvailableModel>> GetAvailableTradingPlatforms(long partyId)
    {
        var party = await userSvc.GetPartyAsync(partyId);
        var items = await configSvc.GetAsync<List<int>>(nameof(Party), partyId, ConfigKeys.TradingPlatformAvailable)
                    ?? await configSvc.GetAsync<List<int>>(nameof(Public), party.SiteId, ConfigKeys.TradingPlatformAvailable)
                    ?? await configSvc.GetAsync<List<int>>(nameof(Public), 0, ConfigKeys.TradingPlatformAvailable)
                    ?? [];

        var result = items.Select(x => new TradeService.ClientTradingPlatformAvailableModel
        {
            ServiceId = x,
            Platform = pool.GetPlatformByServiceId(x)
        }).ToList();

        return result;
    }

    public async Task<List<TradeService.ClientTradingPlatformAvailableModel>> GetAvailableDemoTradingPlatforms(long partyId)
    {
        var party = await userSvc.GetPartyAsync(partyId);
        var items = await configSvc.GetAsync<List<int>>(nameof(Party), partyId, ConfigKeys.DemoTradingPlatformAvailable)
                    ?? await configSvc.GetAsync<List<int>>(nameof(Public), party.SiteId, ConfigKeys.DemoTradingPlatformAvailable)
                    ?? await configSvc.GetAsync<List<int>>(nameof(Public), 0, ConfigKeys.DemoTradingPlatformAvailable)
                    ?? [];

        var result = tenantCtx.TradeServices
            .Where(x => items.Contains(x.Platform))
            .Select(x => new TradeService.ClientTradingPlatformAvailableModel
            {
                ServiceId = x.Id,
                Platform = (PlatformTypes)x.Platform
            })
            .ToList();
       
        return result;
    }

    public async Task<List<CurrencyTypes>> GetAvailableCurrencyTypes(long partyId)
    {
        var party = await userSvc.GetPartyAsync(partyId);
        var currencies = await configSvc.GetAsync<List<CurrencyTypes>>(nameof(Party), partyId, ConfigKeys.CurrencyAvailable)
                         ?? await configSvc.GetAsync<List<CurrencyTypes>>(nameof(Public), party.SiteId, ConfigKeys.CurrencyAvailable)
                         ?? await configSvc.GetAsync<List<CurrencyTypes>>(nameof(Public), 0, ConfigKeys.CurrencyAvailable)
                         ?? [];

        // Remove USC (841) from available currencies if user already has a USC account
        if (currencies.Contains(CurrencyTypes.USC))
        {
            var hasUscAccount = await HasUscAccountByEmailAsync(party.Email);
            if (hasUscAccount)
            {
                currencies = currencies.Where(x => x != CurrencyTypes.USC).ToList();
            }
        }

        return currencies;
    }
    
    public async Task<List<CurrencyTypes>> GetDemoAvailableCurrencyTypes(long partyId)
    {
        var party = await userSvc.GetPartyAsync(partyId);
        return await configSvc.GetAsync<List<CurrencyTypes>>(nameof(Party), partyId, ConfigKeys.DemoCurrencyAvailable)
               ?? await configSvc.GetAsync<List<CurrencyTypes>>(nameof(Public), party.SiteId, ConfigKeys.DemoCurrencyAvailable)
               ?? await configSvc.GetAsync<List<CurrencyTypes>>(nameof(Public), 0, ConfigKeys.DemoCurrencyAvailable)
               ?? [];
    }

    public async Task<List<int>> GetAvailableLeverages(long partyId)
    {
        var party = await userSvc.GetPartyAsync(partyId);
        return await configSvc.GetAsync<List<int>>(nameof(Party), partyId, ConfigKeys.LeverageAvailable)
               ?? await configSvc.GetAsync<List<int>>(nameof(Public), party.SiteId, ConfigKeys.LeverageAvailable)
               ?? await configSvc.GetAsync<List<int>>(nameof(Public), 0, ConfigKeys.LeverageAvailable)
               ?? [];
    }
}