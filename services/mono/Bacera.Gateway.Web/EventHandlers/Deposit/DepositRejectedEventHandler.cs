using Bacera.Gateway.Interfaces;
using MediatR;

namespace Bacera.Gateway.Web.EventHandlers;

public class DepositRejectedEvent : INotification
{
    public Deposit Model { get; }

    public DepositRejectedEvent(Deposit model)
    {
        Model = model;
    }
}

public class DepositRejectedEventHandler : INotificationHandler<DepositRejectedEvent>
{
    private readonly Tenancy _tenancy;
    private readonly ISendMessageService _sendMessageService;

    public DepositRejectedEventHandler(
        Tenancy tenancy
        , ISendMessageService sendMessageService
    )
    {
        _tenancy = tenancy;
        _sendMessageService = sendMessageService;
    }

    public async Task Handle(DepositRejectedEvent notification, CancellationToken cancellationToken)
    {
        var notice = EventNotice.Build("__DEPOSIT_REJECTED__", notification.Model.Id, notification.Model.Type);
        await _sendMessageService.SendEventToManagerAsync(_tenancy.GetTenantId(), notice);
    }
}