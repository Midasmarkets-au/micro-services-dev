using Bacera.Gateway.Web.Response;

namespace Bacera.Gateway.Web.Services.Interface;

public interface IMessageQueueService
{
    Task SendAsync(string message, string queueName, string? messageGroupId = null,
        CancellationToken cancellationToken = default);

    Task<List<MQMessage>> ReceiveAsync(string queueName, int maxNumberOfMessages = 10,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string queueName, MQMessage receiptHandle, CancellationToken cancellationToken = default);
}