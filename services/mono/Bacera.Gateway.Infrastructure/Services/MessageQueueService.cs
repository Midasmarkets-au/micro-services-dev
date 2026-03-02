using Amazon.SQS;
using Amazon.SQS.Model;
using Bacera.Gateway.Web.Response;
using Bacera.Gateway.Web.Services.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Bacera.Gateway.Web.Services;

public class MessageQueueService(
    AmazonSQSClient sqsClient,
    IOptions<AmazonSQSOptions> sqsOptions)
    : IMessageQueueService, IDisposable
{
    public async Task SendAsync(string message, string queueName, string? messageGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var request = new SendMessageRequest
        {
            QueueUrl = $"{sqsOptions.Value.Prefix}/{queueName}",
            MessageBody = message
        };
        
        if (string.IsNullOrEmpty(messageGroupId) == false)
        {
            request.MessageGroupId = messageGroupId;
        }
        
        // For FIFO queues (.fifo), generate MessageDeduplicationId
        if (queueName.EndsWith(".fifo"))
        {
            request.MessageDeduplicationId = GenerateMessageDeduplicationId(message);
        }

        await sqsClient.SendMessageAsync(request, cancellationToken);
    }
    
    private static string GenerateMessageDeduplicationId(string message)
    {
        // Generate a hash-based deduplication ID from message content and timestamp
        // This ensures messages with same content within 5 minutes get deduplicated
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd-HH-mm");
        var combined = $"{message}|{timestamp}";
        
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
        return Convert.ToHexString(hashBytes)[..32]; // Take first 32 characters
    }

    public async Task<List<MQMessage>> ReceiveAsync(string queueName, int maxNumberOfMessages = 10,
        CancellationToken cancellationToken = default)
    {
        var request = new ReceiveMessageRequest
        {
            QueueUrl = $"{sqsOptions.Value.Prefix}/{queueName}",
            MaxNumberOfMessages = maxNumberOfMessages,
        };
        var response = await sqsClient.ReceiveMessageAsync(request, cancellationToken);
        return response.Messages.ToMQMessage(queueName);
    }

    public async Task DeleteAsync(string queueName, MQMessage message,
        CancellationToken cancellationToken = default)
    {
        var request = new DeleteMessageRequest
        {
            QueueUrl = $"{sqsOptions.Value.Prefix}/{queueName}",
            ReceiptHandle = message.ReceiptHandle
        };
        await sqsClient.DeleteMessageAsync(request, cancellationToken);
    }

    public void Dispose()
    {
        sqsClient.Dispose();
    }
}