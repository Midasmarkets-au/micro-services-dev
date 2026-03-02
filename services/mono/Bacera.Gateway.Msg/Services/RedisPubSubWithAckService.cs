using System.Collections.Concurrent;
using Bacera.LiveTrade.MyHub;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Bacera.Gateway.Msg.Services;

public class RedisPubSubWithAckService
{
    private readonly ISubscriber _subscriber;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly string _clientId = Guid.NewGuid().ToString();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _pendingAcks = new();

    public RedisPubSubWithAckService(IConnectionMultiplexer redis, IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
        _subscriber = redis.GetSubscriber();
        var ackChannel = GetAckChannelByClientId(_clientId);
        _subscriber.Subscribe(ackChannel, HandleAcknowledgment);
    }

    private void HandleAcknowledgment(RedisChannel channel, RedisValue message)
    {
        var messageId = message.ToString();
        if (_pendingAcks.TryRemove(messageId, out var tcs))
        {
            tcs.SetResult(true);
        }
    }

    public void SetupSubscriberForParty(string wsGroupName, string redisChannelName) => _subscriber.Subscribe(
        RedisChannel.Literal(redisChannelName), (channel, redisValue) =>
        {
            try
            {
                var message = JsonSerializer.Deserialize<MessageWithAck>(redisValue.ToString());

                if (message == null)
                {
                    Console.WriteLine("Received null message");
                    return;
                }

                _hubContext.Clients.Group(wsGroupName).SendAsync("ReceiveChat", message.Content).Wait();

                var senderChannel = GetAckChannelByClientId(message.SenderClientId);
                _subscriber.Publish(senderChannel, message.MessageId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        });
    
    public async Task<bool> PublishWithAckAsync(string userChannel, string content, TimeSpan timeout = default)
    {
        if (timeout == TimeSpan.Zero) timeout = TimeSpan.FromSeconds(10);

        var message = new MessageWithAck
        {
            Content = content,
            SenderClientId = _clientId
        };

        var tcs = new TaskCompletionSource<bool>();
        _pendingAcks[message.MessageId] = tcs;

        try
        {
            await _subscriber.PublishAsync(RedisChannel.Literal(userChannel), JsonConvert.SerializeObject(message));

            using var cts = new CancellationTokenSource(timeout);
            cts.Token.Register(() =>
            {
                _pendingAcks.TryRemove(message.MessageId, out _);
                tcs.TrySetResult(false);
            });

            return await tcs.Task;
        }
        catch
        {
            _pendingAcks.TryRemove(message.MessageId, out _);
            return false;
        }
    }

    private static RedisChannel GetAckChannelByClientId(string clientId) 
        => RedisChannel.Literal($"bcr_gtw_msg:ack:{clientId}");
}

public class MessageWithAck
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public string SenderClientId { get; set; } = string.Empty;
}

// public void SetupSubscriber(string userChannel, Action<string> messageHandler)
// {
//     _subscriber.Subscribe(RedisChannel.Literal(userChannel), (channel, redisValue) =>
//     {
//         try
//         {
//             var message = JsonSerializer.Deserialize<MessageWithAck>(redisValue.ToString());
//
//             if (message == null)
//             {
//                 Console.WriteLine("Received null message");
//                 return;
//             }
//
//             messageHandler(message.Content);
//
//             var senderChannel = GetAckChannelByClientId(message.SenderClientId);
//             _subscriber.Publish(senderChannel, message.MessageId);
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Error processing message: {ex.Message}");
//         }
//     });
// }