using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

[Trait(TraitTypes.Types, TraitTypes.Value.Service)]
[Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]
public class TradingServiceTests : Startup, IClassFixture<SharedTradingTestFixture>
{
    private readonly TradingService _svc;
    private readonly SharedTradingTestFixture _sharedTestFixture;

    public TradingServiceTests(SharedTradingTestFixture sharedTestFixture)
    {
        _sharedTestFixture = sharedTestFixture;
        _svc = sharedTestFixture.TradingSvc;
    }

    [Fact]
    public async Task CreateWithTradeAccountAsync_ForDemoAccount_Success()
    {
        var party = await FakeParty(UserRoleTypes.Client);
        party.ShouldNotBeNull();
        party.Id.ShouldBeGreaterThan(0);

        var service = await _svc.GetServiceByPlatformAsync(PlatformTypes.MetaTrader4Demo);
        service.ShouldNotBeNull();
        service.Platform.ShouldBe((short)PlatformTypes.MetaTrader4Demo);

        var account = await _svc.CreateWithTradeAccountAsync(AccountRoleTypes.Client, party.Id,
            party.Name, Guid.NewGuid().ToString("N")[..12], Guid.NewGuid().ToString("N")[..8],
            service.Id, "Pass!1234", 100, "demoUSD_STN", AccountTypes.Unknown, CurrencyTypes.USD);
        account.ShouldNotBeNull();
        account.Id.ShouldBeGreaterThan(0);
        account.HasTradeAccount.ShouldBeTrue();
        account.TradeAccount.ShouldNotBeNull();
        account.TradeAccount.AccountNumber.ShouldBeGreaterThan(0);
        await TradeAccountQueryAsync_OrderById_HasReturn();
    }

    [Fact]
    public async Task PointAdd_Get_ShouldBeZero()
    {
        // var party = await FakeParty(UserRoleTypes.IB);
        // var item = await _svc.IbAccountCreateAsync(party.Id, FundTypes.Wire, CurrencyTypes.Invalid, party.Name
        //     , code: Guid.NewGuid().ToString()[..10], group: Guid.NewGuid().ToString()[..10]);
        // item.Role.ShouldBe((short)AccountRoleTypes.Agent);
        //
        // var point = await _svc.PointAddAsync(item.Id, 0, AccountPointTransactionTypes.Unknown, "Test    ");
        // point.ShouldBe(0L);
        //
        // var pointAdded = await _svc.PointAddAsync(item.Id, 100, AccountPointTransactionTypes.Trade, "Test    ");
        // pointAdded.ShouldBe(100L);
    }

    [Fact]
    public async Task SalesAccountCreateAsync_Fake_Success()
    {
        // var party = await FakeParty(UserRoleTypes.Sales);
        // var code = Guid.NewGuid().ToString()[..8].ToUpper();
        // const string group = "Test Group";
        // party.ShouldNotBeNull();
        // party.Id.ShouldBeGreaterThan(0);
        // var item = await _svc.SalesAccountCreateAsync(party.Id, FundTypes.Wire, CurrencyTypes.Invalid, party.Name,
        //     code: code,
        //     group: group);
        // item.Role.ShouldBe((short)AccountRoleTypes.Sales);
        // item.Code.ShouldBe(code);
        // item.Group.ShouldBe(group);
    }

    [Fact]
    public async Task IbAccountCreateAsync_Fake_Success()
    {
        // var party = await FakeParty(UserRoleTypes.IB);
        // var item = await _svc.IbAccountCreateAsync(party.Id, FundTypes.Wire, CurrencyTypes.Invalid, party.Name
        //     , code: Guid.NewGuid().ToString()[..10], group: Guid.NewGuid().ToString()[..10]);
        // item.Role.ShouldBe((short)AccountRoleTypes.Agent);
        // await AccountQuery_OrderById_HasReturn();
    }
    //
    // [Fact]
    // public async Task AssignReferralCodeAsync_NewAgentAccount_Success()
    // {
    //     var referCode = Guid.NewGuid().ToString()[..8].ToUpper();
    //     var party = await FakeParty(UserRoleTypes.Sales);
    //     var referrer = Account.Build(party.Id, AccountRoleTypes.Agent);
    //     referrer.Uid = Utils.GenerateUniqueId();
    //     await tenantDbContext.Accounts.AddAsync(referrer);
    //     await tenantDbContext.SaveChangesAsync();
    //
    //     var agentRebateRule = new AgentRebateRule();
    //     agentRebateRule.AgentAccountId = referrer.Id;
    //
    //     await tenantDbContext.AgentRebateRules.AddAsync(agentRebateRule);
    //     await tenantDbContext.SaveChangesAsync();
    //
    //     var clientParty = await FakeParty(UserRoleTypes.Sales);
    //     var newClient = Account.Build(clientParty.Id, AccountRoleTypes.Client);
    //     newClient.Uid = Utils.GenerateUniqueId();
    //     await tenantDbContext.Accounts.AddAsync(newClient);
    //     await tenantDbContext.SaveChangesAsync();
    //
    //     var referralCode = new ReferralCode
    //     {
    //         Code = referCode,
    //         Name = "Name-" + referCode,
    //         PartyId = party.Id,
    //         AccountId = referrer.Id,
    //         ServiceType = (int)ReferralServiceTypes.Agent
    //     };
    //     await tenantDbContext.ReferralCodes.AddAsync(referralCode);
    //     await tenantDbContext.SaveChangesAsync();
    //
    //     const string data = """
    //                         {
    //                             "AccountRoleType": 300,
    //                             "AccountTypes": [1,2,3],
    //                             "BaseSchemas": { "Alpha":  [
    //                                 {
    //                                     "cid": 1,
    //                                     "r": 1,
    //                                     "c": 0,
    //                                     "p": 0.2
    //                                 }
    //                             ],
    //                         "Standard":  [
    //                             {
    //                                 "cid": 1,
    //                                 "r": 1,
    //                                 "c": 0,
    //                                 "p": 0.2
    //                             }
    //                         ],
    //                         "Advance":  [
    //                             {
    //                                 "cid": 1,
    //                                 "r": 1,
    //                                 "c": 0,
    //                                 "p": 0.2
    //                             }
    //                         ]
    //                             },
    //                             "AllocationSchemas": {
    //                                 "Alpha": [
    //                                     {
    //                                         "cid": 1,
    //                                         "r": 1.0,
    //                                         "cr": 0.2
    //                                     }
    //                                 ],
    //                                 "Standard": [
    //                                     {
    //                                         "cid": 1,
    //                                         "r": 1.0,
    //                                         "cr": 0.2
    //                                     }
    //                                 ],
    //                                 "Advance": [
    //                                     {
    //                                         "cid": 1,
    //                                         "r": 1.0,
    //                                         "cr": 0.2
    //                                     }
    //                                 ]
    //                             }
    //                         }
    //                         """;
    //     var supplement = Supplement.Build(SupplementTypes.ReferralCode, referrer.Id, data);
    //     await tenantDbContext.Supplements.AddAsync(supplement);
    //     await tenantDbContext.SaveChangesAsync();
    //
    //     var account = await tenantDbContext.Accounts
    //         .Where(x => x.PartyId > 0)
    //         .Where(x => x.PartyId != referralCode.PartyId)
    //         .Where(x => x.Id != referralCode.AccountId)
    //         .OrderByDescending(x => x.Id).FirstAsync();
    //
    //     account.ShouldNotBeNull();
    //     if (!string.IsNullOrEmpty(account.ReferCode))
    //     {
    //         account.ReferPath = string.Empty;
    //         account.ReferCode = string.Empty;
    //         account.ReferrerAccountId = null;
    //         tenantDbContext.Accounts.Update(account);
    //         await tenantDbContext.SaveChangesAsync();
    //     }
    //
    //     var assignedAccount = await _svc.AssignReferralCodeAsync(account.Id, referCode);
    //     assignedAccount.ShouldNotBeNull();
    //     assignedAccount.ReferCode.ShouldBe(referCode);
    //     assignedAccount.ReferPath.ShouldNotBeNullOrEmpty();
    //     assignedAccount.ReferrerAccountId.ShouldBe(referralCode.AccountId);
    //     // assignedAccount.AgentAccountId.ShouldBe(referralCode.AccountId);
    //
    //     var rebateRule = await tenantDbContext.ClientRebateRules
    //         .Where(x => x.ClientAccountId == newClient.Id)
    //         .FirstOrDefaultAsync();
    //     rebateRule.ShouldNotBeNull();
    //     rebateRule.GetBaseSchemas().ShouldNotBeNull();
    //     rebateRule.GetBaseSchemas().Any().ShouldBeTrue();
    //     rebateRule.DistributionType.ShouldBe((short)RebateDistributionTypes.Allocation);
    // }

    //[Fact]
    //public async Task GenerateRulesFromTradesTest()
    //{
    //    await CreateWithTradeAccountAsync_ForDemoAccount_Success();
    //    var accountIds = await tenantDbContext.Accounts
    //        .Select(x => x.Id).ToListAsync();
    //    var symbolsIds = await tenantDbContext.Symbols.Select(x => x.Id).ToListAsync();
    //    foreach (var accountId in accountIds)
    //    {
    //        var exists = await tenantDbContext.RebateRules
    //            .Where(x => x.AccountId == accountId)
    //            .AnyAsync();
    //        if (exists) continue;

    //        var rule = new RebateRule
    //        {
    //            AccountId = accountId,
    //            CreatedBy = 0,
    //            CreatedOn = DateTime.UtcNow,
    //            UpdatedBy = 0,
    //            UpdatedOn = DateTime.UtcNow,
    //            // Commission = getPercentage(),
    //            // Rate = getPercentage(),
    //            // Pipe = getPercentage(),
    //            BaseSchema = JsonConvert.SerializeObject(fakeRuleInfo(symbolsIds)),
    //            LevelSchema = JsonConvert.SerializeObject(fakeRuleInfo(symbolsIds)),
    //        };

    //        await tenantDbContext.RebateRules.AddAsync(rule);
    //        await tenantDbContext.SaveChangesAsync();
    //    }

    //    await GetRulesTest();
    //}

    // private async Task GetRulesTest()
    // {
    //     var rebateRule = await tenantDbContext.ClientRebateRules
    //         .Where(x => x.ClientAccountId > 0)
    //         .OrderByDescending(x => x.Id).FirstAsync();
    //
    //     rebateRule.ShouldNotBeNull();
    //     var rules = rebateRule.GetBaseSchemas();
    //     rules.Count.ShouldBeGreaterThan(0);
    //     var record = Faker.PickRandom(rules);
    //     var match = rebateRule.GetBaseRuleForSymbolCategory(record.SymbolCategoryId);
    //     record.ShouldNotBeNull();
    //     match.ShouldNotBeNull();
    //     match.SymbolCategoryId.ShouldBe(record.SymbolCategoryId);
    //     match.Pip.ShouldBe(record.Pip);
    //     match.Rate.ShouldBe(record.Rate);
    //     match.Commission.ShouldBe(record.Commission);
    // }
    //
    // [Fact]
    // public async Task GetPaymentServiceTest()
    // {
    //     var query = from a in tenantDbContext.Accounts
    //             join s in tenantDbContext.Supplements on new
    //                     { a.Id, Type = (int)SupplementTypes.AccountPaymentAccess } equals
    //                 new { Id = s.RowId, s.Type } into g
    //             from accesses in g.DefaultIfEmpty()
    //             where accesses == null
    //             select a
    //         ;
    //     var account = await query.FirstAsync();
    //     account.ShouldNotBeNull();
    //
    //     // // Get payment methods for account
    //     // var access = await _svc.GetAccountPaymentServiceAsync(account.Id);
    //     // access.Withdrawal.Count.ShouldBe(0);
    //     // access.Deposit.Count.ShouldBe(0);
    //
    //     // Set payment methods activated
    //     var services = await tenantDbContext.PaymentServices
    //         .Where(x => x.IsActivated == 1)
    //         .ToListAsync();
    //
    //     var deposit = Faker.PickRandom(services.Where(x => x.CanDeposit == 1), 2).ToList();
    //     var withdrawal = Faker.PickRandom(services.Where(x => x.CanWithdraw == 1), 2).ToList();
    //     deposit.Count.ShouldBeGreaterThan(0);
    //     withdrawal.Count.ShouldBeGreaterThan(0);
    //
    //     // // set payment methods
    //     // var result = await _svc.SetAccountPaymentServiceAsync(account.Id, new PaymentService.Accesses
    //     // {
    //     //     Deposit = deposit.Select(x => x.Id).ToList(),
    //     //     Withdrawal = withdrawal.Select(x => x.Id).ToList(),
    //     // });
    //     // result.ShouldNotBeNull();
    //     // result.Deposit.Any().ShouldBeTrue();
    //     // result.Withdrawal.Any().ShouldBeTrue();
    //     //
    //     // // get payment methods
    //     // access = await _svc.GetAccountPaymentServiceAsync(account.Id);
    //     // access.Deposit.Count.ShouldBe(result.Deposit.Count);
    //     // access.Withdrawal.Count.ShouldBe(result.Withdrawal.Count);
    //
    //     // // set payment methods to empty
    //     // var accessIds = await _svc.SetAccountPaymentServiceAsync(account.Id, new PaymentService.Accesses());
    //     // accessIds.Withdrawal.Count.ShouldBe(0);
    //     // accessIds.Deposit.Count.ShouldBe(0);
    // }

    private decimal GetPercentage()
        => decimal.Round(Faker.Finance.Amount(min: 0.01m, max: 0.23m, decimals: 2), 2, MidpointRounding.AwayFromZero);

    private IList<RebateLevelSchemaItem> FakeRuleInfo(IEnumerable<int> symbolIds)
        => symbolIds.Select(x => new RebateLevelSchemaItem(x, GetPercentage())).ToList();

    private async Task AccountQuery_OrderById_HasReturn()
    {
        var criteria = new Account.Criteria { SortField = "Id" };
        var result = await _svc.AccountQueryAsync(criteria);
        result.ShouldNotBeNull();
        if (result.Criteria.Total > 1)
        {
            result.Data.First().Id.ShouldBeGreaterThan(result.Data.Skip(1).First().Id);
        }

        result.Criteria.Total.ShouldBeGreaterThan(0);
        var id = result.Data.First().Id;
        var item = await _svc.AccountGetAsync(id);
        item.ShouldNotBeNull();
        item.Id.ShouldBe(id);
    }

    private async Task TradeAccountQueryAsync_OrderById_HasReturn()
    {
        var criteria = new TradeAccount.Criteria { SortField = "Id" };
        var result = await _svc.TradeAccountQueryAsync(criteria);
        result.ShouldNotBeNull();
        result.Criteria.Total.ShouldBeGreaterThan(0);

        var id = result.Data.First().Id;
        var item = await _svc.AccountGetAsync(id);
        item.ShouldNotBeNull();
        item.Id.ShouldBe(id);
    }
}