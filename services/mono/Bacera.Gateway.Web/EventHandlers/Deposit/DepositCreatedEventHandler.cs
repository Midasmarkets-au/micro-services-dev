using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;

namespace Bacera.Gateway.Web.EventHandlers;

public class DepositCreatedEvent : INotification
{
    public Deposit Model { get; }

    public DepositCreatedEvent(Deposit model)
    {
        Model = model;
    }
}

public class DepositCreatedEventHandler(Tenancy tenancy, ISendMessageService sendMessageService, IBackgroundJobClient backgroundJobClient)
    : INotificationHandler<DepositCreatedEvent>
{
    public async Task Handle(DepositCreatedEvent notification, CancellationToken cancellationToken)
    {
        await sendMessageService.SendEventToManagerAsync(tenancy.GetTenantId(), EventNotice.Build("__DEPOSIT_CREATED__", notification.Model.Id));
        if (notification.Model.TargetAccountId == null) return;

        backgroundJobClient.Enqueue<IGeneralJob>(x => x.DepositCreatedAsync(tenancy.GetTenantId(), notification.Model.Id));
    }
}