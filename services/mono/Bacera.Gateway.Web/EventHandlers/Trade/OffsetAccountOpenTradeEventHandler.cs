using Bacera.Gateway.Interfaces;
using MediatR;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.EventHandlers.Trade;

public class OffsetAccountOpenTradeEvent : INotification
{
    public TradeViewModel Trade { get; }

    public OffsetAccountOpenTradeEvent(TradeViewModel trade)
    {
        Trade = trade;
    }
}

public class OffsetAccountOpenTradeEventHandler : INotificationHandler<OffsetAccountOpenTradeEvent>
{
    private readonly Tenancy _tenancy;
    private readonly ISendMessageService _sendMessageService;

    public OffsetAccountOpenTradeEventHandler(
        Tenancy tenancy
        , ISendMessageService sendMessageService
    )
    {
        _tenancy = tenancy;
        _sendMessageService = sendMessageService;
    }

    public async Task Handle(OffsetAccountOpenTradeEvent notification, CancellationToken cancellationToken)
    {
        await _sendMessageService.SendEventToManagerAsync(_tenancy.GetTenantId(),
            EventNotice.Build("__OFFSET_ACCOUNT_TRADE_OPENED__", notification.Trade.Id,
                message: JsonConvert.SerializeObject(notification.Trade)));
    }
}