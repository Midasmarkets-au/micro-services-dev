using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ISendMessageService = Bacera.Gateway.Interfaces.ISendMessageService;

namespace Bacera.Gateway.Web.EventHandlers;

public class WithdrawalRejectedEvent : INotification
{
    public Withdrawal Model { get; }

    public WithdrawalRejectedEvent(Withdrawal model)
    {
        Model = model;
    }
}

public class WithdrawalRejectedEventHandler : INotificationHandler<WithdrawalRejectedEvent>
{
    private readonly Tenancy _tenancy;
    private readonly ISendMessageService _sendMessageService;
    private readonly IBackgroundJobClient _backgroundJobClient;


    public WithdrawalRejectedEventHandler(
        Tenancy tenancy,
        ISendMessageService sendMessageService,
        IBackgroundJobClient backgroundJobClient)
    {
        _tenancy = tenancy;
        _sendMessageService = sendMessageService;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task Handle(WithdrawalRejectedEvent notification, CancellationToken cancellationToken)
    {
        var notice = EventNotice.Build("__WITHDRAWAL_REJECTED__", notification.Model.Id, 0,
            notification.Model.PartyId.ToString());
        await _sendMessageService.SendEventToManagerAsync(_tenancy.GetTenantId(), notice);
        _backgroundJobClient.Enqueue<IGeneralJob>(x =>
            x.WithdrawalRejectedAsync(_tenancy.GetTenantId(), notification.Model.Id));
    }
}