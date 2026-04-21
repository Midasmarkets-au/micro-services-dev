using Bacera.Gateway.Context;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.Services.Interface;
using Hangfire;

namespace Bacera.Gateway.Web.BackgroundJobs.Hosting;

public class PollMetaTradeHandler : IDisposable
{
    private readonly ILogger<PollMetaTradeHandler> _logger;
    private readonly IMyCache _myCache;
    private readonly IMessageQueueService _mqService;
    private readonly MyDbContextPool _myDbContextPool;
    private const int DefaultMaxNumberOfMessages = 10; // thread count
    private const int DelayTime = 5000;
    // [MIGRATED] All BCRTrade and BCREventTrade queue fields removed — pipelines replaced by NATS JetStream.
    // BCRTrade: scheduler/src/jobs/trade_monitor.rs + trade_handler.rs
    // BCREventTrade: scheduler/src/jobs/event_trade_handler.rs
    // private readonly string _bcrTradeQueue;
    // private readonly string _bcrSalesRebateTradeQueue;
    // private readonly string _bcrEventTradeQueue;
    private readonly IBackgroundJobClient _backgroundJobClient;


    public PollMetaTradeHandler(ILogger<PollMetaTradeHandler> logger,
        MyDbContextPool myDbContextPool, IMyCache myCache,
        IMessageQueueService mqService, IBackgroundJobClient backgroundJobClient)
    {
        _logger = logger;
        _mqService = mqService;
        _backgroundJobClient = backgroundJobClient;
        _myCache = myCache;
        _myDbContextPool = myDbContextPool;
    }

    // [MIGRATED] PollRebateTradeAsync removed — BCRTrade queue no longer consumed here.
    // public async Task PollRebateTradeAsync(CancellationToken stoppingToken) { ... }

    // [MIGRATED] ProcessMessageAsync removed — BCRTrade queue consumer no longer in use.
    // public async Task<bool> ProcessMessageAsync(MQMessage message, CancellationToken cancellationToken) { ... }

    // [MIGRATED] AddToOtherProcessTradeQueue, GetAccountTenantId, ClearRelatedCache removed
    // — no longer called; BCRTrade pipeline replaced by scheduler NATS JetStream.

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