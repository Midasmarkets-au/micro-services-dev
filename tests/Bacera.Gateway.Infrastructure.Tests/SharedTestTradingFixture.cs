using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Bacera.Gateway.Infrastructure.Tests;

public class SharedTradingTestFixture : Startup
{
    public TradingService TradingSvc;

    public SharedTradingTestFixture()
    {
        Init().Wait();
        TradingSvc = ServiceProvider.GetRequiredService<TradingService>();
    }

    private async Task Init()
    {
        await TenantDbContext.SeedSymbolsAsync();
        await TenantDbContext.SeedTradeServiceAsync();
    }

    public async Task<Account> FakeAccount(UserRoleTypes type, AccountRoleTypes accountRole,
        long? agentAccountId = default, long? salesAccountId = default)
    {
        var party = await FakeParty(type);
        var account = new Faker<Account>()
            .RuleFor(x => x.Uid, f => f.Random.Long(1000000, 10000000))
            .RuleFor(x => x.PartyId, party.Id)
            .RuleFor(x => x.Role, (short)accountRole)
            .RuleFor(x => x.Status, (short)AccountStatusTypes.Activate)
            .RuleFor(x => x.TradeAccount, FakeTradeAccount())
            .RuleFor(x => x.HasTradeAccount, true)
            .RuleFor(x => x.Code, f => "CODE_" + f.Internet.UserName().ToUpper())
            .RuleFor(x => x.Group, f => "GROUP_" + f.Internet.UserName().ToUpper())
            .RuleFor(x => x.Name, f => "Name_" + f.Internet.UserName().ToUpper())
            .RuleFor(x => x.ReferPath, string.Empty)
            .Generate();
        return account;
    }

    public static TradeAccount FakeTradeAccount()
    {
        var tradeAccount = new Faker<TradeAccount>()
            .RuleFor(x => x.Id, f => f.Random.Long(1000, 100000))
            .RuleFor(x => x.ServiceId, 1)
            .RuleFor(x => x.CurrencyId, (int)CurrencyTypes.USD)
            .RuleFor(x => x.AccountNumber, f => f.Random.Long(1000, 100000))
            .Generate();
        return tradeAccount;
    }

    public static RebateClientRule FakeClientRebateRule(long accountId,
        List<RebateLevelSchemaItem>? baseSchemas = default)
    {
        var rebateRule = new Faker<RebateClientRule>()
            .RuleFor(x => x.ClientAccountId, accountId)
            .Generate();
        return rebateRule;
    }

    public static RebateAgentRule FakeAgentRebateRule(long accountId,
        List<RebateLevelSchema>? schemas = default)
    {
        var rebateRule = new Faker<RebateAgentRule>()
            .RuleFor(x => x.AgentAccountId, accountId)
            .RuleFor(x => x.Schema, schemas != null
                ? JsonConvert.SerializeObject(schemas)
                : "[]")
            .Generate();
        return rebateRule;
    }

    public static RebateDirectRule FakeDirectRule(long sourceAccountId, long targetAccountId, long rebateRuleId)
    {
        var rule = new Faker<RebateDirectRule>()
            .RuleFor(x => x.SourceTradeAccountId, sourceAccountId)
            .RuleFor(x => x.TargetAccountId, targetAccountId)
            .RuleFor(x => x.RebateDirectSchemaId, rebateRuleId)
            .Generate();
        return rule;
    }

    public static List<RebateLevelSchemaItem> FakeBaseSchemaItems(List<int> symbolCategoryIds, decimal? rate = default)
    {
        var schemas = new Faker<RebateLevelSchemaItem>()
            .RuleFor(x => x.CategoryId, 0)
            .RuleFor(x => x.Rate, f => rate ?? RandomPercentage())
            .Generate(symbolCategoryIds.Count);
        for (var i = 0; i < symbolCategoryIds.Count; i++)
        {
            schemas.ElementAt(i).CategoryId = symbolCategoryIds.ElementAt(i);
        }

        return schemas;
    }

    public static ExchangeRate FakeExchangeRate(CurrencyTypes fromCurrency, CurrencyTypes toCurrency,
        decimal? rate = default,
        decimal? withdrawalRate = default, decimal? adjustRate = default)
    {
        return new ExchangeRate
        {
            FromCurrencyId = (int)fromCurrency,
            ToCurrencyId = (int)toCurrency,
            BuyingRate = rate ?? RandomPercentage(),
            SellingRate = withdrawalRate ?? RandomPercentage(),
            AdjustRate = adjustRate ?? RandomPercentage()
        };
    }

    private static decimal RandomPercentage(decimal? from = null, decimal? to = null) =>
        Math.Round(new Faker().Random.Decimal(from ?? 0.001m, to ?? 0.5m), 4);
}