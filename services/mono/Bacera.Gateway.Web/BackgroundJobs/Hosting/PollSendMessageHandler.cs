using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Message;
using Bacera.Gateway.Web.Response;
using Bacera.Gateway.Web.Services;
using Bacera.Gateway.Web.Services.Interface;
using Microsoft.Extensions.Options;

namespace Bacera.Gateway.Web.BackgroundJobs.Hosting;

using M = MessageRecord;

public class PollSendMessageHandler(
    IOptions<AmazonSQSOptions> sqsOptions,
    ILogger<PollSendMessageHandler> logger,
    IMessageQueueService mqService,
    IServiceProvider serviceProvider) : IDisposable
{
    private const int DefaultMaxNumberOfMessages = 10; // thread count
    private readonly string _bcrSendMessageQueue = sqsOptions.Value.BCRSendMessage;

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        IEnumerable<Task> tasks = new List<Task>();
        var cycleCount = 0;
        logger.LogInformation("Send message queue polling started - checking every 50ms");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var messages = await mqService.ReceiveAsync(_bcrSendMessageQueue,
                    DefaultMaxNumberOfMessages, stoppingToken);

                // Only log when messages are found or every 5 minutes for health check
                if (messages.Count > 0)
                {
                    logger.LogInformation("Processing {count} messages from send-message-queue", messages.Count);
                    foreach (var message in messages)
                    {
                        await ProcessMessageAsync(message, stoppingToken);
                    }
                    logger.LogInformation("Completed processing {count} send messages", messages.Count);
                }
                else if (cycleCount % 6000 == 0) // Health check every 5 minutes (6000 cycles * 50ms = 300s)
                {
                    logger.LogInformation("Send message queue health check - polling active, no messages in last 5 minutes");
                }

                await Task.Delay(50, stoppingToken);
                cycleCount++;
            }
            catch (OperationCanceledException e)
            {
                await Task.WhenAll(tasks);
                logger.LogInformation("Send message queue polling cancelled: {message}", e.Message);
            }
            catch (Exception e)
            {
                logger.LogError("Error receiving messages from queue, {message}", e.Message);
            }
        }

        logger.LogInformation("PollSendMessageHandler is stopping.");
    }


    private async Task ProcessMessageAsync(MQMessage message, CancellationToken cancellationToken)
    {
        if (SendMessageMqDTO.TryParse(message.Body, out var mqDTO))
        {
            if (mqDTO.Category == SendMessageMqCategoryTypes.BatchEmail &&
                SendBatchEmailDTO.TryParse(mqDTO.Data, out var batchEmailDTO))
            {
                var scope = GetScopeByTenantIdAsync(batchEmailDTO.TenantId);
                var batchEmailSvc = scope.ServiceProvider.GetRequiredService<BatchSendEmailService>();
                var (result, msg) = await batchEmailSvc.SendEmailByTopicIdWithContent(batchEmailDTO);
                if (!result)
                {
                    BcrLog.Slack($"PollSendMessageHandler_error sending batch email: {msg}");
                    // return to avoid deleting the message, so that it can be retried
                    return;
                }

            }
            if (mqDTO.Category == SendMessageMqCategoryTypes.GeneralEmail &&
                M.MQSource.TryParse(mqDTO.Data, out var mqSource))
            {
                var scope = GetScopeByTenantIdAsync(mqSource.TenantId);
                var messageSvc = scope.ServiceProvider.GetRequiredService<MessageService>();
                if (mqSource.Method == "email")
                {
                    if (M.EmailSenderOptions.TryParse(mqSource.Options, out var senderOptions))
                    {
                        var (result, msg) =
                            await messageSvc.SendEmailByIdAsync(mqSource.MessageRecordId, senderOptions);
                        if (!result) BcrLog.Slack($"PollSendMessageHandler_error sending email: {msg}");
                    }
                }

                if (mqSource.Method == "notification")
                {
                    // TOBE IMPLEMENTED
                }

                //...
            }
        }

        await mqService.DeleteAsync(_bcrSendMessageQueue, message, cancellationToken);
    }

    private readonly Dictionary<long, IServiceScope> _scopes = new();

    private IServiceScope GetScopeByTenantIdAsync(long tenantId)
    {
        if (_scopes.TryGetValue(tenantId, out var scope))
        {
            var ctx = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
            ctx.ChangeTracker.Clear();
            return scope;
        }

        // var tenant = await _centralDbContext.Tenants.SingleAsync(x => x.Id == tenantId);
        _scopes[tenantId] = serviceProvider.CreateScope();
        _scopes[tenantId].ServiceProvider.GetRequiredService<Tenancy>().SetTenantId(tenantId);
        return _scopes[tenantId];
    }

    public void Dispose()
    {
        _scopes.Values.ToList().ForEach(x => x.Dispose());
    }
}