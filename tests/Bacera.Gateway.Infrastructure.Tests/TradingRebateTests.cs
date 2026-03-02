// using Microsoft.EntityFrameworkCore;
// using Shouldly;
//
// namespace Bacera.Gateway.Infrastructure.Tests;
//
// [Trait(TraitTypes.Types, TraitTypes.Value.Service)]
// [Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]
// public class TradingRebateTests : Startup, IClassFixture<SharedTradingTestFixture>
// {
//     private readonly TradingService _svc;
//     private readonly SharedTradingTestFixture _sharedTestFixture;
//
//     public TradingRebateTests(SharedTradingTestFixture sharedTestFixture)
//     {
//         _sharedTestFixture = sharedTestFixture;
//         _svc = sharedTestFixture.TradingSvc;
//         _svc.ShouldNotBeNull();
//     }
//
//     // [Fact]
//     // public async Task CalculateDirectRebates_WithoutFormula_ShouldBeValued()
//     // {
//     //     var agents = await generateRebateAccounts_TenLevel_ReturnList();
//     //     var account =
//     //         await generateClientAccount_WithAgentAccountId_ReturnAccount(RebateRuleTypes.Direct, agents.Last().Id);
//     //
//     //     var rule = await buildClientRebateRule(account.Id);
//     //     await buildDirectRule(rule.Id, agents.First().Id);
//     //     await buildDirectRule(rule.Id, agents.Skip(1).First().Id);
//     //
//     //     var tradeRebates = await generateTradeRebatesAsync_ForFakeAccount_ResultShouldBeGreaterThenZero(account.Id,
//     //         account.TradeAccount!.AccountNumber);
//     //     var result = await _svc.CalculateMt4DirectRebates(tradeRebates.First());
//     //     result.ShouldNotBeNull();
//     //     result.Any().ShouldBeTrue();
//     //
//     //     var svcResult = await _svc.ProcessRebate(tradeRebates.Skip(1).First().Id);
//     //     svcResult.ShouldBe(RebateProcessResultCodeType.Success);
//     //
//     //     var agent = agents.Last();
//     //     var rebateQueryResult = await _svc.GetLastRebateForIbAsync(agent.Uid, agent.PartyId);
//     //     rebateQueryResult.ShouldNotBeNull();
//     // }
//
//     // [Fact]
//     // public async Task CalculateLevelRebates_WithoutFormula_ShouldBeValued()
//     // {
//     //     var agents = await generateRebateAccounts_TenLevel_ReturnList();
//     //     var account =
//     //         await generateClientAccount_WithAgentAccountId_ReturnAccount(RebateRuleTypes.Level, agents.Last().Id);
//     //
//     //     var tradeRebates = await generateTradeRebatesAsync_ForFakeAccount_ResultShouldBeGreaterThenZero(account.Id,
//     //         account.TradeAccount!.AccountNumber);
//     //
//     //     var rebateResult = await CalculateLevelRebates(tradeRebates.First(), agents);
//     //     rebateResult.Any().ShouldBeTrue();
//     // }
//     //
//     // [Fact]
//     // public async Task StartProcessSingleTradeRebateTest()
//     // {
//     //     // await GenerateTradeRebatesAsync_ForFakeAccount_ResultShouldBeGreaterThenZero();
//     //     var seq = await tenantDbContext.TradeRebates
//     //         .Where(x => x.Status == (short)TradeRebateStatusTypes.Created)
//     //         .FirstOrDefaultAsync();
//     //     seq.ShouldNotBeNull();
//     //     var item = await _svc.ProcessRebate(seq.Id);
//     //     item.ShouldBe(RebateProcessResultCodeType.Success);
//     //     var trade = await tenantDbContext.TradeRebates.FindAsync(seq.Id);
//     //     trade.ShouldNotBeNull();
//     //     trade.Status.ShouldBe((short)TradeRebateStatusTypes.Completed);
//     // }
//     //
//     // private async Task<List<TradeRebate>> generateTradeRebatesAsync_ForFakeAccount_ResultShouldBeGreaterThenZero(
//     //     long accountId,
//     //     long accountNumber, int count = 10)
//     // {
//     //     var trades = SharedTradingTestFixture.FakeTrades(accountId, accountNumber, count);
//     //     await tenantDbContext.TradeTransactions.AddRangeAsync(trades);
//     //     await tenantDbContext.SaveChangesAsync();
//     //
//     //     (await tenantDbContext.Symbols.AnyAsync()).ShouldBeTrue();
//     //     (await tenantDbContext.SymbolCategories.AnyAsync()).ShouldBeTrue();
//     //     (await tenantDbContext.TradeTransactions.AnyAsync()).ShouldBeTrue();
//     //
//     //     var tradeRebates = await _svc.GenerateTradeRebatesAsync(0, DateTime.UtcNow.AddYears(-20));
//     //     tradeRebates.ShouldNotBeNull();
//     //     tradeRebates.Count.ShouldBeGreaterThan(0);
//     //     await tenantDbContext.TradeRebates.AddRangeAsync(tradeRebates);
//     //     await tenantDbContext.SaveChangesAsync();
//     //     return tradeRebates.ToList();
//     // }
//     //
//     // [Fact]
//     // public async Task BatchProcessTradeRebateTest()
//     // {
//     //     var query = tenantDbContext.TradeRebates
//     //             .Where(x => x.Status == (short)TradeRebateStatusTypes.Created)
//     //             .Select(x => x.Id)
//     //             .Take(100)
//     //         ;
//     //     var i = 0;
//     //     while (await query.AnyAsync() && i < 10)
//     //     {
//     //         var items = await query.ToListAsync();
//     //         foreach (var item in items)
//     //         {
//     //             var result = await _svc.ProcessRebate(item);
//     //             result.ShouldBe(RebateProcessResultCodeType.Success);
//     //         }
//     //
//     //         i++;
//     //     }
//     // }
//     //
//     // private decimal getPercentage()
//     //     => decimal.Round(Faker.Finance.Amount(min: 0.01m, max: 0.23m, decimals: 2), 2, MidpointRounding.AwayFromZero);
//     //
//     // private IList<RebateBaseSchemaItem> fakeRuleInfo(IEnumerable<int> symbolIds)
//     //     => symbolIds.Select(x => new RebateBaseSchemaItem(x, getPercentage(), getPercentage(), 0))
//     //         .ToList();
//     //
//     // private async Task accountQuery_OrderById_HasReturn()
//     // {
//     //     var criteria = new Account.Criteria { SortField = "Id" };
//     //     var result = await _svc.AccountQueryAsync(criteria);
//     //     result.ShouldNotBeNull();
//     //     if (result.Criteria.Total > 1)
//     //     {
//     //         result.Data.First().Id.ShouldBeGreaterThan(result.Data.Skip(1).First().Id);
//     //     }
//     //
//     //     result.Criteria.Total.ShouldBeGreaterThan(0);
//     //     var id = result.Data.First().Id;
//     //     var item = await _svc.AccountGetAsync(id);
//     //     item.ShouldNotBeNull();
//     //     item.Id.ShouldBe(id);
//     // }
//     //
//     // private async Task tradeAccountQueryAsync_OrderById_HasReturn()
//     // {
//     //     var criteria = new TradeAccount.Criteria { SortField = "Id" };
//     //     var result = await _svc.TradeAccountQueryAsync(criteria);
//     //     result.ShouldNotBeNull();
//     //     result.Criteria.Total.ShouldBeGreaterThan(0);
//     //
//     //     var id = result.Data.First().Id;
//     //     var item = await _svc.AccountGetAsync(id);
//     //     item.ShouldNotBeNull();
//     //     item.Id.ShouldBe(id);
//     // }
//     //
//     // private async Task<List<RebateAccount>> generateRebateAccounts_TenLevel_ReturnList()
//     // {
//     //     var parentAgentId = 0L;
//     //     var rebateAccounts = new List<RebateAccount>();
//     //     for (var i = 0; i < 10; i++)
//     //     {
//     //         var agent = await _sharedTestFixture.FakeAccount(UserRoleTypes.IB, AccountRoleTypes.Agent,
//     //             i == 0 ? null : parentAgentId);
//     //         await tenantDbContext.Accounts.AddAsync(agent);
//     //         await tenantDbContext.SaveChangesAsync();
//     //         parentAgentId = agent.Id;
//     //
//     //         await buildAgentRebateRule(agent.Id);
//     //
//     //         rebateAccounts.Add(RebateAccount.From(agent, i));
//     //     }
//     //
//     //     rebateAccounts.Count.ShouldBe(10);
//     //     return rebateAccounts;
//     // }
//     //
//     // private async Task<AgentRebateRule> buildAgentRebateRule(long accountId)
//     // {
//     //     var symbolCategoryIds = await tenantDbContext.SymbolCategories.Select(x => x.Id).ToListAsync();
//     //     symbolCategoryIds.Any().ShouldBeTrue();
//     //     var allocationSchemas = SharedTradingTestFixture.FakeAllocationSchemaItems(symbolCategoryIds);
//     //
//     //     // fake rebate rule
//     //     var rebateRule = SharedTradingTestFixture.FakeAgentRebateRule(accountId, allocationSchemas);
//     //     await tenantDbContext.AgentRebateRules.AddAsync(rebateRule);
//     //     await tenantDbContext.SaveChangesAsync();
//     //     return rebateRule;
//     // }
//     //
//     // private async Task<ClientRebateRule> buildClientRebateRule(long accountId)
//     // {
//     //     var symbolCategoryIds = await tenantDbContext.SymbolCategories.Select(x => x.Id).ToListAsync();
//     //     symbolCategoryIds.Any().ShouldBeTrue();
//     //     var baseSchemas = SharedTradingTestFixture.FakeBaseSchemaItems(symbolCategoryIds);
//     //
//     //     // fake rebate rule
//     //     var rebateRule = SharedTradingTestFixture.FakeClientRebateRule(accountId, baseSchemas);
//     //     await tenantDbContext.ClientRebateRules.AddAsync(rebateRule);
//     //     await tenantDbContext.SaveChangesAsync();
//     //     return rebateRule;
//     // }
//     //
//     // private async Task<DirectRebateRule> buildDirectRule(long sourceAccountId, long targetAccountId)
//     // {
//     //     var symbolCategoryIds = await tenantDbContext.SymbolCategories.Select(x => x.Id).ToListAsync();
//     //     symbolCategoryIds.Any().ShouldBeTrue();
//     //     var directRule =
//     //         SharedTradingTestFixture.FakeDirectRule(sourceAccountId, targetAccountId,
//     //             1); // TODO: fix new class model; temporary set to 1
//     //     await tenantDbContext.DirectRebateRules.AddAsync(directRule);
//     //     await tenantDbContext.SaveChangesAsync();
//     //     return directRule;
//     // }
//     //
//     // private async Task<Account> generateClientAccount_WithAgentAccountId_ReturnAccount(RebateRuleTypes rebateRuleType,
//     //     long agentAccountId)
//     // {
//     //     var account =
//     //         await _sharedTestFixture.FakeAccount(UserRoleTypes.Client, AccountRoleTypes.Client,
//     //             agentAccountId);
//     //     await tenantDbContext.Accounts.AddAsync(account);
//     //     await tenantDbContext.SaveChangesAsync();
//     //
//     //     account.Id.ShouldBeGreaterThan(0);
//     //     account.AgentAccountId.ShouldBe(agentAccountId);
//     //
//     //     return account;
//     // }
//
//     [Fact]
//     private async Task TradingService_CalculateLevelRebates_ShouldBeNotNull()
//     {
//         await Task.Delay(0);
//         return new List<Rebate>();
//         //var rebates = new List<Rebate>();
//         //var rule = await tenantDbContext.ClientRebateRules.FirstOrDefaultAsync(x => x.Id == tradeRebate.Trade.TradeAccountId);
//         //if (rule == null)
//         //    return rebates;
//
//         //var schema = rule.GetBaseRuleForSymbolCategory(tradeRebate.SymbolCategoryId);
//         //var formula = await _svc.GetFormulaBySymbolIdAsync(tradeRebate.SymbolId);
//         //var baseRebate = schema == null
//         //    ? TradingService.BaseRebate.Empty
//         //    : TradingService.CalculateBaseRebate(tradeRebate.Volume, schema, formula);
//         //var total = baseRebate.Total;
//         //var remain = baseRebate.Total;
//
//         //agentAccounts = agentAccounts
//         //    .OrderBy(x => x.Depth).TakeLast(5).ToList();
//
//         //var parentAccount = agentAccounts.First();
//
//         //// start from the second agent
//         //for (var index = 1; index < agentAccounts.Count; index++)
//         //{
//         //    long amount;
//         //    TradingService.RebateBreakdownForTrade breakdown;
//         //    var agent = agentAccounts[index];
//
//         //    // remind amount for the current client who make the trade
//         //    if (index != agentAccounts.Count - 1)
//         //    {
//         //        var levelRule = await tenantDbContext.AgentRebateRules
//         //            .Where(x => x.AgentAccountId == agent.AgentAccountId)
//         //            .AsNoTracking()
//         //            .FirstOrDefaultAsync();
//         //        if (levelRule == null)
//         //            continue;
//         //        var rebateItem = new TradingService.LevelRebate();
//         //        var levelSchema = levelRule.GetAllocationSchemaItem(tradeRebate.SymbolCategoryId);
//         //        if (levelSchema != null)
//         //        {
//         //            rebateItem = TradingService.CalculateLevelRebateBySchema(remain, levelSchema, formula);
//         //        }
//
//         //        // calculate the amount for the current agent
//         //        amount = rebateItem.Total;
//         //        breakdown = TradingService.RebateBreakdownForTrade.Build(total, rebateItem.Total, 1m, agent.Depth);
//         //    }
//         //    else
//         //    {
//         //        amount = remain;
//         //        breakdown = TradingService.RebateBreakdownForTrade.Build(total, remain, 1, agent.Depth);
//         //    }
//         //    //
//         //    // if (amount == 0)
//         //    //     continue;
//
//         //    var rebate = new Rebate
//         //    {
//         //        Id = 0,
//         //        PartyId = parentAccount.PartyId,
//         //        AccountId = parentAccount.Id,
//         //        TradeId = tradeRebate.TradeId,
//         //        Amount = amount,
//         //        CurrencyId = tradeRebate.CurrencyId,
//         //        HoldUntilOn = DateTime.UtcNow.AddMonths(1),
//         //        IdNavigation = Matter.Build().Rebate().SetState(StateTypes.RebateOnHold),
//         //        Information = breakdown.ToJson()
//         //    };
//         //    rebates.Add(rebate);
//         //    remain -= amount;
//         //    parentAccount = agent;
//         //}
//
//         //return rebates;
//     }
// }