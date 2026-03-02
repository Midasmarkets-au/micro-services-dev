using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;

namespace Bacera.Gateway.Web.EventHandlers.Event;

public class UserEventShopOrderPlacedEvent(long eventShopOrderId) : INotification
{
    public long EventShopOrderId { get; } = eventShopOrderId;
}

public class UserEventShopOrderPlacedHandler(Tenancy tenancy, IBackgroundJobClient client) : INotificationHandler<UserEventShopOrderPlacedEvent>
{
    public async Task Handle(UserEventShopOrderPlacedEvent notification, CancellationToken cancellationToken)
    {
        await Task.Delay(0, cancellationToken);
        client.Enqueue<IGeneralJob>(x => x.UserEventShopOrderPlaced(tenancy.GetTenantId(), notification.EventShopOrderId));
    }
}