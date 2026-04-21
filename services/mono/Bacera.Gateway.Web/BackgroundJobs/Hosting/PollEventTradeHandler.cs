// [MIGRATED] PollEventTradeHandler — BCREventTrade SQS consumer fully migrated to NATS JetStream.
//
// This class previously polled the AWS SQS BCREventTrade.fifo queue and dispatched messages to
// EventService.ProcessTradeSourceAsync / ProcessOpenAccountSourceAsync / ProcessDepositSourceAsync.
//
// Migration summary:
//   - Publishers replaced: AccountCreatedEventHandler and GeneralJob.DepositCompletedAsync now
//     publish to NATS BCR_EVENT_TRADE stream via NatsPublisher (source_type=1 and source_type=3).
//   - Trade publisher: scheduler/src/jobs/trade_handler.rs publishes to BCR_EVENT_TRADE
//     (source_type=2) after inserting a TradeRebate record.
//   - Consumer: scheduler/src/jobs/event_trade_handler.rs handles all source types
//     (OpenAccount=1, Trade=2, Deposit=3) from BCR_EVENT_TRADE via NATS durable pull consumer.
//   - The SQS queue BCREventTrade.fifo is no longer consumed or published to.
//
// This file is kept for reference and can be deleted once the migration is confirmed stable.

// using Amazon.SQS;
// using Amazon.SQS.Model;
// using Bacera.Gateway.Context;
// using Bacera.Gateway.Core.Types;
// using Bacera.Gateway.Services;
// using Bacera.Gateway.Web.Response;
// using Bacera.Gateway.Web.Services;
// using Bacera.Gateway.Web.Services.Interface;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Options;
// using Newtonsoft.Json;
//
// namespace Bacera.Gateway.Web.BackgroundJobs.Hosting;
//
// using M = EventShopPointTransaction;
//
// public class PollEventTradeHandler(
//     IOptions<AmazonSQSOptions> sqsOptions,
//     ILogger<PollEventTradeHandler> logger,
//     MyDbContextPool myDbContextPool,
//     IMyCache myCache,
//     IMessageQueueService mqService,
//     IServiceProvider serviceProvider)
//     : IDisposable
// {
//     private const int DefaultMaxNumberOfMessages = 10; // thread count
//     private const long EventId = 1;
//     private readonly string _bcrEventTradeQueue = sqsOptions.Value.BCREventTrade;
//
//     public async Task PollEventTradeAsync(CancellationToken stoppingToken)
//     {
//         // return;
//         IEnumerable<Task> tasks = new List<Task>();
//         var cycleCount = 0;
//         logger.LogInformation("Event trade queue polling started - checking every 5 seconds");
//
//         while (!stoppingToken.IsCancellationRequested)
//         {
//             try
//             {
//                 var messages = await mqService.ReceiveAsync(_bcrEventTradeQueue, DefaultMaxNumberOfMessages, stoppingToken);
//
//                 // Only log when messages are found or every 5 minutes for health check
//                 if (messages.Count > 0)
//                 {
//                     logger.LogInformation("Polling messages from queue, {message}...", messages.Count);
//                     foreach (var message in messages)
//                     {
//                         await ProcessMessageAsync(message, stoppingToken);
//                     }
//                     logger.LogInformation("Completed processing {count} event trade messages", messages.Count);
//                 }
//                 else if (cycleCount % 60 == 0) // Health check every 5 minutes (60 cycles * 5s = 300s)
//                 {
//                     logger.LogInformation("Event trade queue health check - polling active, no messages in last 5 minutes");
//                 }
//
//                 await Task.Delay(5000, stoppingToken);
//                 cycleCount++;
//             }
//             catch (OperationCanceledException e)
//             {
//                 await Task.WhenAll(tasks);
//                 logger.LogInformation("Task cancelled when receiving messages from queue, {message}", e.Message);
//             }
//             catch (Exception e)
//             {
//                 logger.LogError("Error receiving messages from queue, {message}", e.Message);
//             }
//         }
//     }
//
//     public async Task ProcessMessageAsync(MQMessage message, CancellationToken cancellationToken)
//     {
//         logger.LogInformation($"🔍 [DEBUG] ProcessMessageAsync started with message: {message.Body}");
//
//         if (!M.MQSource.TryParse(message.Body, out var mqSource))
//         {
//             logger.LogInformation("Invalid message, invalid message: {message}", message.Body);
//             await mqService.DeleteAsync(_bcrEventTradeQueue, message, cancellationToken);
//             return;
//         }
//
//         logger.LogInformation($"✅ [DEBUG] Parsed message - SourceType: {mqSource.SourceType}, RowId: {mqSource.RowId}, TenantId: {mqSource.TenantId}");
//
//         var value = await myCache.GetStringAsync(mqSource.ToRedisKey());
//         if (value != null)
//         {
//             logger.LogInformation("Duplicated message, {message}", message.Body);
//             await mqService.DeleteAsync(_bcrEventTradeQueue, message, cancellationToken);
//             return;
//         }
//
//         await myCache.SetStringAsync(mqSource.ToRedisKey(), "1", TimeSpan.FromHours(2));
//         var scope = GetScopeByTenantIdAsync(mqSource.TenantId);
//         var svc = scope.ServiceProvider.GetRequiredService<EventService>();
//         var result = false;
//
//         logger.LogInformation($"🔄 [DEBUG] Processing message with EventService - EventId: {EventId}, SourceType: {mqSource.SourceType}, RowId: {mqSource.RowId}");
//         logger.LogInformation("Processing message, {message}", message.Body);
//
//         try
//         {
//             result = mqSource.SourceType switch
//             {
//                 EventShopPointTransactionSourceTypes.Trade => await svc.ProcessTradeSourceAsync(EventId, mqSource.RowId),
//                 EventShopPointTransactionSourceTypes.OpenAccount => await svc.ProcessOpenAccountSourceAsync(EventId, mqSource.RowId),
//                 EventShopPointTransactionSourceTypes.Deposit => await svc.ProcessDepositSourceAsync(EventId, mqSource.RowId),
//                 EventShopPointTransactionSourceTypes.Adjust => await svc.ProcessAdjustSourceAsync(EventId, mqSource.RowId),
//                 _ => false
//             };
//             logger.LogInformation($"📊 [DEBUG] EventService processing result: {result}");
//         }
//         catch (Exception e)
//         {
//             logger.LogError("ProcessMessageAsync_Error_processing_message, {message}", e.Message);
//             logger.LogError($"❌ [DEBUG] Exception stack trace: {e.StackTrace}");
//             BcrLog.Slack($"ProcessMessageAsync_Error_processing_message: {e}");
//         }
//
//         if (!result)
//         {
//             logger.LogInformation("ProcessMessageAsync_Process_message_failed: {message}", message.Body);
//             return;
//         }
//
//         logger.LogInformation($"✅ [DEBUG] Processing successful - deleting message from queue");
//         await mqService.DeleteAsync(_bcrEventTradeQueue, message, cancellationToken);
//         // _logger.LogInformation("Processing message, MessageId: {message}", message.ReceiptHandle);
//     }
//
//     private readonly Dictionary<long, IServiceScope> _scopes = new();
//
//     private IServiceScope GetScopeByTenantIdAsync(long tenantId)
//     {
//         if (_scopes.TryGetValue(tenantId, out var scope))
//         {
//             var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
//             ctx.ChangeTracker.Clear();
//             return scope;
//         }
//
//         // var tenant = await _centralDbContext.Tenants.SingleAsync(x => x.Id == tenantId);
//         _scopes[tenantId] = serviceProvider.CreateScope();
//         _scopes[tenantId].ServiceProvider.GetRequiredService<Tenancy>().SetTenantId(tenantId);
//         return _scopes[tenantId];
//     }
//
//     public void Dispose()
//     {
//         myDbContextPool.Dispose();
//         _scopes.Values.ToList().ForEach(x => x.Dispose());
//     }
// }
