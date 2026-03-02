using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;

namespace Bacera.Gateway.Web.EventHandlers;

public class WithdrawalCompletedEvent : INotification
{
    public Withdrawal Model { get; }

    public WithdrawalCompletedEvent(Withdrawal model)
    {
        Model = model;
    }
}

public class WithdrawalCompletedEventHandler : INotificationHandler<WithdrawalCompletedEvent>
{
    private readonly Tenancy _tenancy;
    private readonly ISendMessageService _sendMessageService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public WithdrawalCompletedEventHandler(
        Tenancy tenancy
        , ISendMessageService sendMessageService
        , IBackgroundJobClient backgroundJobClient)
    {
        _tenancy = tenancy;
        _backgroundJobClient = backgroundJobClient;
        _sendMessageService = sendMessageService;
    }

    public async Task Handle(WithdrawalCompletedEvent notification, CancellationToken cancellationToken)
    {
        var tenantId = _tenancy.GetTenantId();
        var notice = EventNotice.Build("__WITHDRAWAL_COMPLETED__", notification.Model.Id, 0,
            notification.Model.PartyId.ToString());
        await _sendMessageService.SendEventToManagerAsync(tenantId, notice);
        _backgroundJobClient.Enqueue<IGeneralJob>(x =>
            x.WithdrawalCompletedAsync(_tenancy.GetTenantId(), notification.Model.Id));
    }
}