using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services.Common;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services;

public class EventService(
    TenantDbContext tenantDbContext,
    ILogger<EventService> logger,
    IMyCache myCache,
    ITradingApiService tradingApiService,
    AccountingService accountingService,
    RebateService rebateService)
{
    public async Task<long> GetEventIdByKeyAsync(string key)
    {
        var value = await myCache.GetStringAsync(CacheKeys.EventKeyToIdCache);
        if (value != "0" && long.TryParse(value, out var id)) return id;

        id = await tenantDbContext.Events
            .Where(x => x.Key == key)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();
        await myCache.SetStringAsync(CacheKeys.EventKeyToIdCache, id.ToString(), TimeSpan.FromDays(3));
        return id;
    }

    public async Task<bool> ProcessAdjustSourceAsync(long eventId, long accountId)
    {
        await Task.Delay(0);
        return true;
    }

    public async Task<bool> ProcessOpenAccountSourceAsync(long eventId, long accountId, bool test = false)
    {
        try
        {
            var account = await tenantDbContext.Accounts
                .Where(x => x.Id == accountId)
                .Select(x => new { x.Id, x.Role, x.SalesAccountId, x.AgentAccountId })
                .SingleAsync();

            // ========= For testing purpose =========
            if (test)
            {
                // account = new
                // {
                //     Id = 52473L, Role = (short)400, SalesAccountId = (long?)52199,
                //     AgentAccountId = (long?)52465
                // };

                account = new
                {
                    Id = 52453L, Role = (short)400, SalesAccountId = (long?)52199,
                    AgentAccountId = (long?)52447
                };


                // account = new
                // {
                //     Id = 52447L, Role = (short)300, SalesAccountId = (long?)52199,
                //     AgentAccountId = (long?)52446
                // };
            }

            // ========= For testing purpose =========
            if (account.SalesAccountId == null) return true;

            var salesEventPartyId = await GetEventPartyIdByAccountIdAsync(eventId, account.SalesAccountId.Value);
            switch (account.Role)
            {
                case (int)AccountRoleTypes.Agent when salesEventPartyId != 0:
                {
                    var agentPoint = EventShopClientPoint.Build(account.Id, account.SalesAccountId.Value,
                        AccountRoleTypes.Sales, 5);
                    tenantDbContext.EventShopClientPoints.Add(agentPoint);
                    break;
                }
                case (int)AccountRoleTypes.Client when account.AgentAccountId == null && salesEventPartyId != 0:
                {
                    var clientPoint = EventShopClientPoint.Build(account.Id, account.SalesAccountId.Value,
                        AccountRoleTypes.Sales, 6);
                    tenantDbContext.EventShopClientPoints.Add(clientPoint);
                    break;
                }

                case (int)AccountRoleTypes.Client when account.AgentAccountId != null && salesEventPartyId != 0:
                {
                    var agentEventPartyId =
                        await GetEventPartyIdByAccountIdAsync(eventId, account.AgentAccountId.Value);
                    if (agentEventPartyId != 0)
                    {
                        var clientPoint = EventShopClientPoint.Build(account.Id, account.AgentAccountId.Value,
                            AccountRoleTypes.Agent);
                        tenantDbContext.EventShopClientPoints.Add(clientPoint);
                    }

                    var isAgentNewForSales = await IsNewAccountForSalesAsync(account.AgentAccountId.Value,
                        account.SalesAccountId.Value);
                    if (isAgentNewForSales)
                    {
                        var clientPoint = EventShopClientPoint.Build(account.Id, account.SalesAccountId.Value,
                            AccountRoleTypes.Sales);
                        tenantDbContext.EventShopClientPoints.Add(clientPoint);
                    }

                    break;
                }
                case (int)AccountRoleTypes.Client when account.AgentAccountId != null && salesEventPartyId == 0:
                {
                    var agentEventPartyId =
                        await GetEventPartyIdByAccountIdAsync(eventId, account.AgentAccountId.Value);
                    if (agentEventPartyId != 0)
                    {
                        var clientPoint = EventShopClientPoint.Build(account.Id, account.AgentAccountId.Value,
                            AccountRoleTypes.Agent);
                        tenantDbContext.EventShopClientPoints.Add(clientPoint);
                    }

                    break;
                }
            }

            await tenantDbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            logger.LogError("Process Open Account Failed, message: {message}", e.Message);
            return false;
        }
    }

    public async Task<bool> ProcessDepositSourceAsync(long eventId, long depositId, bool test = false)
    {
        var deposit = await tenantDbContext.Deposits
            .Where(x => x.Id == depositId)
            .Select(x => new { x.Id, x.PartyId, x.TargetAccountId, x.Amount })
            .SingleOrDefaultAsync();

        // ========= For testing purpose =========
        if (test)
        {
            deposit = new { Id = 1L, PartyId = 362274L, TargetAccountId = (long?)52453L, Amount = 2300L };
        }

        if (deposit == null || deposit.TargetAccountId == null || deposit.TargetAccountId == 0)
        {
            logger.LogInformation("Deposit is not valid, depositId: {depositId}", depositId);
            return false;
        }

        var accountId = deposit.TargetAccountId.Value;
        var directAgentId = await GetAgentAccountIdByAccountIdAsync(accountId);
        if (directAgentId != 0)
        {
            var agentEventPartyId = await GetEventPartyIdByAccountIdAsync(eventId, directAgentId);
            if (agentEventPartyId != 0)
            {
                await TryEnsureParentHasClientPoint(accountId, directAgentId);
            }

            // var salesAccountId = await GetSalesAccountIdByAccountIdAsync(accountId);
            // if (salesAccountId != 0)
            // {
            //     var salesEventPartyId = await GetEventPartyIdByAccountIdAsync(eventId, salesAccountId);
            //     if (salesEventPartyId != 0)
            //     {
            //         await ProcessDepositForParentAsync(eventId, directAgentId, deposit.Amount);
            //     }
            // }
        }

        var result = await ProcessDepositForParentAsync(eventId, accountId, deposit.Amount);
        // result &= await ProcessDepositForParentAsync(eventId, directAgentId, 0, depositJson);
        return result;
    }

    public async Task<bool> ProcessTradeSourceAsync(long eventId, long tradeRebateId, bool test = false)
    {
        logger.LogInformation("ProcessTradeSourceAsync_tradeRebateId_start: {tradeRebateId}", tradeRebateId);
        var trade = await tenantDbContext.TradeRebates
            .Where(x => x.Id == tradeRebateId)
            .SingleOrDefaultAsync();
        if (trade == null || trade.AccountId == null || trade.AccountId == 0)
        {
            logger.LogInformation("TradeRebate is not valid, tradeRebateId: {tradeRebateId}", tradeRebateId);
            return true;
        }

        if (test)
        {
            trade.AccountId = 52453;
            trade.Volume = 320;
        }

        if (trade.ClosedLessThanOneMinute())
        {
            logger.LogInformation("ProcessTradeSourceAsync_tradeRebateId_closedLessThanOneMin: {serviceId}, ticket: {ticket}"
                , trade.TradeServiceId
                , trade.Ticket);
            return true;
        }

        var tradeJson = JsonConvert.SerializeObject(new
            { trade.AccountNumber, trade.Ticket, trade.Volume, ServiceId = trade.TradeServiceId });

        // var client
        var result = await ProcessTradeRewardAsync(eventId, tradeRebateId);
        var accountId = trade.AccountId.Value;
        var selfEventPartyId = await GetEventPartyIdByAccountIdAsync(eventId, accountId);
        logger.LogInformation("ProcessTradeSourceAsync_selfEventPartyId: {selfEventPartyId}", selfEventPartyId);
        if (selfEventPartyId != 0)
        {
            if (await CheckIfTransactionExistAsync(selfEventPartyId, accountId, EventShopPointTransactionSourceTypes.Trade, tradeJson))
            {
                logger.LogInformation("ProcessTradeSourceAsync_EventTransaction_exist: {tradeRebateId}", tradeRebateId);
            }
            else
            {
                // 客户积分计算：考虑USC汇率转换 1 volumn == 0.01 point for USD, 1 volumn == 0.0001 point for USC
                var pointMultiplier = trade.CurrencyId == (int)CurrencyTypes.USC ? 0.01 : 1.0;
                // 缩小100倍转换成手，原因是只需要模拟前端扩大100*100倍
                var point = (long)((((decimal)trade.Volume) / 100m).ToScaledFromCents() * (decimal)pointMultiplier);
                result &= await ChangePointAsync(selfEventPartyId, point, EventShopPointTransactionSourceTypes.Trade, tradeJson, accountId,
                    tradeRebateId);
            }
        }

        var directAgentId = await GetAgentAccountIdByAccountIdAsync(accountId);
        logger.LogInformation("ProcessTradeSourceAsync_directAgentId: {directAgentId}", directAgentId);
        if (directAgentId != 0 && await GetAccountRoleByIdAsync(directAgentId) == AccountRoleTypes.Agent)
        {
            var agentEventPartyId = await GetEventPartyIdByAccountIdAsync(eventId, directAgentId);
            logger.LogInformation("ProcessTradeSourceAsync_agentEventPartyId: {agentEventPartyId}", agentEventPartyId);
            if (agentEventPartyId != 0)
            {
                await TryEnsureParentHasClientPoint(accountId, directAgentId);
                // 代理积分计算：考虑USC汇率转换 1 volumn == 0.01 point for USD, 1 volumn == 0.0001 point for USC
                var pointMultiplier = trade.CurrencyId == (int)CurrencyTypes.USC ? 0.01 : 1.0;
                // 缩小100倍转换成手，原因是只需要模拟前端扩大100 * 100倍
                var point = (long)((((decimal)trade.Volume) / 100m).ToScaledFromCents() * 0.3m * (decimal)pointMultiplier);
                if (point > 0)
                {
                    if (await CheckIfTransactionExistAsync(agentEventPartyId, directAgentId, EventShopPointTransactionSourceTypes.Trade, tradeJson))
                    {
                        logger.LogInformation("ProcessTradeSourceAsync_AgentTransaction_exist: {tradeRebateId}", tradeRebateId);
                    }
                    else
                    {
                        result &= await ChangePointAsync(agentEventPartyId, point, EventShopPointTransactionSourceTypes.Trade, tradeJson,
                            directAgentId, tradeRebateId);
                    }
                }
            }
            
            await ProcessTradeForParentAsync(eventId, directAgentId, trade.Volume, trade.CurrencyId);
        }

        result &= await ProcessTradeForParentAsync(eventId, accountId, trade.Volume, trade.CurrencyId);
        return result;
    }


    private async Task<bool> ProcessDepositForParentAsync(long eventId, long childAccountId, long amount)
    {
        var result = true;
        var ids = await GetValidClientPointIdsAsync(childAccountId, AccountRoleTypes.Agent);
        foreach (var id in ids)
        {
            result &= await AddDepositPointByIdAsync(id, amount);
            // result &= await TryReleaseClientDepositPointByIdAsync(eventId, id);
        }

        return result;
    }

    private async Task<bool> ProcessTradeForParentAsync(long eventId, long childAccountId, int volume, int currencyId)
    {
        var result = true;
        var ids = await GetValidClientPointIdsAsync(childAccountId);
        logger.LogInformation("ProcessTradeForParentAsync_ids: {ids}", ids);
        foreach (var id in ids)
        {
            result &= await AddTradePointByIdAsync(id, volume, currencyId);
            // result &= await TryReleaseClientDepositPointByIdAsync(eventId, id);
        }

        return result;
    }

    private async Task<bool> ProcessTradeRewardAsync(long eventId, long tradeRebateId)
    {
        var result = await ProcessTradeRewardForClientAsync(eventId, tradeRebateId);
        result &= await ProcessTradeRewardForParentAsync(eventId, tradeRebateId);
        return result;
    }

    public async Task<bool> ProcessTradeRewardForParentAsync(long eventId, long tradeRebateId)
    {
        var trade = await tenantDbContext.TradeRebates
            .Where(x => x.Id == tradeRebateId)
            .Select(x => new
            {
                x.Account!.Type, x.Account.AgentAccountId, x.Account.SalesAccountId, x.Ticket, x.TradeServiceId
            })
            .SingleOrDefaultAsync();
        if (trade?.AgentAccountId == null) return true;

        var parentIds = new[] { trade.AgentAccountId, trade.SalesAccountId }
            .Where(x => x != null)
            .Select(x => x!.Value);

        var isAlphaTradeAccount = trade.Type is (short)AccountTypes.Alpha or (short)AccountTypes.AlphaPlus;
        foreach (var parentId in parentIds)
        {
            var eventPartyId = await GetEventPartyIdByAccountIdAsync(eventId, parentId);

            if (eventPartyId == 0) return true;
            var account = await tenantDbContext.Accounts
                .Where(x => x.Id == parentId)
                .Select(x => new { x.PartyId, TargetCurrencyId = (CurrencyTypes)x.CurrencyId, x.FundType })
                .SingleAsync();

            var rawRewards = await tenantDbContext.EventShopRewards
                .Where(x => x.EventPartyId == eventPartyId)
                .Where(x => x.Status == (int)EventShopRewardStatusTypes.Active)
                .Where(x => x.EventShopItem.Type == (short)EventShopItemTypes.AgentReward ||
                            x.EventShopItem.Type == (short)EventShopItemTypes.SalesReward)
                .Where(x => x.EffectiveTo > DateTime.UtcNow)
                .Select(x => new { x.Id, x.EventShopItem.Type, x.EventShopItem.Configuration, x.EventPartyId })
                .ToListAsync();

            foreach (var rawReward in rawRewards)
            {
                if (!EventShopItem.RewardConfiguration.TryParse(rawReward.Configuration, out var config))
                    continue;

                long rewardAmountInCents;
                string comments;
                switch (config.RewardType)
                {
                    case EventShopRewardTypes.Point1000:
                    {
                        rewardAmountInCents = isAlphaTradeAccount ? 30 : 50;
                        comments = $"Agent RCA1#{trade.Ticket}";
                        break;
                    }
                    case EventShopRewardTypes.Point3000:
                    {
                        rewardAmountInCents = isAlphaTradeAccount ? 50 : 100;
                        comments = $"Agent RCA2#{trade.Ticket}";
                        break;
                    }
                    case EventShopRewardTypes.Point5000:
                    {
                        rewardAmountInCents = isAlphaTradeAccount ? 100 : 200;
                        comments = $"Agent RCA3#{trade.Ticket}";
                        break;
                    }
                    case EventShopRewardTypes.Point250:
                    {
                        rewardAmountInCents = isAlphaTradeAccount ? 50 : 100;
                        comments = $"Agent RCS3#{trade.Ticket}";
                        break;
                    }
                    default: return false;
                }

                var rewardRebate = EventShopRewardRebate.Build(rawReward.Id, trade.Ticket, rewardAmountInCents,
                    EventShopRewardRebateStatusTypes.Succeed);
                tenantDbContext.EventShopRewardRebates.Add(rewardRebate);

                var exchangeRate = await rebateService.GetMtExchangeRate(trade.TradeServiceId, CurrencyTypes.USD,
                    account.TargetCurrencyId);

                var actualAmount = (long)Math.Round(rewardAmountInCents * exchangeRate, 0);

                var walletId = await accountingService.EnsureWalletExistsAsync(account.PartyId,
                    account.TargetCurrencyId, (FundTypes)account.FundType);

                var walletAdjust = WalletAdjust.BuildFromEventReward(walletId, actualAmount, comments);
                tenantDbContext.WalletAdjusts.Add(walletAdjust);
                await tenantDbContext.SaveChangesAsync();

                await accountingService.WalletChangeBalanceAsync(walletId, walletAdjust.Id, actualAmount);
            }
        }

        return true;
    }

    private async Task<bool> ProcessTradeRewardForClientAsync(long eventId, long tradeRebateId)
    {
        var item = await tenantDbContext.TradeRebates
            .Where(x => x.Id == tradeRebateId)
            .Where(x => x.AccountId != null)
            .Select(x => new
            {
                AccountId = (long)x.AccountId!, AccountType = x.Account!.Type, x.Account!.AccountNumber,
                x.Ticket, x.Volume, ServiceId = x.TradeServiceId
            })
            .SingleOrDefaultAsync();
        if (item == null) return true;
        var selfEventPartyId = await GetEventPartyIdByAccountIdAsync(eventId, item.AccountId);
        if (selfEventPartyId == 0) return true;
        var rawReward = await tenantDbContext.EventShopRewards
            .Where(x => x.EventPartyId == selfEventPartyId)
            .Where(x => x.Status == (int)EventShopRewardStatusTypes.Active)
            .Where(x => x.EventShopItem.Type == (short)EventShopItemTypes.ClientReward)
            .Where(x => x.EffectiveTo > DateTime.UtcNow)
            .Select(x => new { x.Id, x.EventShopItem.Type, x.EventShopItem.Configuration })
            .SingleOrDefaultAsync();
        if (rawReward == null || !EventShopItem.RewardConfiguration.TryParse(rawReward.Configuration, out var config))
            return true;

        var isAlpha = item.AccountType is (short)AccountTypes.Alpha or (short)AccountTypes.AlphaPlus;
        int rewardUnitInCents;
        string comments;
        switch (config.RewardType)
        {
            case EventShopRewardTypes.Point1000:
            {
                rewardUnitInCents = isAlpha ? 50 : 100;
                comments = $"Agent RCC1#{item.Ticket}";
                break;
            }
            case EventShopRewardTypes.Point3000:
            {
                rewardUnitInCents = isAlpha ? 100 : 200;
                comments = $"Agent RCC2#{item.Ticket}";
                break;
            }
            case EventShopRewardTypes.Point5000:
            {
                rewardUnitInCents = isAlpha ? 100 : 300;
                comments = $"Agent RCC3#{item.Ticket}";
                break;
            }
            default: return false;
        }

        var rewardAmountInCents = (long)Math.Round(rewardUnitInCents * item.Volume / 100m, 0);
        var rewardRebate = EventShopRewardRebate.Build(rawReward.Id, item.Ticket, rewardAmountInCents,
            EventShopRewardRebateStatusTypes.Succeed);
        tenantDbContext.EventShopRewardRebates.Add(rewardRebate);
        await tenantDbContext.SaveChangesAsync();
        await ChangeEventShopRewardTotalPointAsync(rawReward.Id, rewardAmountInCents);

        var amountInDecimal = rewardAmountInCents / 100m;
        await tradingApiService.ChangeBalance(item.ServiceId, item.AccountNumber, amountInDecimal, comments);
        return true;
    }


    private async Task<bool> TryReleaseClientDepositPointByIdAsync(long eventId, long id)
    {
        await using var scope = await tenantDbContext.Database.BeginTransactionAsync();
        try
        {
            var item = await tenantDbContext.EventShopClientPoints
                .FromSqlInterpolated($"select * from event.\"_EventShopClientPoint\" where \"Id\" = {id} for update")
                .SingleAsync();

            var parentEventPartyId = await GetEventPartyIdByAccountIdAsync(eventId, item.ParentAccountId);

            var volumeUnit = item.GetVolumeInUnit();
            if (volumeUnit != 0 && parentEventPartyId != 0 &&
                item.ParentAccountRole is (short)AccountRoleTypes.Agent or (short)AccountRoleTypes.Sales)
            {
                var openAccountUnit = item.GetOpenAccountUnit();
                var content = JsonConvert.SerializeObject(new { AccountId = item.ChildAccountId });
                if (openAccountUnit > 0)
                {
                    item.OpenAccount = 0;
                    var point = openAccountUnit.ToScaledFromCents();
                    await ChangePointAsync(parentEventPartyId, point,
                        EventShopPointTransactionSourceTypes.OpenAccount, content, item.ParentAccountId,
                        item.ChildAccountId);
                }

                if (item.ParentAccountRole is (short)AccountRoleTypes.Agent)
                {
                    var depositAmountUnit = item.GetDepositAmountInUnit();
                    var releaseUnit = Math.Min(volumeUnit, depositAmountUnit);
                    if (releaseUnit > 0)
                    {
                        var point = releaseUnit.ToScaledFromCents();
                        await ChangePointAsync(parentEventPartyId, point,
                            EventShopPointTransactionSourceTypes.Deposit, content, item.ParentAccountId);
                        item.AddUnitToDepositAmount(-releaseUnit);
                        item.AddUnitToVolume(-releaseUnit);
                    }
                }
            }

            tenantDbContext.EventShopClientPoints.Update(item);
            await tenantDbContext.SaveChangesAsync();
            await scope.CommitAsync();
            return true;
        }
        catch (Exception e)
        {
            logger.LogError("Process Release Frozen Point Failed, message: {message}", e.Message);
            await scope.RollbackAsync();
            return false;
        }
    }


    private async Task<bool> AddDepositPointByIdAsync(long id, long amount)
    {
        try
        {
            await tenantDbContext.Database.ExecuteSqlInterpolatedAsync
            (
                $"""
                 update event."_EventShopClientPoint" set "DepositAmount" = {amount} + "DepositAmount", "UpdatedOn" = now() where "Id" = {id}
                 """
            );
            tenantDbContext.ChangeTracker.Clear();
            return true;
        }
        catch (Exception e)
        {
            logger.LogError("Add Deposit Point Failed, message: {message}", e.Message);
            return false;
        }
    }

    private async Task TryEnsureParentHasClientPoint(long accountId, long parentAccountId)
    {
        var exists = await tenantDbContext.EventShopClientPoints
            .AnyAsync(x => x.ChildAccountId == accountId && x.ParentAccountId == parentAccountId);
        if (exists) return;
        var role = await tenantDbContext.Accounts
            .Where(x => x.Id == parentAccountId)
            .Select(x => (AccountRoleTypes)x.Role)
            .SingleOrDefaultAsync();
        var item = EventShopClientPoint.Build(accountId, parentAccountId, role, 0);
        tenantDbContext.EventShopClientPoints.Add(item);
        await tenantDbContext.SaveChangesAsync();
    }

    // sales only release open account point
    private Task<List<long>> GetValidClientPointIdsAsync(long accountId, AccountRoleTypes? parentRole = null)
        => tenantDbContext.EventShopClientPoints
            .Where(x => parentRole == null || x.ParentAccountRole == (short)parentRole)
            .Where(x => x.ParentAccountRole != (int)AccountRoleTypes.Sales || x.OpenAccount > 0)
            .Where(x => x.ChildAccountId == accountId)
            .Select(x => x.Id)
            .ToListAsync();

    private async Task<long> GetAgentAccountIdByAccountIdAsync(long accountId)
        => await tenantDbContext.Accounts
            .Where(x => x.Id == accountId && x.AgentAccountId != null)
            .Select(x => x.AgentAccountId)
            .SingleOrDefaultAsync() ?? 0;

    private Task<long> GetEventPartyIdByAccountIdAsync(long eventId, long accountId)
        => tenantDbContext.EventParties
            .Where(x => x.EventId == eventId)
            .Where(x => x.Party.Accounts.Any(y => y.Id == accountId))
            .Select(x => x.Id)
            .SingleOrDefaultAsync();

    private Task<bool> CheckIfTransactionExistAsync(long eventPartyId, long accountId,
        EventShopPointTransactionSourceTypes source, string sourceContent)
        => tenantDbContext.EventShopPointTransactions
            .Where(x => x.EventPartyId == eventPartyId && x.AccountId == accountId)
            .Where(x => x.SourceType == (short)source && x.SourceContent == sourceContent)
            .AnyAsync();

    private async Task<AccountRoleTypes> GetAccountRoleByIdAsync(long accountId)
    {
        var role = await tenantDbContext.Accounts
            .Where(x => x.Id == accountId).Select(x => x.Role)
            .SingleOrDefaultAsync();
        return (AccountRoleTypes)role;
    }
    

    private Task<bool> IsNewAccountForSalesAsync(long childAccountId, long salesAccountId)
        => tenantDbContext.EventShopClientPoints
            .AnyAsync(x =>
                x.ParentAccountRole == (int)AccountRoleTypes.Sales && x.ChildAccountId == childAccountId &&
                x.ParentAccountId == salesAccountId);

    private async Task<bool> AddTradePointByIdAsync(long id, int volume, int currencyId)
    {
        try
        {
            // 应用USC汇率转换
            var volumeMultiplier = currencyId == (int)CurrencyTypes.USC ? 0.01 : 1.0;
            var adjustedVolume = (long)(volume * volumeMultiplier);

            await tenantDbContext.Database.ExecuteSqlInterpolatedAsync
            (
                $"""
                 update event."_EventShopClientPoint" set "Volume" = {adjustedVolume} + "Volume", "UpdatedOn" = now() where "Id" = {id}
                 """
            );
            tenantDbContext.ChangeTracker.Clear();
            return true;
        }
        catch (Exception e)
        {
            logger.LogError("Add Trade Point Failed, message: {message}", e.Message);
            return false;
        }
    }

    public async Task<bool> ChangePointAsync(long eventPartyId, long point,
        EventShopPointTransactionSourceTypes type, string sourceContent = "{}", long? accountId = null,
        long sourceId = 0, CancellationToken token = default)
    {
        try
        {
            await tenantDbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 update event."_EventShopPoint" set "Point" = {point.ToScaledFromCents()} + "Point", "TotalPoint" = {point.ToScaledFromCents()} + "TotalPoint", "UpdatedOn" = now()
                 where "EventPartyId" = {eventPartyId};

                 insert into event."_EventShopPointTransaction" ("EventPartyId", "Point", "SourceType", "SourceId", "SourceContent", "Status", "AccountId", "CreatedOn", "UpdatedOn")
                 values ({eventPartyId}, {point.ToScaledFromCents()}, {(short)type}, {sourceId}, {sourceContent}, {(short)EventShopPointTransactionStatusTypes.Success}, {accountId}, now(), now());
                 """, cancellationToken: token);
            tenantDbContext.ChangeTracker.Clear();
            return true;
        }
        catch (Exception e)
        {
            logger.LogError("Add Trade Point Failed, message: {message}", e.Message);
            return false;
        }
    }
    
    public async Task<bool> ChangeEventShopRewardTotalPointAsync(long id, long point, CancellationToken token = default)
    {
        try
        {
            await tenantDbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 update event."_EventShopReward" set "TotalPoint" = "TotalPoint" + {point} where "Id" = {id};
                 """
                , cancellationToken: token);
            tenantDbContext.ChangeTracker.Clear();
            return true;
        }
        catch (Exception e)
        {
            logger.LogError("Add Trade Point Failed, message: {message}", e.Message);
            return false;
        }
    }
}