using Bacera.Gateway.Core.Types;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace Bacera.Gateway.Web.Services;

/// <summary>
/// Publishes EventShopPointTransaction MQSource messages to NATS JetStream BCR_EVENT_TRADE stream.
/// Replaces the legacy SQS BCREventTrade.fifo publish path.
/// Consumed by: scheduler/src/jobs/event_trade_handler.rs
/// </summary>
public class NatsPublisher : IAsyncDisposable
{
    private const string StreamName = "BCR_EVENT_TRADE";
    private const string Subject = "trade.event";

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

    public async ValueTask DisposeAsync()
    {
        await _nats.DisposeAsync();
    }
}
