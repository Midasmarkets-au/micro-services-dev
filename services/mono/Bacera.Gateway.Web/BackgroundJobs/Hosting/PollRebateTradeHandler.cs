using Amazon.SQS;
using Amazon.SQS.Model;
using Bacera.Gateway.Context;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs.Hosting.Utils;
using Bacera.Gateway.Web.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.BackgroundJobs.Hosting;

// [MIGRATED] PollRebateTradeHandler — BCRSalesRebateTrade queue consumer, never implemented.
// Pipeline replaced by scheduler NATS JetStream. This class is kept for reference only.
using SQSMessage = Amazon.SQS.Model.Message;

public class PollRebateTradeHandler
{
    private readonly ILogger<PollRebateTradeHandler> _logger;

    private readonly IMyCache _myCache;
    private readonly MyDbContextPool _myDbContextPool;
    private readonly IMessageQueueService _mqService;
    private const int DefaultMaxNumberOfMessages = 10; // thread count
    private const int DelayTime = 5000; // thread count

    public PollRebateTradeHandler(IOptions<AmazonSQSOptions> sqsOptions,
        ILogger<PollRebateTradeHandler> logger,
        MyDbContextPool myDbContextPool, IMyCache myCache, IMessageQueueService mqService)
    {
        _logger = logger;
        _myDbContextPool = myDbContextPool;
        _myCache = myCache;
        _mqService = mqService;
    }

    public async Task PollRebateTradeAsync(CancellationToken stoppingToken)
    {
        IEnumerable<Task<bool>> processMessageTasks = new List<Task<bool>>();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // var messages = await ReceiveMessageAsync(stoppingToken);
                // _logger.LogInformation("Received {count} messages from queue", messages.Count);
                // processMessageTasks = messages.Select(x => ProcessMessageAsync(x, stoppingToken)).ToList();
                // var results = await Task.WhenAll(processMessageTasks);
                // _logger.LogInformation("Processed {count} messages from queue", results.Count(x => x));
                _logger.LogInformation("Polling messages from queue... Delaying for {delayTime} ms...", DelayTime);
                await Task.Delay(DelayTime, stoppingToken); // 5-second delay
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

    private async Task<bool> ProcessMessageAsync(SQSMessage message, CancellationToken cancellationToken)
    {
        var ctx = await _myDbContextPool.BorrowCentralAsync(cancellationToken);
        try
        {
            return true;
        }
        finally
        {
            _myDbContextPool.ReturnCentral(ctx);
        }
    }
}