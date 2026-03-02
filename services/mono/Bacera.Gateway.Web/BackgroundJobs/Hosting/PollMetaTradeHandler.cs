using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Response;
using Bacera.Gateway.Web.Services.Interface;
using Hangfire;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.BackgroundJobs.Hosting;

public class PollMetaTradeHandler : IDisposable
{
    private readonly ILogger<PollMetaTradeHandler> _logger;
    private readonly IMyCache _myCache;
    private readonly IMessageQueueService _mqService;
    private readonly MyDbContextPool _myDbContextPool;
    private const int DefaultMaxNumberOfMessages = 10; // thread count
    private const int DelayTime = 5000;
    private readonly string _bcrTradeQueue;
    private readonly string _bcrEventTradeQueue;
    private readonly string _bcrSalesRebateTradeQueue;
    private readonly IBackgroundJobClient _backgroundJobClient;


    public PollMetaTradeHandler(ILogger<PollMetaTradeHandler> logger,
        MyDbContextPool myDbContextPool, IOptions<AmazonSQSOptions> options, IMyCache myCache,
        IMessageQueueService mqService, IBackgroundJobClient backgroundJobClient)
    {
        _logger = logger;
        _mqService = mqService;
        _backgroundJobClient = backgroundJobClient;
        _myCache = myCache;
        _myDbContextPool = myDbContextPool;
        _bcrTradeQueue = options.Value.BCRTrade;
        _bcrEventTradeQueue = options.Value.BCREventTrade;
        _bcrSalesRebateTradeQueue = options.Value.BCRSalesRebateTrade;
    }

    public async Task PollRebateTradeAsync(CancellationToken stoppingToken)
    {
        await ClearRelatedCache();
        IEnumerable<Task<bool>> processMessageTasks = new List<Task<bool>>();
        var cycleCount = 0;
        _logger.LogInformation("Trade queue polling started - checking every {delayMs}ms", DelayTime);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var messages = await _mqService.ReceiveAsync(_bcrTradeQueue, DefaultMaxNumberOfMessages, stoppingToken);
                
                // Only log when messages are found or every 5 minutes for health check
                if (messages.Count > 0)
                {
                    _logger.LogInformation("Received {count} messages from queue", messages.Count);
                    processMessageTasks = messages.Select(x => ProcessMessageAsync(x, stoppingToken)).ToList();
                    var results = await Task.WhenAll(processMessageTasks);
                    var successCount = results.Count(x => x);
                    _logger.LogInformation("Completed processing {successCount}/{totalCount} trade messages", successCount, messages.Count);
                }
                else if (cycleCount % 60 == 0) // Health check every 5 minutes (60 cycles * 5s = 300s)
                {
                    _logger.LogInformation("Trade queue health check - polling active, no messages in last 5 minutes");
                }
                
                await Task.Delay(DelayTime, stoppingToken);
                cycleCount++;
            }
            catch (OperationCanceledException e)
            {
                await Task.WhenAll(processMessageTasks);
                _logger.LogInformation("Task cancelled when receiving messages from queue, {message}", e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError("Error receiving messages from queue, {message}", e.Message);
                throw;
            }
        }

        _myDbContextPool.Dispose();
    }

    public async Task<bool> ProcessMessageAsync(MQMessage message, CancellationToken cancellationToken)
    {
        if (!MetaTrade.TryParse(message.Body, out var trade))
        {
            _logger.LogInformation("Trade is not valid, AccountNumber, invalid message: {message}", message.Body);
            await _mqService.DeleteAsync(_bcrTradeQueue, message, cancellationToken);
            return false;
        }

        var tenantId = await GetAccountTenantId(trade.AccountNumber, trade.ServiceId);
        if (tenantId == 0)
        {
            await _mqService.DeleteAsync(_bcrTradeQueue, message, cancellationToken);
            return false;
        }

        trade.TenantId = tenantId;
        var tenantCtx = await _myDbContextPool.BorrowTenant(tenantId, cancellationToken);
        try
        {
            var existId = await tenantCtx.TradeRebates
                .Where(x => x.Ticket == trade.Ticket && x.TradeServiceId == trade.ServiceId)
                .Select(x => x.Id)
                .SingleOrDefaultAsync(cancellationToken);


            trade.Id = 0;
            var tradeRebate = trade.ToTradeRebate();

            if (existId > 0)
            {
                _logger.LogInformation("Trade already exists, Ticket: {ticket}, ServiceId: {serviceId}",
                    trade.Ticket, trade.ServiceId);
                tradeRebate.Id = existId;
                await AddToOtherProcessTradeQueue(tenantId, tradeRebate);
                await _mqService.DeleteAsync(_bcrTradeQueue, message, cancellationToken);
                return false;
            }

            var account = await tenantCtx.Accounts
                .Where(x => x.AccountNumber == tradeRebate.AccountNumber)
                .Select(x => new { x.Id, x.CurrencyId, x.ReferPath })
                .FirstOrDefaultAsync(cancellationToken);
            if (account == null) return false;

            tradeRebate.AccountId = account.Id;
            tradeRebate.ReferPath = account.ReferPath;
            tradeRebate.CurrencyId = account.CurrencyId;
            tenantCtx.TradeRebates.Add(tradeRebate);
            await tenantCtx.SaveChangesAsync(cancellationToken);
            // _backgroundJobClient.Enqueue<IProcessAccountStatJob>("account-stat-event",
            //     x => x.TradeClosedAsync(tenantId, tradeRebate.Id));
            await _mqService.DeleteAsync(_bcrTradeQueue, message, cancellationToken);
            await AddToOtherProcessTradeQueue(tenantId, tradeRebate);
            return true;
        }
        finally
        {
            _myDbContextPool.ReturnTenant(tenantCtx);
        }
    }

    private async Task AddToOtherProcessTradeQueue(long tenantId, TradeRebate trade)
    {
        var model = EventShopPointTransaction.MQSource.Build(EventShopPointTransactionSourceTypes.Trade
            , trade.Id, tenantId);
        var json = model.ToString();
        await _mqService.SendAsync(json, _bcrSalesRebateTradeQueue, messageGroupId: tenantId.ToString());

        if (trade.Reason is 1 or 2) return;
        if (trade.ClosedOn - trade.OpenedOn <= TimeSpan.FromSeconds(60)) return;

        await _mqService.SendAsync(json, _bcrEventTradeQueue, messageGroupId: tenantId.ToString());
        // _backgroundJobClient.Enqueue<IRebateJob>("intensive-job",
        //     x => x.GenerateByTradeRebateIdAsync(tenantId, trade.Id));
    }

    private async Task<long> GetAccountTenantId(long accountNumber, int serviceId)
    {
        var cacheKey = CacheKeys.GetAccountTenantIdHashKey();
        var field = $"{accountNumber}:{serviceId}";
        var cacheValue = await _myCache.HGetStringAsync(cacheKey, field);
        if (!string.IsNullOrWhiteSpace(cacheValue)) return long.Parse(cacheValue);

        var centralDbCtx = await _myDbContextPool.BorrowCentralAsync();
        var dict = new Dictionary<long, TenantDbContext>();

        try
        {
            var tenantIds = await centralDbCtx.Tenants.Select(x => x.Id).ToListAsync();
            foreach (var tid in tenantIds)
            {
                var tenantCtx = await _myDbContextPool.BorrowTenant(tid);
                dict.Add(tid, tenantCtx);
            }

            var tasks = dict.Select(async x =>
            {
                var tid = x.Key;
                var ctx = dict[tid];
                var exists = await ctx.Accounts.AnyAsync(a =>
                    a.AccountNumber == accountNumber && a.ServiceId == serviceId && a.Status == 0);
                return exists ? tid : 0;
            });

            var results = await Task.WhenAll(tasks);
            var tenantId = results.FirstOrDefault(x => x != 0);
            // if (tenantId == 0) return 0;
            cacheValue = tenantId.ToString();
            await _myCache.HSetStringAsync(cacheKey, field, cacheValue);
            return tenantId;
        }
        finally
        {
            dict.Values.ForEach(x => _myDbContextPool.ReturnTenant(x));
            _myDbContextPool.ReturnCentral(centralDbCtx);
        }
    }

    private async Task ClearRelatedCache()
    {
        await _myCache.HSetDeleteByKeyAsync(CacheKeys.GetAccountTenantIdHashKey());
    }

    public void Dispose()
    {
        _myDbContextPool.Dispose();
    }
}


// private async Task ProcessMessageAsync(SQSMessage message, CancellationToken cancellationToken)
//     {
//         // check if closed time is after 1 minutes after open time
//         var trade = JsonConvert.DeserializeObject<TradeViewModel>(message.Body);
//         if (trade == null)
//         {
//             _logger.LogInformation("Trade is not valid, AccountNumber, invalid message: {message}", message.Body);
//             await DeleteMessageAsync(cancellationToken, message);
//             return;
//         }
//
//         var tradeRebate = trade.ToTradeRebate();
//         var tenantId = await GetAccountTenantId(tradeRebate.AccountNumber);
//         if (tenantId == 0)
//         {
//             _logger.LogInformation("Account not in tenants: {accountNumber}", tradeRebate.AccountNumber);
//             await DeleteMessageAsync(cancellationToken, message);
//             return;
//         }
//
//         var ctx = await _myDbContextPool.BorrowTenant(tenantId, cancellationToken);
//         try
//         {
//             // var exists = await ctx.TradeRebates.AnyAsync(
//             //     x => x.Ticket == tradeRebate.Ticket && x.TradeServiceId == tradeRebate.TradeServiceId,
//             //     cancellationToken);
//             // if (exists)
//             // {
//             //     _logger.LogInformation("TradeRebate already exists, Ticket: {ticket}, ServiceId: {serviceId}",
//             //         tradeRebate.Ticket, tradeRebate.TradeServiceId);
//             //     await DeleteMessageAsync(cancellationToken, message);
//             //     return;
//             // }
//             //
//             // var account = await GetAccountByAccountNumber(tradeRebate.AccountNumber);
//             // if (account == null)
//             // {
//             //     _logger.LogInformation("Account not found, AccountNumber: {accountNumber}", tradeRebate.AccountNumber);
//             //     await DeleteMessageAsync(cancellationToken, message);
//             //     return;
//             // }
//             //
//             // tradeRebate.AccountId = account.Id;
//             // tradeRebate.CurrencyId = account.CurrencyId;
//             // tradeRebate.Status = (int)TradeRebateStatusTypes.Created;
//             // tradeRebate.RuleType = 99;
//             // ctx.TradeRebates.Add(tradeRebate);
//             // _logger.LogInformation(
//             //     "TradeRebate added, Ticket: {ticket}, ServiceId: {serviceId}, TradeRebate: {tradeRebate}",
//             //     tradeRebate.Ticket,
//             //     tradeRebate.TradeServiceId, tradeRebate);
//             await ctx.SaveChangesAsync(cancellationToken);
//             await DeleteMessageAsync(cancellationToken, message);
//         }
//         finally
//         {
//             _myDbContextPool.ReturnTenant(ctx);
//         }
//     }

// private async Task<Account?> GetAccountByAccountNumber(long accountNumber)
// {
//     var cacheKey = RedisKeyRepository.GetAccountNumberToAccountHashKey();
//     var accountJson = await _cache.HashGetStringAsync(cacheKey, accountNumber.ToString());
//     if (accountJson != null && Account.TryParse(accountJson, out var account))
//     {
//         return account;
//     }
//
//     var tid = await GetAccountTenantId(accountNumber);
//     var ctx = await _myDbContextPool.BorrowTenant(tid);
//     try
//     {
//         account = await ctx.Accounts.FirstOrDefaultAsync(x => x.AccountNumber == accountNumber);
//         if (account == null) return null;
//         accountJson = JsonConvert.SerializeObject(account);
//         await _cache.HashSetStringAsync(cacheKey, accountNumber.ToString(), accountJson);
//         return account;
//     }
//     finally
//     {
//         _myDbContextPool.ReturnTenant(ctx);
//     }
// }