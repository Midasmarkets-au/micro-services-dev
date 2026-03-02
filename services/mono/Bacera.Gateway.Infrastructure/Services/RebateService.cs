using Bacera.Gateway.Context;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.ViewModels.Tenant;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bacera.Gateway.Services;

public class RebateService(
    TenantDbContext ctx,
    IMyCache myCache,
    ILogger<RebateService> logger,
    MyDbContextPool myDbContextPool)
{
    public async Task<List<RebateBasicViewModel>> TradeRebateCheckForTenantAsync(long id)
    {
        var rebates = await GenerateRebatesByTradeRebateId(id);
        var accountIds = rebates.Select(x => x.AccountId).ToList();
        var accounts = await ctx.Accounts.AsNoTracking()
            .Where(x => accountIds.Contains(x.Id))
            .ToListAsync();
        rebates.ForEach(x => x.Account = accounts.Single(a => a.Id == x.AccountId));

        var parties = await ctx.Parties.AsNoTracking()
            .Where(x => rebates.Select(r => r.PartyId).Distinct().Contains(x.Id))
            .ToTenantBasicViewModel()
            .ToListAsync();

        var models = rebates.ToRebateBasicViewModel().ToList();
        models.ForEach(x => x.User = parties.Single(p => p.PartyId == x.PartyId));

        var partyIds = models.Select(x => x.PartyId).ToList();
        partyIds.AddRange(models.Select(x => x.TargetAccount.PartyId));
        partyIds = partyIds.Distinct().ToList();

        var users = await ctx.Parties
            .Where(x => partyIds.Contains(x.Id))
            .ToTenantBasicViewModel()
            .ToListAsync();

        foreach (var rebate in models)
        {
            var user = users.FirstOrDefault(x => x.PartyId == rebate.PartyId);
            if (user == null) continue;
            rebate.User = user;
            var targetUser = users.FirstOrDefault(x => x.PartyId == rebate.TargetAccount.PartyId);
            if (targetUser == null) continue;
            rebate.TargetAccount.User = targetUser;
        }

        return models;
    }

    public async Task<List<Rebate>> GenerateRebatesByTradeRebateId(long id, bool shouldSend = false)
    {
        var ruleType = await ctx.TradeRebates
            .Where(x => x.Id == id)
            .Where(x => x.Account != null)
            .Where(x => x.Account!.RebateClientRule != null)
            .Select(x => (RebateDistributionTypes)x.Account!.RebateClientRule!.DistributionType)
            .SingleOrDefaultAsync();

        var rebates = ruleType switch
        {
            RebateDistributionTypes.LevelPercentage => await GenerateLevelPercentageRebatesByTradeRebateId(id),
            RebateDistributionTypes.Allocation => await GenerateAllocationRebatesByTradeRebateId(id),
            RebateDistributionTypes.Direct => await GenerateDirectRebatesByTradeRebateId(id),
            _ => []
        };

        if (shouldSend)
        {
            await SendRebates(id, rebates);
        }

        return rebates;
    }

    private async Task SendRebates(long tradeRebateId, List<Rebate> rebates)
    {
        var tradeRebate = await ctx.TradeRebates
            .Where(x => x.Id == tradeRebateId)
            .Where(x => x.Status == (short)TradeRebateStatusTypes.PendingResend || x.Status == (short)TradeRebateStatusTypes.Created)
            .SingleOrDefaultAsync();

        if (tradeRebate?.AccountId == null) return;

        var isActive = await ctx.Accounts
            .Where(x => x.Id == tradeRebate.AccountId)
            .AnyAsync(x => x.Status == (short)AccountStatusTypes.Activate);
        if (!isActive) return;

        if (rebates.Count == 0 && await ctx.Rebates.AllAsync(x => x.TradeRebateId != tradeRebate.Id))
        {
            tradeRebate.Status = (int)TradeRebateStatusTypes.HasNoRebate;
            await ctx.SaveChangesAsync();
            return;
        }

        tradeRebate.Status = rebates.All(x => x.IdNavigation.StateId == (int)StateTypes.RebateTradeClosedLessThanOneMinute)
            ? (int)TradeRebateStatusTypes.SkippedWithOpenCloseTimeLessThanOneMinute
            : (int)TradeRebateStatusTypes.Completed;

        if (rebates.Count > 0 && await ctx.Rebates.AllAsync(x => x.TradeRebateId != tradeRebate.Id))
        {
            ctx.Rebates.AddRange(rebates);
            await ctx.SaveChangesAsync();
            return;
        }

        var existingRebates = await ctx.Rebates.Where(x => x.TradeRebateId == tradeRebate.Id).ToListAsync();
        // var deductingRebates = new List<Rebate>();
        foreach (var existing in existingRebates)
        {
            var current = rebates.SingleOrDefault(x => x.AccountId == existing.AccountId);
            if (current == null)
            {
                existing.ResetMatterId().SetAmount(-existing.Amount);
                ctx.Rebates.Add(existing);
            }
            else if (current.Amount == existing.Amount)
            {
                rebates.Remove(current);
            }
            else if (current.Amount != existing.Amount)
            {
                current.SetAmount(current.Amount - existing.Amount);
            }
        }

        ctx.Rebates.AddRange(rebates);
        await ctx.SaveChangesAsync();
    }

    private async Task<List<Rebate>> GenerateLevelPercentageRebatesByTradeRebateId(long tradeRebateId)
    {
        var tradeRebate = await ctx.TradeRebates
            .Include(x => x.Account)
            .SingleOrDefaultAsync(x => x.Id == tradeRebateId);

        if (tradeRebate?.AccountId == null || tradeRebate.Account is not { Status: (short)AccountStatusTypes.Activate }) return [];

        var trimmedSymbol = GetTrimmedSymbol(tradeRebate.Symbol);
        var symbolCategory = await ctx.Symbols
            .Where(x => x.Code == trimmedSymbol)
            .Select(x => x.Category)
            .FirstOrDefaultAsync() ?? "OTHER";

        symbolCategory = symbolCategory.ToUpper();
        if (symbolCategory.Contains("FOREX"))
        {
            symbolCategory = "FOREX";
        }
        else if (symbolCategory.Contains("GOLD"))
        {
            symbolCategory = "GOLD";
        }
        else
        {
            symbolCategory = "OTHER";
        }
        
        var rebateDirectSchemaItem = await ctx.RebateClientRules
            .Where(x => x.DistributionType == (short)RebateRuleTypes.LevelPercentage)
            .Where(x => x.ClientAccountId == tradeRebate.AccountId)
            .Where(x => x.RebateDirectSchema != null)
            .SelectMany(x => x.RebateDirectSchema!.RebateDirectSchemaItems)
            .Where(x => x.SymbolCode == trimmedSymbol)
            .FirstOrDefaultAsync();

        if (rebateDirectSchemaItem == null) return [];

        var agentAccounts = await GetSortedAgentAccountsForAllocation(tradeRebate.AccountId.Value);
        if (agentAccounts.Count == 0) return [];

        var topAgent = agentAccounts.FirstOrDefault();
        if (topAgent == null) return [];

        var baseRebate = await CalculateRatePipCommissionByDirectSchema(tradeRebate.TradeServiceId, rebateDirectSchemaItem, tradeRebate.Volume,
            (CurrencyTypes)tradeRebate.CurrencyId);

        var totalRebateAmount = baseRebate.GetTotal();
        if (totalRebateAmount == 0) return [];

        var remainRebateAmount = (decimal)totalRebateAmount;
        var rebates = new List<Rebate>();

        var volume = tradeRebate.Volume / 100m;
        var sourceExchangeRate = await GetMtExchangeRate(tradeRebate.TradeServiceId, (CurrencyTypes)tradeRebate.CurrencyId, CurrencyTypes.USD);
        
        var level2Agent = agentAccounts.Skip(1).FirstOrDefault();
        if (level2Agent != null)
        {
            var levelSetting = level2Agent.RebateAgentRule?.GetLevelSetting();
            if (levelSetting == null) return [];

            // var category = RebateAgentRule.GetLevelPercentageSymbolCategory(trimmedSymbol);
            var settings = level2Agent.RebateAgentRule?.GetLevelSetting().PercentageSetting;
            if (settings == null || !settings.TryGetValue(symbolCategory, out var setting)) return [];
            if (setting.Count == 0) return [];

            setting.Reverse();
            var queue = new Queue<decimal>(setting);
            for (var i = agentAccounts.Count - 1; remainRebateAmount > 0 && i > 0 && queue.Count > 1; i--)
            {
                var account = agentAccounts[i];
                var percentage = queue.Dequeue() * 100;

                var targetExchangeRate = await GetMtExchangeRate(tradeRebate.TradeServiceId, CurrencyTypes.USD, (CurrencyTypes)account.CurrencyId);
                // var amount = Math.Ceiling(totalRebateAmount * percentage * (decimal)targetExchangeRate);
                var amount = Math.Round(volume * percentage * (decimal)sourceExchangeRate * (decimal)targetExchangeRate, MidpointRounding.ToZero);
                if (amount == 0) break;

                amount = Math.Min(amount, remainRebateAmount);
                remainRebateAmount -= amount;

                var rebate = new Rebate
                {
                    PartyId = account.PartyId,
                    AccountId = account.Id,
                    Amount = (long)amount * 100,
                    TradeRebateId = tradeRebate.Id,
                    CurrencyId = account.CurrencyId,
                    FundType = tradeRebate.Account.FundType,
                    HoldUntilOn = DateTime.UtcNow.AddHours(0),
                    IdNavigation = Matter.Build().Rebate().SetState(StateTypes.RebateOnHold),
                    Information = Utils.JsonSerializeObject(new
                    {
                        Depth = i + 1,
                        BaseRebate = baseRebate,
                        ExchangeRate = targetExchangeRate,
                        RemainRebate = remainRebateAmount,
                        Version = "v3_level_percentage"
                    })
                };

                rebates.Add(rebate);
            }
        }

        // ReSharper disable once InvertIf
        if (remainRebateAmount > 0)
        {
            var targetExchangeRate = await GetMtExchangeRate(tradeRebate.TradeServiceId, CurrencyTypes.USD, (CurrencyTypes)topAgent.CurrencyId);
            var amount = (long)Math.Round(remainRebateAmount * (decimal)targetExchangeRate, MidpointRounding.ToZero);
            var rebate = new Rebate
            {
                PartyId = topAgent.PartyId,
                AccountId = topAgent.Id,
                Amount = amount * 100,
                TradeRebateId = tradeRebate.Id,
                CurrencyId = topAgent.CurrencyId,
                FundType = tradeRebate.Account.FundType,
                HoldUntilOn = DateTime.UtcNow.AddHours(0),
                IdNavigation = Matter.Build().Rebate().SetState(StateTypes.RebateOnHold),
                Information = Utils.JsonSerializeObject(new
                {
                    Depth = 1,
                    BaseRebate = baseRebate,
                    ExchangeRate = targetExchangeRate,
                    RemainRebate = remainRebateAmount,
                    Version = "v3_level_percentage_mib"
                })
            };

            rebates.Add(rebate);
        }

        return rebates;
    }

    private async Task<List<Rebate>> GenerateAllocationRebatesByTradeRebateId(long tradeRebateId)
    {
        var tradeRebate = await ctx.TradeRebates
            .Include(x => x.Account)
            .SingleOrDefaultAsync(x => x.Id == tradeRebateId);

        if (tradeRebate?.AccountId == null || tradeRebate.Account is not { Status: (short)AccountStatusTypes.Activate }) return [];

        var trimmedSymbol = GetTrimmedSymbol(tradeRebate.Symbol);
        var symbolCategoryId = await ctx.Symbols
            .Where(x => x.Code == trimmedSymbol)
            .Select(x => x.CategoryId)
            .FirstOrDefaultAsync();

        var rebateDirectSchemaItem = await ctx.RebateClientRules
            .Where(x => x.DistributionType == (short)RebateRuleTypes.Allocation)
            .Where(x => x.ClientAccountId == tradeRebate.AccountId)
            .Where(x => x.RebateDirectSchema != null)
            .SelectMany(x => x.RebateDirectSchema!.RebateDirectSchemaItems)
            .Where(x => x.SymbolCode == trimmedSymbol)
            .FirstOrDefaultAsync();
        if (rebateDirectSchemaItem == null) return [];

        var agentAccounts = await GetSortedAgentAccountsForAllocation(tradeRebate.AccountId.Value);
        if (agentAccounts.Count == 0) return [];

        var sourceExchangeRate = await GetMtExchangeRate(tradeRebate.TradeServiceId, (CurrencyTypes)tradeRebate.CurrencyId, CurrencyTypes.USD);

        var baseRebate = await CalculateRatePipCommissionByDirectSchema(
            tradeRebate.TradeServiceId,
            rebateDirectSchemaItem, tradeRebate.Volume,
            (CurrencyTypes)tradeRebate.CurrencyId);

        var remainRebate = BaseRebate.Build(baseRebate.Rate, baseRebate.Commission, baseRebate.Pip, baseRebate.Price);
        var depth = 1;
        var results = new List<Rebate>();
        for (var i = 0; i < agentAccounts.Count; i++)
        {
            var currentAccountForAllocation = agentAccounts[i];
            var targetCurrencyId = (CurrencyTypes)currentAccountForAllocation.CurrencyId;
            var targetExchangeRate = await GetMtExchangeRate(tradeRebate.TradeServiceId, CurrencyTypes.USD, targetCurrencyId);

            // =======
            // 一分钟加点加佣 hold
            // var hasPipsAndCommission = await ctx.Accounts
            //     .Where(x => x.Id == tradeRebate.AccountId)
            //     .Where(x => x.AccountTags.Any(y => y.Name == AccountTagTypes.AddPips) ||
            //                 x.AccountTags.Any(y => y.Name == AccountTagTypes.AddCommission))
            //     .AnyAsync();
            //
            // var matterStateId = tenancy.GetTenantId() != 10004 && tradeRebate.ClosedLessThanOneMinute() && hasPipsAndCommission
            //     ? StateTypes.RebateTradeClosedLessThanOneMinute
            //     : StateTypes.RebateOnHold;
            // =======

            // =======
            // 一分钟 hold
            // var matterStateId = tenancy.GetTenantId() != 10004 && tradeRebate.ClosedLessThanOneMinute()
            //     ? StateTypes.RebateTradeClosedLessThanOneMinute
            //     : StateTypes.RebateOnHold;
            // =======

            // 所有不hold
            var matterStateId = StateTypes.RebateOnHold;

            AllocationRebate allocationRebate;
            var rebate = new Rebate
            {
                PartyId = currentAccountForAllocation.PartyId,
                AccountId = currentAccountForAllocation.Id,
                TradeRebateId = tradeRebate.Id,
                CurrencyId = (int)targetCurrencyId,
                FundType = tradeRebate.Account.FundType,
                HoldUntilOn = DateTime.UtcNow.AddHours(0),
                IdNavigation = Matter.Build().Rebate().SetState(matterStateId),
            };

            var account = i == agentAccounts.Count - 1 ? null : agentAccounts[i + 1];

            if (account == null)
            {
                allocationRebate = CalculateLastAllocationRebate(ref remainRebate, sourceExchangeRate);
                rebate.Amount = (long)Math.Round(allocationRebate.Total * targetExchangeRate, MidpointRounding.ToZero) * 100;
                if (rebate.Amount == 0) break;
                rebate.Information = Utils.JsonSerializeObject(new
                {
                    Depth = depth,
                    BaseRebate = baseRebate,
                    ExchangeRate = targetExchangeRate,
                    RemainRebate = remainRebate,
                    AllocationSchemaItem = new RebateLevelSchemaItem(0, 0),
                    AllocationRebate = allocationRebate,
                    Note = "remaining all rebate",
                    Version = "v2"
                });
                results.Add(rebate);
                break;
            }

            var (schema, percentage) = account.RebateAgentRule!
                .GetSchemaItemAndPercentage((AccountTypes)tradeRebate.Account.Type, symbolCategoryId);
            
            if (schema == null) continue;

            allocationRebate = CalculateAllocationRebateBySchema(ref remainRebate, tradeRebate.Volume, schema, percentage, sourceExchangeRate);
            if (allocationRebate.Total == 0) continue;

            rebate.Information = Utils.JsonSerializeObject(new
            {
                Depth = depth++,
                BaseRebate = baseRebate,
                ExchangeRate = targetExchangeRate,
                RemainRebate = remainRebate,
                AllocationSchemaItem = schema,
                AllocationRebate = allocationRebate,
                Version = "v2"
            });

            var total = (long)Math.Round(allocationRebate.Total * targetExchangeRate, MidpointRounding.ToZero);
            rebate.Amount = total * 100;
            results.Add(rebate);
        }

        return results;
    }

    private async Task<List<Rebate>> GenerateDirectRebatesByTradeRebateId(long tradeRebateId)
    {
        var tradeRebate = await ctx.TradeRebates
            .Include(x => x.Account)
            .SingleOrDefaultAsync(x => x.Id == tradeRebateId);

        if (tradeRebate?.AccountId == null || tradeRebate.Account is not { Status: (short)AccountStatusTypes.Activate }) return [];

        var directRules = await ctx.RebateDirectRules
            .Where(x => x.SourceTradeAccount.AccountNumber != 59999999)
            .Where(x => x.SourceTradeAccountId == tradeRebate.AccountId)
            .Select(x => new
            {
                x.RebateDirectSchemaId,
                x.TargetAccountId
            })
            .ToListAsync();

        var targetAccounts = await ctx.Accounts
            .Where(x => directRules.Select(y => y.TargetAccountId).Contains(x.Id))
            .Select(x => new { x.Id, x.PartyId, x.CurrencyId, x.FundType, x.ServiceId })
            .ToListAsync();

        var results = new List<Rebate>();
        foreach (var rule in directRules)
        {
            var trimmedSymbol = GetTrimmedSymbol(tradeRebate.Symbol);

            var rebateDirectSchemaItem = await ctx.RebateDirectSchemaItems
                .Where(x => x.RebateDirectSchemaId == rule.RebateDirectSchemaId)
                .Where(x => x.SymbolCode == trimmedSymbol)
                .Where(x => x.Rate > 0 || x.Pips > 0 || x.Commission > 0)
                .FirstOrDefaultAsync();

            if (rebateDirectSchemaItem == null)
                continue;

            var itemData = await CalculateRatePipCommissionByDirectSchema(tradeRebate.Account.ServiceId, rebateDirectSchemaItem, tradeRebate.Volume,
                (CurrencyTypes)tradeRebate.CurrencyId);

            var targetAccount = targetAccounts.FirstOrDefault(x => x.Id == rule.TargetAccountId);
            if (targetAccount == null) continue;

            var targetExchangeRate = await GetMtExchangeRate(targetAccount.ServiceId, CurrencyTypes.USD, (CurrencyTypes)targetAccount.CurrencyId);
            var total = (long)Math.Round(itemData.GetTotal() * (decimal)targetExchangeRate, MidpointRounding.ToZero);

            // =======
            // 一分钟加点加佣 hold
            // var hasPipsAndCommission = await ctx.Accounts
            //     .Where(x => x.Id == tradeRebate.AccountId)
            //     .Where(x => x.AccountTags.Any(y => y.Name == AccountTagTypes.AddPips) ||
            //                 x.AccountTags.Any(y => y.Name == AccountTagTypes.AddCommission))
            //     .AnyAsync();
            //
            // var matterStateId = tenancy.GetTenantId() != 10004 && tradeRebate.ClosedLessThanOneMinute() && hasPipsAndCommission
            //     ? StateTypes.RebateTradeClosedLessThanOneMinute
            //     : StateTypes.RebateOnHold;
            // =======

            // =======
            // 一分钟 hold
            // var matterStateId = tenancy.GetTenantId() != 10004 && tradeRebate.ClosedLessThanOneMinute()
            //     ? StateTypes.RebateTradeClosedLessThanOneMinute
            //     : StateTypes.RebateOnHold;
            // =======

            // 所有不hold
            var matterStateId = StateTypes.RebateOnHold;

            var rebate = new Rebate
            {
                PartyId = targetAccount.PartyId,
                AccountId = targetAccount.Id,
                TradeRebateId = tradeRebate.Id,
                Amount = total * 100,
                CurrencyId = targetAccount.CurrencyId,
                HoldUntilOn = DateTime.UtcNow,
                FundType = targetAccount.FundType,
                IdNavigation = Matter.Build().Rebate().SetState(matterStateId),
                Information = Utils.JsonSerializeObject(new
                {
                    BaseRebate = itemData,
                    rebateDirectSchemaItem,
                    ExchangeRate = targetExchangeRate,
                    Version = "v2"
                })
            };
            results.Add(rebate);
        }

        return results;
    }

    private static string GetTrimmedSymbol(string symbol)
    {
        var baseSymbol = symbol;
        if (baseSymbol.Contains('.'))
        {
            // keep the symbol before '"'.s'"'
            baseSymbol = baseSymbol[..baseSymbol.IndexOf('.', StringComparison.Ordinal)];
        }

        if (baseSymbol.StartsWith("#CL"))
        {
            return "#CL";
        }

        if (baseSymbol.StartsWith("#BRN"))
        {
            return "#BRN";
        }

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (baseSymbol == "XAGUSDmin")
        {
            return baseSymbol.ToUpper();
        }

        return baseSymbol;
    }

    /// <returns>Rate Pip Commission</returns>
    private async Task<BaseRebate> CalculateRatePipCommissionByDirectSchema(int tradeServiceId, RebateDirectSchemaItem directSchemaItem, int volume,
        CurrencyTypes sourceCurrencyId = CurrencyTypes.USD)
    {
        var dividedVolume = volume / 100m;
        var sourceExchangeRate = await GetMtExchangeRate(tradeServiceId, sourceCurrencyId, CurrencyTypes.USD);

        var rate = (long)Math.Round(directSchemaItem.Rate * (decimal)sourceExchangeRate * 100 * dividedVolume * 100, MidpointRounding.ToZero);

        if (directSchemaItem is { Pips: 0, Commission: 0 })
            return BaseRebate.Build(rate, 0, 0);
        
        var commission = (long)Math.Round(
            directSchemaItem.Commission * (decimal)sourceExchangeRate * 100 * dividedVolume * 100,
            MidpointRounding.ToZero);

        var (pipFormula, exchangeRate) =
            await GetMtPipValueBySymbolCode(tradeServiceId, directSchemaItem.SymbolCode);

        var pip = (long)Math.Round(directSchemaItem.Pips * (decimal)sourceExchangeRate * 100 * dividedVolume * (decimal)pipFormula * 100,
            MidpointRounding.ToZero);

        return BaseRebate.Build(rate, commission, pip, exchangeRate);
    }

    private static AllocationRebate CalculateAllocationRebateBySchema(ref BaseRebate remainBaseRebate
        , int volume
        , RebateLevelSchemaItem schemaItem
        , decimal takePercentage
        , double sourceExchangeRate = 1.0d)
    {
        // Fix: 与基础返佣计算保持一致，使用 volume/100 而不是 volume
        // 修复前: volume * 100 * 100 导致r=1.0计算出100美元
        // 修复后: volume * 100 使r=1.0计算出1美元，符合"从21块里拿1块"的语义
        var rate = (long)Math.Round(schemaItem.Rate * (decimal)sourceExchangeRate * volume * 100, MidpointRounding.ToZero);

        var schemaRateValue = Math.Min(remainBaseRebate.Rate, rate);

        remainBaseRebate.Rate -= schemaRateValue;
        var schemaCombineRateValue = 0L;
        if (remainBaseRebate.Pip > 0)
        {
            var pipValue = (long)Math.Round((decimal)remainBaseRebate.Pip * (decimal)sourceExchangeRate * Math.Abs(takePercentage) / 100m * 100m
                , MidpointRounding.ToZero); // CombinedRate is Percentage
            var pipTempValue = Math.Min(remainBaseRebate.Pip, pipValue);
            schemaCombineRateValue += pipTempValue;
            remainBaseRebate.Pip -= pipTempValue;
        }

        if (remainBaseRebate.Commission > 0)
        {
            var commissionValue = (long)Math.Round((decimal)remainBaseRebate.Commission
                * (decimal)sourceExchangeRate
                * Math.Abs(takePercentage) / 100m * 100m
                , MidpointRounding.ToZero);
            var commissionTempValue = Math.Min(remainBaseRebate.Commission, commissionValue);
            schemaCombineRateValue += commissionTempValue;
            remainBaseRebate.Commission -= commissionTempValue;
        }

        var item = AllocationRebate.BuildSchema(schemaRateValue, schemaCombineRateValue);
        return item;
    }

    private static AllocationRebate CalculateLastAllocationRebate(ref BaseRebate remainBaseRebate,
        double sourceExchangeRate = 1.0d)
    {
        var schemaRateValue = remainBaseRebate.Rate;
        remainBaseRebate.Rate = 0;
        var schemaCombineRateValue = 0L;
        if (remainBaseRebate.Pip > 0)
        {
            schemaCombineRateValue += remainBaseRebate.Pip;
            remainBaseRebate.Pip = 0;
        }

        if (remainBaseRebate.Commission > 0)
        {
            schemaCombineRateValue += remainBaseRebate.Commission;
            remainBaseRebate.Commission -= remainBaseRebate.Commission;
        }

        var item = AllocationRebate.BuildSchema(schemaRateValue, schemaCombineRateValue);
        return item;
    }

    private async Task<List<Account>> GetSortedAgentAccountsForAllocation(long accountId)
    {
        var account = await ctx.Accounts.FirstOrDefaultAsync(x => x.Id == accountId);
        if (account == null)
            return [];

        var parentUids = account.ReferPathUids;
        var parentAccounts = await ctx.Accounts
            .Where(x => parentUids.Contains(x.Uid))
            .Where(x => x.RebateAgentRule != null)
            .Include(x => x.RebateAgentRule)
            .OrderBy(x => x.Level)
            .ToListAsync();

        var count = parentAccounts.TakeWhile(item => !item.IsTopLevelAgent()).Count();
        return parentAccounts.Skip(count).ToList();
    }

    private async Task<(double, double)> GetMtPipValueBySymbolCode(int serviceId, string symbolCode)
    {
        if (symbolCode.EndsWith("USD"))
        {
            var price = await GetMtPrice(serviceId, symbolCode);
            var value = 1 / Math.Pow(10, price.Digit) / price.Bid * GetContractSize(symbolCode) * price.Bid;
            return (value, price.Bid);
        }

        if (symbolCode.StartsWith("#"))
        {
            if (new List<string>
                {
                    "#AUS200", "#GER30", "#GER40", "#EUSTX50", "#FRA40", "#ESP35", "#UK100"
                }.Contains(symbolCode))
            {
                MtPrice price;
                if (symbolCode == "#AUS200")
                {
                    price = await GetMtPrice(serviceId, "AUDUSD");
                }
                else if (symbolCode == "#UK100")
                {
                    price = await GetMtPrice(serviceId, "GBPUSD");
                }
                else
                {
                    price = await GetMtPrice(serviceId, "EURUSD");
                }

                return (price.Bid * GetPrefixPipValue(symbolCode), price.Bid);
            }

            if (symbolCode == "#HKG50")
            {
                var price = await GetMtPrice(serviceId, "USDHKD");
                return (Math.Round(1 / price.Bid, 5) * GetPrefixPipValue(symbolCode), price.Bid);
            }

            return (GetPrefixPipValue(symbolCode), 0d);
        }

        if (symbolCode.StartsWith("USD"))
        {
            var price = await GetMtPrice(serviceId, symbolCode);
            return ((1 / Math.Pow(10, price.Digit) / price.Bid * GetContractSize(symbolCode)),
                price.Bid);
        }

        if (symbolCode.Length != 6)
            return (0d, 0d);

        var lastThree = symbolCode[3..];
        if (ContractSize.ContainsKey(lastThree + "USD"))
        {
            var price = await GetMtPrice(serviceId, lastThree + "USD");
            return (price.Bid, price.Bid);
        }

        if (ContractSize.ContainsKey("USD" + lastThree))
        {
            var price = await GetMtPrice(serviceId, "USD" + lastThree);
            return (1 / Math.Pow(10, price.Digit) / price.Bid * GetContractSize(symbolCode),
                price.Bid);
        }

        if (PrefixPipValues.ContainsKey(symbolCode))
        {
            return (GetPrefixPipValue(symbolCode), 0d);
        }

        return (0d, 0d);
    }

    public async Task<double> GetMtExchangeRate(int serviceId, CurrencyTypes from, CurrencyTypes to)
    {
        if (from == to) return 1;
        
        // Handle USC conversions using the common helper
        var (isUscConversion, uscRate) = CurrencyConversionHelper.GetUscConversionRateDouble(from, to);
        if (isUscConversion)
        {
            if (uscRate == -1.0)
            {
                // Multi-step USC conversion needed
                return await CurrencyConversionHelper.CalculateUscConversionRateDoubleAsync(from, to, 
                    (fromCurrency, toCurrency) => GetMtExchangeRate(serviceId, fromCurrency, toCurrency));
            }
            return uscRate;
        }
        
        var fromCurrency = Enum.GetName(typeof(CurrencyTypes), from)!;
        var toCurrency = Enum.GetName(typeof(CurrencyTypes), to)!;

        var price = await GetMtPrice(serviceId, fromCurrency + toCurrency);
        if (price.Bid != 0) return price.Bid;

        price = await GetMtPrice(serviceId, toCurrency + fromCurrency);
        return price.Bid == 0 ? 1 : 1 / price.Bid;
    }

    public async Task<bool> EnableReleaseRebateAsync(long timeInMinutes = 60)
    {
        try
        {
            var key = CacheKeys.GetReleaseDisabledKey();
            await myCache.SetStringAsync(key, "1", TimeSpan.FromMinutes(timeInMinutes));
            return true;
        }
        catch (Exception e)
        {
            BcrLog.Slack($"EnableReleaseRebateAsync_Error_{e.Message}");
            return false;
        }
    }

    public async Task<bool> DisableReleaseRebateAsync()
    {
        try
        {
            var key = CacheKeys.GetReleaseDisabledKey();
            await myCache.KeyDeleteAsync(key);
            return true;
        }
        catch (Exception e)
        {
            BcrLog.Slack($"DisableReleaseRebateAsync_Error_{e.Message}");
            return false;
        }
    }


    private async Task<MtPrice> GetMtPrice(int serviceId, string symbolCode)
    {
        var platform = myDbContextPool.GetPlatformByServiceId(serviceId);
        var price = new MtPrice();
        switch (platform)
        {
            case PlatformTypes.MetaTrader4:
            {
                await using var mt4Ctx = myDbContextPool.CreateCentralMT4DbContextAsync(serviceId);
                price = await mt4Ctx.Mt4Prices
                    .AsNoTracking()
                    .Where(x => x.Symbol == symbolCode)
                    .Select(x => new MtPrice
                    {
                        Bid = x.Bid,
                        Digit = x.Digits,
                    })
                    .FirstOrDefaultAsync();
                break;
            }
            case PlatformTypes.MetaTrader5:
            {
                await using var mt5Ctx = myDbContextPool.CreateCentralMT5DbContextAsync(serviceId);
                price = await mt5Ctx.Mt5Prices
                    .AsNoTracking()
                    .Where(x => x.Symbol == symbolCode)
                    .Select(x => new MtPrice
                    {
                        Bid = x.BidLast,
                        Digit = (int)x.Digits,
                    })
                    .FirstOrDefaultAsync();
                break;
            }
        }

        return price ?? new MtPrice();
    }

    private static string NormalizeSymbolCode(string code)
        => PrefixSymbols.FirstOrDefault(code.Contains) ?? code;

    private static double GetPrefixPipValue(string symbolCode)
    {
        var normalizeSymbol = NormalizeSymbolCode(symbolCode);
        if (!PrefixPipValues.ContainsKey(normalizeSymbol)) return 0;
        var pipValue = PrefixPipValues.First(x => x.Key == normalizeSymbol);
        return pipValue.Value;
    }

    private static int GetContractSize(string symbolCode)
    {
        var normalizeSymbol = NormalizeSymbolCode(symbolCode);
        if (!ContractSize.ContainsKey(normalizeSymbol)) return 1;
        var contractSize = ContractSize.First(x => x.Key == normalizeSymbol);
        return contractSize.Value;
    }

    private static readonly List<string> PrefixSymbols = new()
    {
        "#CL",
        "#NKD",
        "#Copper",
        "#Corn",
        "#Wheat",
        "#Soybean",
        "#CN300"
        // "#ES",
        // "#YM",
        // "#NQ",
        // "#NG",
        // "#BRN",
    };

    private static readonly Dictionary<string, int> ContractSize
        = new()
        {
            { "#AAPL", 100 },
            { "#CL", 1000 },
            { "#ES", 50 },
            { "#NKD", 5 },
            { "#NQ", 20 },
            { "#YM", 5 },
            { "#NG", 10000 },
            { "#Copper", 25000 },
            { "#Soybean", 5000 },
            { "#Wheat", 5000 },
            { "#Corn", 5000 },
            { "#MSFT", 100 },
            { "#AXP", 100 },
            { "#MCD", 100 },
            { "#INTC", 100 },
            { "#IBM", 100 },
            { "#KO", 100 },
            { "#C", 100 },
            { "#BAC", 100 },
            { "#DIS", 100 },
            { "#BA", 100 },
            { "#QAN.AX", 100 },
            { "#APT.AX", 100 },
            { "#CSL.AX", 100 },
            { "#BHP.AX", 100 },
            { "#6501.T", 100 },
            { "#6502.T", 100 },
            { "#7201.T", 100 },
            { "#7261.T", 100 },
            { "#8306.T", 100 },
            { "#0005.HK", 100 },
            { "#0291.HK", 100 },
            { "#0700.HK", 100 },
            { "#0728.HK", 100 },
            { "#0941.HK", 100 },
            { "#1088.HK", 100 },
            { "#1810.HK", 100 },
            { "#1928.HK", 100 },
            { "#2628.HK", 100 },
            { "#3328.HK", 100 },
            { "#3988.HK", 100 },
            { "XTIUSD", 1000 },
            { "XBRUSD", 1000 },
            { "XNGUSD", 10000 },
            { "AUDCAD", 100000 },
            { "AUDCHF", 100000 },
            { "AUDJPY", 100000 },
            { "AUDNZD", 100000 },
            { "AUDUSD", 100000 },
            { "CADCHF", 100000 },
            { "CADJPY", 100000 },
            { "CHFJPY", 100000 },
            { "EURAUD", 100000 },
            { "EURCAD", 100000 },
            { "EURCHF", 100000 },
            { "EURGBP", 100000 },
            { "EURJPY", 100000 },
            { "EURNZD", 100000 },
            { "EURUSD", 100000 },
            { "GBPAUD", 100000 },
            { "GBPCAD", 100000 },
            { "GBPCHF", 100000 },
            { "GBPJPY", 100000 },
            { "GBPNZD", 100000 },
            { "GBPUSD", 100000 },
            { "XAUUSD", 100 },
            { "XPTUSD", 100 },
            { "XPDUSD", 100 },
            { "NZDCAD", 100000 },
            { "NZDCHF", 100000 },
            { "NZDJPY", 100000 },
            { "NZDUSD", 100000 },
            { "XAGUSD", 5000 },
            { "XAGUSDmin", 1000 },
            { "USDCAD", 100000 },
            { "USDCHF", 100000 },
            { "USDCNH", 100000 },
            { "USDJPY", 100000 },
            { "USDMXN", 100000 },
            { "USDNOK", 100000 },
            { "USDSEK", 100000 },
            { "USDPLN", 100000 },
            { "USDSGD", 100000 },
            { "USDTRY", 100000 },
            { "USDZAR", 100000 },
            { "EURMXN", 100000 },
            { "EURNOK", 100000 },
            { "EURPLN", 100000 },
            { "EURSEK", 100000 },
            { "EURTRY", 100000 },
            { "GBPMXN", 100000 },
            { "GBPNOK", 100000 },
            { "GBPSEK", 100000 },
            { "GBPTRY", 100000 },
            { "Bitcoin", 1 },
            { "Ethereum", 5 },
            { "#HKG50", 1 },
            { "#JPN225", 1 },
            { "#US500", 1 },
            { "#US100", 1 },
            { "#US30", 1 },
            { "#CN300", 1 },
            { "#AUS200", 1 },
            { "#GER30", 1 },
            { "#EUSTX50", 1 },
            { "#ESP35", 1 },
            { "#FRA40", 1 },
            { "#UK100", 1 },
            { "#CHN50", 1 },
        };

    private static readonly Dictionary<string, double>
        PrefixPipValues = new()
        {
            { "XAUUSD", 1 },
            { "XAGUSDmin", 1 },
            { "XAGUSD", 50 },
            { "XPTUSD", 1 },
            { "XPDUSD", 1 },
            { "XTIUSD", 1 },
            { "XNGUSD", 1 },
            { "#CL", 1 },
            { "#Copper", 2.5 },
            { "#Corn", 0.5 },
            { "#Wheat", 0.5 },
            { "#Soybean", 0.5 },
            { "#ES", 0.5 },
            { "#NQ", 0.2 },
            { "#YM", 5 },
            { "#CN300", 5 },
            { "#NKD", 5 },
            { "#MSFT", 1 },
            { "#AAPL", 1 },
            { "#AXP", 1 },
            { "#MCD", 1 },
            { "#INTC", 1 },
            { "#IBM", 1 },
            { "#KO", 1 },
            { "#C", 1 },
            { "#BAC", 1 },
            { "#DIS", 1 },
            { "#BA", 1 },
            { "#QAN.AX", 1 },
            { "#APT.AX", 1 },
            { "#CSL.AX", 1 },
            { "#BHP.AX", 1 },
            { "#6501.T", 1 },
            { "#6502.T", 1 },
            { "#7201.T", 1 },
            { "#7261.T", 1 },
            { "#8306.T", 1 },
            { "#0005.HK", 1 },
            { "#0291.HK", 1 },
            { "#0700.HK", 1 },
            { "#0728.HK", 1 },
            { "#0941.HK", 1 },
            { "#1088.HK", 1 },
            { "#1810.HK", 1 },
            { "#1928.HK", 1 },
            { "#2628.HK", 1 },
            { "#3328.HK", 1 },
            { "#3988.HK", 1 },
            { "Bitcoin", 0.01 },
            { "Ethereum", 0.05 },
            { "#AUS200", 1 }, // 0.1
            { "#GER30", 1 }, // 0.1
            { "#EUSTX50", 1 }, // 0.1
            { "#ESP35", 1 }, // 0.1
            { "#FRA40", 1 }, // 0.1
            { "#UK100", 1 }, // 0.1
            { "#JPN225", 10 }, // 5
            { "#HKG50", 10 }, // 1 // need to confirm
            { "#CHN50", 1 }, // 0.1
            { "#US500", 1 }, // 5
            { "#US100", 1 }, // 2
            { "#US30", 10 }, // 5

            //{"#NG", 10},
            //{"#BRN", 10},
            //{"#ES", 0.5},
            //{"#NQ", 0.2},
            //{"#YM", 5,
        };
}

public class AllocationRebate
{
    public static AllocationRebate BuildSchema(long rate, long combinedRate)
        => new()
        {
            Rate = rate,
            CombinedRate = combinedRate,
        };


    public long Total => (CombinedRate ?? 0) + (Rate ?? 0);
    private long? CombinedRate { get; set; }
    private long? Rate { get; set; }
}

public class BaseRebate
{
    public static BaseRebate Build(long rate, long commission, long pip, double price = 0d)
        => new()
        {
            Pip = pip,
            Rate = rate,
            Commission = commission,
            Price = price,
        };

    public long Commission { get; set; }
    public long Pip { get; set; }
    public long Rate { get; set; }
    public double Price { get; set; }

    public long GetTotal() => Commission + Pip + Rate;
}

public class MtPrice
{
    public double Bid { get; set; }
    public int Digit { get; set; }
}