using Bacera.Gateway.Core.Types;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace Bacera.Gateway.Web.Services;

/// <summary>
/// Publishes messages to NATS JetStream streams.
/// BCR_EVENT_TRADE: EventShopPointTransaction events (replaces SQS BCREventTrade.fifo).
///   Consumed by: scheduler/src/jobs/event_trade_handler.rs
/// BCR_SEND_MESSAGE: Batch email fan-out messages (replaces SQS BCRSendMessage).
///   Consumed by: scheduler/src/jobs/send_message_handler.rs
/// </summary>
public class NatsPublisher : IAsyncDisposable
{
    private const string StreamName = "BCR_EVENT_TRADE";
    private const string Subject = "trade.event";

    private const string SendMessageStreamName = "BCR_SEND_MESSAGE";
    private const string SendMessageSubject = "send.message";

    private readonly NatsJSContext _js;
    private readonly NatsConnection _nats;
    private readonly ILogger<NatsPublisher> _logger;

    public NatsPublisher(string natsUrl, ILogger<NatsPublisher> logger)
    {
        _logger = logger;
        var opts = new NatsOpts { Url = natsUrl };
        _nats = new NatsConnection(opts);
        _js = new NatsJSContext(_nats);
    }

    /// <summary>
    /// Publishes an MQSource message to BCR_EVENT_TRADE.
    /// sourceType: 1=OpenAccount, 2=Trade, 3=Deposit
    /// </summary>
    public async Task PublishAsync(
        EventShopPointTransactionSourceTypes sourceType,
        long rowId,
        long tenantId,
        CancellationToken cancellationToken = default)
    {
        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            SourceType = (int)sourceType,
            RowId = rowId,
            TenantId = tenantId,
        });

        try
        {
            await _js.PublishAsync(Subject, payload, cancellationToken: cancellationToken);
            _logger.LogInformation(
                "NatsPublisher: published SourceType={SourceType} RowId={RowId} TenantId={TenantId}",
                sourceType, rowId, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "NatsPublisher: failed to publish SourceType={SourceType} RowId={RowId} TenantId={TenantId}",
                sourceType, rowId, tenantId);
            throw;
        }
    }

    /// <summary>
    /// Publishes a SendMessageMqDTO to BCR_SEND_MESSAGE (batch email fan-out).
    /// Replaces the legacy SQS BCRSendMessage publish path.
    /// Consumed by: scheduler/src/jobs/send_message_handler.rs
    /// </summary>
    public async Task PublishSendMessageAsync(
        SendMessageMqDTO dto,
        CancellationToken cancellationToken = default)
    {
        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            Category = (int)dto.Category,
            Data = dto.Data,
        });

        try
        {
            await _js.PublishAsync(SendMessageSubject, payload, cancellationToken: cancellationToken);
            _logger.LogInformation(
                "NatsPublisher: published SendMessage Category={Category}",
                dto.Category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "NatsPublisher: failed to publish SendMessage Category={Category}",
                dto.Category);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _nats.DisposeAsync();
    }
}
