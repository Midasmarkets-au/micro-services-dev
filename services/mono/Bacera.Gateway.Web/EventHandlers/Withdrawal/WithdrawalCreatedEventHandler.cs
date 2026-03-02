using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.EventHandlers;

public class WithdrawalCreatedEvent(Withdrawal model) : INotification
{
    public Withdrawal Model { get; } = model;
}

public class WithdrawalCreatedEventHandler(
    Tenancy tenancy,
    ISendMessageService sendMessageService,
    IBackgroundJobClient backgroundJobClient)
    : INotificationHandler<WithdrawalCreatedEvent>
{
    public async Task Handle(WithdrawalCreatedEvent notification, CancellationToken cancellationToken)
    {
        var withdrawal = notification.Model;
        var notice = EventNotice.Build("__WITHDRAWAL_CREATED__"
            , notification.Model.Id
            , 0
            , notification.Model.PartyId.ToString());
        await sendMessageService.SendEventToManagerAsync(tenancy.GetTenantId(), notice);

        backgroundJobClient.Enqueue<IGeneralJob>(x => x.WithdrawalCreatedAsync(tenancy.GetTenantId(), withdrawal.Id));
    }
}