using Bacera.Gateway.Interfaces;
using MediatR;

namespace Bacera.Gateway.Web.EventHandlers;

public class DepositCanceledEvent(long depositId) : INotification
{
    public long DepositId { get; set; } = depositId;
}

public class DepositCanceledEventHandler(Tenancy tenancy, ISendMessageService sendMessageService) : INotificationHandler<DepositCanceledEvent>
{
    public async Task Handle(DepositCanceledEvent notification, CancellationToken cancellationToken)
    {
        var notice = EventNotice.Build("__DEPOSIT_CANCELED__", notification.DepositId);
        await sendMessageService.SendEventToManagerAsync(tenancy.GetTenantId(), notice);
    }
}