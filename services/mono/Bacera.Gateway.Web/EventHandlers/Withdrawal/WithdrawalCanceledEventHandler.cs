using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Message;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.EventHandlers;

public class WithdrawalCanceledEvent : INotification
{
    public Withdrawal Model { get; }

    public WithdrawalCanceledEvent(Withdrawal model)
    {
        Model = model;
    }
}

public class WithdrawalCanceledEventHandler : INotificationHandler<WithdrawalCanceledEvent>
{
    private readonly Tenancy _tenancy;
    private readonly ISendMessageService _sendMessageService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public WithdrawalCanceledEventHandler(
        Tenancy tenancy
        , ISendMessageService sendMessageService
        , IBackgroundJobClient backgroundJobClient)
    {
        _tenancy = tenancy;
        _backgroundJobClient = backgroundJobClient;
        _sendMessageService = sendMessageService;
    }

    public async Task Handle(WithdrawalCanceledEvent notification, CancellationToken cancellationToken)
    {
        var notice = EventNotice.Build("__WITHDRAWAL_CANCELED__", notification.Model.Id, 0,
            notification.Model.PartyId.ToString());
        await _sendMessageService.SendEventToManagerAsync(_tenancy.GetTenantId(), notice);
        _backgroundJobClient.Enqueue<IGeneralJob>(x =>
            x.WithdrawalCancelledAsync(_tenancy.GetTenantId(), notification.Model.Id));
    }
}