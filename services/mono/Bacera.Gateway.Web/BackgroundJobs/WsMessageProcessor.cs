using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.Services;
using Bacera.Gateway.Web.WebSocket;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.BackgroundJobs;

public class WsMessageProcessor
{
    private readonly Channel<string> _channel;
    private readonly Task _consumerTask;
    private readonly IServiceProvider _provider;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ILogger<WsMessageProcessor> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private int _retryCount = 0;

    public WsMessageProcessor(IServiceProvider provider, ILogger<WsMessageProcessor> logger,
        IHttpClientFactory httpClientFactory, CancellationToken cancellationToken = default)
    {
        // BcrLog.Slack($"Starting_WsMessageProcessor_on[{Environment.MachineName}]");
        _provider = provider;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false 
        });

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _consumerTask = Task.Run(() => ConsumerAsync(_cancellationTokenSource.Token), cancellationToken);
    }

    public async Task ConsumerAsync(CancellationToken cancellationToken)
    {
        if (_retryCount >= 20)
        {
            BcrLog.Slack($"Failed_to_connect_to_websocket_after_20_retries,_exiting");
            return;
        }
        
        ClientWebSocket? clientWebSocket;
        int retryCount = 0;
        do
        {
            clientWebSocket = await GetSocketClient();
            if (clientWebSocket == null)
            {
                await Task.Delay(1000, cancellationToken);
                BcrLog.Slack($" Failed to connect to websocket, retrying in 1 second");
                retryCount++;
            }

            if (retryCount > 15)
            {
                BcrLog.Slack($"Failed to connect to websocket after 10 retries, exiting");
                return;
            }
        } while (clientWebSocket == null);

        try
        {
            // BcrLog.Slack($"Starting_consumer_task");
            await foreach (var message in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                // BcrLog.Slack($"Got_message_from_channel: {message}");
                var buffer = Encoding.UTF8.GetBytes(message);
                await clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _retryCount++;
            BcrLog.Slack($"ConsumerAsync_error: {ex.Message}");
            await ConsumerAsync(cancellationToken);
        }
        finally
        {
            await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
            clientWebSocket.Dispose();
        }
    }

    public void AddMessage(WsSendDTO wsDto)
    {
        // BcrLog.Slack($"Adding_message_to_channel.wsDto: {wsDto.ToJson()}");
        if (!_channel.Writer.TryWrite(wsDto.ToWsMessage()))
        {
            BcrLog.Slack($"Failed_to_write_message_to_channel.wsDto: {wsDto}");
            _logger.LogWarning("Failed_to_write_message_to_channel.wsDto: {wsDto}", wsDto);
        }
    }

    public async Task StopAsync()
    {
        await _cancellationTokenSource.CancelAsync();
        _channel.Writer.Complete();
        await _consumerTask;
    }

    // private const string TenantApiHost = "https://pro.portal.mybcr.dev";
    private const string TenantApiHost = "https://api.au.thebcr.com";
    // private const string TenantApiHost = "http://localhost:5000";

    private async Task<ClientWebSocket?> GetSocketClient()
    {
        // using var httpClient = new HttpClient();
        using var scope = _provider.CreateScope();

        var tokenPayload = new Dictionary<string, string>
        {
            { "client_id", "api" },
            { "scope", "api" },
            { "grant_type", "password" },
            { "tenantId", "1" },
            { "username", Environment.GetEnvironmentVariable("CRM_SYSTEM_EMAIL")! },
            { "password", Environment.GetEnvironmentVariable("CRM_SYSTEM_PASSWORD")! },
        };

        var client = _httpClientFactory.CreateClient();
        var tokenResponse =
            await client.PostAsync($"{TenantApiHost}/connect/token", new FormUrlEncodedContent(tokenPayload));
        if (!tokenResponse.IsSuccessStatusCode) return null;

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        var tokenJson = JsonConvert.DeserializeObject<dynamic>(tokenContent)!;

        string accessToken = tokenJson.access_token;
        const string url = $"{TenantApiHost}/hub/client/negotiate?negotiateVersion=1";
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.PostAsync(url, new StringContent(""));
        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        var negotiateResponse = JsonConvert.DeserializeObject<dynamic>(content);
        if (negotiateResponse == null) return null;

        string connectionToken = negotiateResponse.connectionToken;

        var wsUrl = $"{TenantApiHost.Replace("https://", "wss://")}/hub/client?id={connectionToken}&access_token={accessToken}";
        // var wsUrl = $"{TenantApiHost.Replace("http://", "ws://")}/hub/client?id={connectionToken}&access_token={accessToken}";
        var clientWebSocket = new ClientWebSocket();
        await clientWebSocket.ConnectAsync(new Uri(wsUrl), _cancellationTokenSource.Token);
        // {"protocol":"json","version":1}

        const string protocol = """
                                {"protocol":"json","version":1}
                                """;

        var buffer = Encoding.UTF8.GetBytes(protocol);
        await clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);


        // var operatorPartyGroup = NotificationHub.GetPartyGroupName(1, 103);
        // var messagePopup = MessagePopupDTO.BuildInfo("Report Process Finished", "111");
        // // wsMessageProcessor.AddMessage(WsSendDTO.Build("SendMessageToGroup", [operatorPartyGroup, messagePopup.ToJson()]));
        // var dto = WsSendDTO.Build("SendMessageToGroup", [operatorPartyGroup, "ReceivePopup", messagePopup.ToJson()]);
        // Console.WriteLine($"Got_message_from_channel: {dto.ToJson()}");
        //
        // var msg = dto.ToWsMessage();
        // await clientWebSocket.SendAsync(dto.ToBytes(), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
        return clientWebSocket;
    }
}