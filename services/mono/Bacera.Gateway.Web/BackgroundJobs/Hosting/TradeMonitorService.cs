using System.Globalization;
using Bacera.Gateway.Context;
using Bacera.Gateway.Integration;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services.Interface;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.BackgroundJobs.Hosting;

public class TradeMonitorService(
    IServiceProvider serviceProvider,
    ILogger<TradeMonitorService> logger,
    IOptions<AmazonSQSOptions> sqsOptions,
    IMyCache myCache,
    CentralDbContext centralDbContext,
    MyDbContextPool myDbContextPool,
    IMessageQueueService mqService)
    : IDisposable
{
    //private readonly Dictionary<int, MetaTrade4DbContext> _mt4TradeServiceCtxPool = new();
    private readonly Dictionary<int, MetaTrade5DbContext> _mt5TradeServiceCtxPool = new();
    private readonly string _bcrTradeQueueName = sqsOptions.Value.BCRTrade;

    ~TradeMonitorService()
    {
        Dispose(false);
    }

    public async Task ExecuteJobAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Trade Monitoring Service is running...");
        
        try
        {
            logger.LogInformation("Setting up MetaTrade DbContext Pool...");
            await SetUpMetaTradeDbContextPool(stoppingToken);

            logger.LogInformation($"MT5 DbContext pool has {_mt5TradeServiceCtxPool.Count} connections");
            
            if (_mt5TradeServiceCtxPool.Count == 0)
            {
                logger.LogWarning("No MT5 trade services configured. Trade monitoring will exit.");
                return;
            }

            // Only for testing!!!!!!
            // await ClearRelatedCache(stoppingToken);
            var roundCount = 0;
            logger.LogInformation("Trade monitoring initialized with {count} MT5 services - starting fast polling (1s intervals)", _mt5TradeServiceCtxPool.Count);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await Task.WhenAll(CheckMT5(stoppingToken));
                var enqueuedTrades = result.SelectMany(x => x).ToList();
                
                // Only log when trades are found or every 300 cycles (5 minutes) for health check
                if (enqueuedTrades.Count > 0)
                {
                    logger.LogInformation("Found {count} new trades - details: {trades}", 
                        enqueuedTrades.Count, 
                        string.Join(", ", enqueuedTrades.Select(t => $"Ticket:{t.Ticket},Account:{t.AccountNumber},Symbol:{t.Symbol}")));
                }
                else if (roundCount % 300 == 0) // Log health status every 5 minutes
                {
                    logger.LogInformation("Trade monitor health check - polling active, no trades found in last 5 minutes");
                }
                const int delaySeconds = 1; // 1 second for real-time trading requirements
                // No verbose polling logs for performance
                    
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                roundCount++;
                
                if (roundCount >= 3600 * 12)
                {
                    logger.LogInformation("Performing scheduled cache cleanup after 12 hours");
                    roundCount = 0;
                    await ClearRelatedCache(stoppingToken);
                }

                if (!stoppingToken.IsCancellationRequested) continue;
                logger.LogInformation("Trade Monitoring Service is stopping...");
                break;
            }
        }
        catch (Exception ex)
        {
            logger.LogInformation($"❌ [ERROR] Exception in ExecuteJobAsync: {ex.GetType().Name}: {ex.Message}");
            logger.LogInformation($"❌ [ERROR] Stack trace: {ex.StackTrace}");
            logger.LogError(ex, "Fatal error in TradeMonitorService");
            throw;
        }
        
        logger.LogInformation("TradeMonitorService.ExecuteJobAsync() completed");
    }

    //public static string GetMt4LastTradeTimeKey(int serviceId)
    //    => $"TradeMonitorService.MT4.LastTradeTime.SID:{serviceId}";

    //public static string GetMt4LastTicketKey(int serviceId)
    //    => $"TradeMonitorService.MT4.LastTicket.SID:{serviceId}";

    //private async Task<List<MetaTrade>> CheckMT4(CancellationToken stoppingToken)
    //{
    //    var processed = new List<MetaTrade>();
    //    foreach (var (serviceId, dbCtx) in _mt4TradeServiceCtxPool)
    //    {
    //        var timeCacheKey = GetMt4LastTradeTimeKey(serviceId);
    //        var lastTradeTimeStr = await myCache.GetStringAsync(timeCacheKey);
    //        var time = string.IsNullOrWhiteSpace(lastTradeTimeStr)
    //            ? DateTime.UtcNow.AddHours(-24)
    //            : DateTime.Parse(lastTradeTimeStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

    //        var ticketCacheKey = GetMt4LastTicketKey(serviceId);
    //        var lastTicketStr = await myCache.GetStringAsync(ticketCacheKey);
    //        var lastTicket = long.TryParse(lastTicketStr, out var ticket) ? ticket : 0;

    //        var trades = await dbCtx.Mt4Trades
    //            .Where(x => x.Cmd == 0 || x.Cmd == 1)
    //            .Where(x => x.CloseTime >= time)
    //            // .Where(x => lastTicket == 0 || x.Ticket > lastTicket)
    //            .OrderBy(x => x.CloseTime)
    //            .ToMetaTrade(0, serviceId)
    //            .ToListAsync(cancellationToken: stoppingToken);

    //        logger.LogInformation("ServiceId: {ServiceId,-5} => Got {count,3} trades in MT4", serviceId.ToString(), trades.Count);

    //        var enqueuedTrades = await EnqueueSQSModels(trades, stoppingToken);
    //        if (enqueuedTrades.Count == 0) continue;

    //        var maxTimeEnqueued = enqueuedTrades.Max(x => x.CloseAt);
    //        if (maxTimeEnqueued != null)
    //        {
    //            var maxTime = maxTimeEnqueued.Value.AddSeconds(-1);
    //            await myCache.SetStringAsync(timeCacheKey, maxTime.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
    //        }

    //        var maxTicket = enqueuedTrades.Max(x => x.Ticket);
    //        if (maxTicket != 0) await myCache.SetStringAsync(ticketCacheKey, maxTicket.ToString());

    //        processed.AddRange(enqueuedTrades);
    //    }

    //    return processed;
    //}

    public static string GetMt5LastTradeTimeKey(int serviceId)
        => $"TradeMonitorService.MT5.LastTradeTime.SID:{serviceId}";

    public static string GetMt5LastTicketKey(int serviceId)
        => $"TradeMonitorService.MT5.LastTicket.SID:{serviceId}";

    private async Task<List<MetaTrade>> CheckMT5(CancellationToken stoppingToken, HashSet<long>? prevTickets = null)
    {
        var processed = new List<MetaTrade>();
        foreach (var (serviceId, dbCtx) in _mt5TradeServiceCtxPool)
        {
            var timeCacheKey = GetMt5LastTradeTimeKey(serviceId); // TradeMonitorService.MT5.LastTradeTime.SID:30
            var lastTradeTimeStr = await myCache.GetStringAsync(timeCacheKey);
            var time = string.IsNullOrWhiteSpace(lastTradeTimeStr)
                ? DateTime.UtcNow.AddHours(-24)
                : DateTime.Parse(lastTradeTimeStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

            var ticketCacheKey = GetMt5LastTicketKey(serviceId);
            var lastTicketStr = await myCache.GetStringAsync(ticketCacheKey);
            var lastTicket = ulong.TryParse(lastTicketStr, out var ticket) ? ticket : 0;

            var trades = await dbCtx.Mt5Deals2025s
                .Where(x => x.VolumeClosed > 0)
                .Where(x => x.Action == 0 || x.Action == 1)
                .Where(x => x.Login < 82200000)
                .Where(x => x.TimeMsc > time || (x.TimeMsc == time && x.Deal > lastTicket))
                .OrderBy(x => x.TimeMsc).ThenBy(x => x.Deal)
                .ToMetaTrade(0, serviceId)
                .ToListAsync(cancellationToken: stoppingToken);

            var positions = trades.Select(x => (ulong)x.Position!).ToList();
            var openTrades = await dbCtx.Mt5Deals2025s
                .Where(x => positions.Contains(x.PositionId))
                .Where(x => x.VolumeClosed == 0)
                .Select(x => new { x.PositionId, x.TimeMsc, x.Order, x.Price })
                .ToListAsync(cancellationToken: stoppingToken);

            var validTradesInTenants = new List<MetaTrade>();
            foreach (var trade in trades)
            {
                var openTrade = openTrades.FirstOrDefault(x => x.PositionId == (ulong)trade.Position!);
                if (openTrade == null) continue;
                trade.OpenAt = openTrade.TimeMsc;
                trade.OpenPrice = openTrade.Price;
                validTradesInTenants.Add(trade);
            }

            // Only log when trades are found for this service
            if (validTradesInTenants.Count > 0)
            {
                logger.LogInformation("ServiceId: {serviceId} => Found {count} new trades in MT5", serviceId, validTradesInTenants.Count);
                logger.LogInformation("🔍 [DEBUG] ServiceId {serviceId} - Trade tickets: {tickets}", serviceId, string.Join(", ", validTradesInTenants.Select(t => t.Ticket)));
            }

            var enqueuedTrades = await EnqueueSQSModels(validTradesInTenants, stoppingToken);
            logger.LogInformation("🔍 [DEBUG] ServiceId {serviceId} - Found: {foundCount}, Enqueued: {enqueuedCount}", 
                serviceId, validTradesInTenants.Count, enqueuedTrades.Count);
                
            if (enqueuedTrades.Count == 0) 
            {
                if (validTradesInTenants.Count > 0)
                {
                    logger.LogWarning("🔍 [DEBUG] ServiceId {serviceId} - All {count} trades were skipped (likely duplicates)", serviceId, validTradesInTenants.Count);
                }
                continue;
            }

            var maxTimeEnqueued = enqueuedTrades.Max(x => x.CloseAt);
            if (maxTimeEnqueued != null)
            {
                await myCache.SetStringAsync(timeCacheKey, maxTimeEnqueued.Value.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
            }

            var maxTicket = enqueuedTrades.Max(x => x.Ticket);
            if (maxTicket != 0) await myCache.SetStringAsync(ticketCacheKey, maxTicket.ToString());
            processed.AddRange(validTradesInTenants);
        }

        return processed;
    }

    //public async Task<List<MetaTrade>> ManualEnqueueMT4Async(CancellationToken stoppingToken)
    //{
    //    await SetUpMetaTradeDbContextPool(stoppingToken);
    //    var tickets = new List<long>
    //    {
    //    };
    //    var processed = new List<MetaTrade>();
    //    foreach (var (serviceId, dbCtx) in _mt4TradeServiceCtxPool.Where(x => x.Key == 10))
    //    {
    //        var trades = await dbCtx.Mt4Trades
    //            .Where(x => x.Cmd == 0 || x.Cmd == 1)
    //            .Where(x => tickets.Contains(x.Ticket))
    //            // .Where(x => lastTicket == 0 || x.Ticket > lastTicket)
    //            .OrderBy(x => x.CloseTime)
    //            .ToMetaTrade(0, serviceId)
    //            .ToListAsync(cancellationToken: stoppingToken);

    //        logger.LogInformation("ServiceId: {ServiceId,-5} => Got {count,3} trades in MT4", serviceId.ToString(),
    //            trades.Count);

    //        var enqueuedTrades = await EnqueueSQSModels(trades, stoppingToken);
    //        processed.AddRange(enqueuedTrades);
    //    }

    //    return processed;
    //}

    private async Task<List<MetaTrade>> EnqueueSQSModels(IEnumerable<MetaTrade> metaTrades, CancellationToken stoppingToken)
    {
        var successCount = 0;
        var failCount = 0;
        var skipCount = 0;
        var enqueuedTrades = new List<MetaTrade>();
        var tasks = metaTrades.Select(async trade =>
        {
            try
            {
                var field = $"{trade.ServiceId}:{trade.Ticket}";
                var hashKey = CacheKeys.GetTradeToQueueHashKey();
                var isEnqueued = await myCache.HGetStringAsync(hashKey, field);
                if (isEnqueued != null)
                {
                    skipCount++;
                    return;
                }

                var message = JsonConvert.SerializeObject(trade, Gateway.Utils.UtcJsonSerializerSettings);
                // FIFO queues require MessageGroupId - use ServiceId as the group for ordering
                await mqService.SendAsync(message, _bcrTradeQueueName, messageGroupId: trade.ServiceId.ToString(), cancellationToken: stoppingToken);
                await myCache.HSetStringAsync(hashKey, field, "1");
                successCount++;
                enqueuedTrades.Add(trade);
            }
            catch (OperationCanceledException e)
            {
                logger.LogInformation(
                    "Operation Cancelled, Failed to send message to " +
                    "queue: {queueName}, TenantId:{tenantId}, AccountNumber: {accountNumber}, Ticket: {ticket}, CloseTime: {closeTime} " +
                    "with message: {message}"
                    , _bcrTradeQueueName
                    , trade.TenantId
                    , trade.AccountNumber
                    , trade.Ticket
                    , trade.CloseAt
                    , e.Message);
                failCount++;
            }
            catch (Exception e)
            {
                logger.LogWarning("Failed to send message to queue: BCRTrade, {message}", e.Message);
                failCount++;
            }
        });

        await Task.WhenAll(tasks);
        // Only log enqueue results when there were actual operations or failures
        if (successCount > 0 || failCount > 0)
        {
            logger.LogInformation("Enqueue results - Success: {successCount}, Skipped: {skipCount}, Failed: {failCount}",
                successCount, skipCount, failCount);
        }
        return enqueuedTrades;
    }

    private async Task SetUpMetaTradeDbContextPool(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        logger.LogInformation("Setting up MetaTrade DbContext Pool...");
        var centralDbCtx = await myDbContextPool.BorrowCentralAsync(stoppingToken);
        try
        {
            var tradeServices = await centralDbCtx.CentralTradeServices.ToListAsync(cancellationToken: stoppingToken);
            logger.LogInformation("Found {count} trade services in database", tradeServices.Count);
            
            // Only MT5 will be processed
            foreach (var tradeService in tradeServices.Where(x => x.Name.ToUpper().Contains("MT5") && !x.Description.ToUpper().Contains("DEMO")))
            {
                if (!AppEnvironment.IsProduction())
                    logger.LogInformation("Processing TradeService - ID: {id}, Name: {name}, Platform: {platform}, Configuration: {config}",
                        tradeService.Id, tradeService.Name, tradeService.Platform, tradeService.Configuration?.Substring(0, Math.Min(400, tradeService.Configuration?.Length ?? 0)) + "...");

                try
                {
                    //if (tradeService.Platform == (int)PlatformTypes.MetaTrader4)
                    //{
                    //    var mt4DbCtx = await myDbContextPool.BorrowCentralMT4Async(tradeService.Id, stoppingToken);
                    //    _mt4TradeServiceCtxPool[tradeService.Id] = mt4DbCtx;
                    //}
                    //else if (tradeService.Platform == (int)PlatformTypes.MetaTrader5)
                    //{
                    var mt5DbCtx = await myDbContextPool.BorrowCentralMT5Async(tradeService.Id, stoppingToken);
                    _mt5TradeServiceCtxPool[tradeService.Id] = mt5DbCtx;
                    logger.LogInformation("Successfully created MT5 DbContext for ServiceId: {serviceId}", tradeService.Id);
                    //}
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to create MT5 DbContext for ServiceId: {serviceId}", tradeService.Id);
                }
            }
            
            logger.LogInformation("Total MT5 DbContexts created: {count}", _mt5TradeServiceCtxPool.Count);
        }
        finally
        {
            myDbContextPool.ReturnCentral(centralDbCtx);
            logger.LogInformation("MetaTrade DbContext Pool is set up.");
        }
    }

    private async Task ClearRelatedCache(CancellationToken stoppingToken)
    {
        var mt5ServiceIds = _mt5TradeServiceCtxPool.Keys.ToList();
        //var mt4ServiceIds = _mt4TradeServiceCtxPool.Keys.ToList();
        var tasks = mt5ServiceIds.Select(async sid => { await myCache.KeyDeleteAsync(GetMt5LastTradeTimeKey(sid)); })
            .ToList();
        //tasks.AddRange(
        //    mt4ServiceIds.Select(async sid => { await myCache.KeyDeleteAsync(GetMt4LastTradeTimeKey(sid)); }));
        await Task.WhenAll(tasks);
        await myCache.HSetDeleteByKeyAsync(CacheKeys.GetTradeToQueueHashKey());
        // await _cache.HashSetDeleteByKeyAsync(RedisKeyRepository.GetAccountTenantIdHashKey());
        // await _cache.HashSetDeleteByKeyAsync(RedisKeyRepository.GetAccountNumberToAccountHashKey());
    }


    private void Dispose(bool disposing)
    {
        myDbContextPool.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}