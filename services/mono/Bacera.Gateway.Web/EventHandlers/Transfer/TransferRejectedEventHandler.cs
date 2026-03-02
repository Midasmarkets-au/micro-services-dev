using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;

namespace Bacera.Gateway.Web.EventHandlers;

public class TransferRejectedEvent : INotification
{
    public Transaction Model { get; }

    public TransferRejectedEvent(Transaction model)
    {
        Model = model;
    }
}

public class TransferRejectedEventHandler : INotificationHandler<TransferRejectedEvent>
{
    private readonly Tenancy _tenancy;
    private readonly ISendMessageService _sendMessageService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public TransferRejectedEventHandler(
        Tenancy tenancy
        , ISendMessageService sendMessageService, IBackgroundJobClient backgroundJobClient)
    {
        _tenancy = tenancy;
        _sendMessageService = sendMessageService;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task Handle(TransferRejectedEvent notification, CancellationToken cancellationToken)
    {
        var transaction = notification.Model;
        var notice = EventNotice.Build("__TRANSFER_REJECTED__", notification.Model.Id, 0,
            notification.Model.ReferenceNumber);
        await _sendMessageService.SendEventToManagerAsync(_tenancy.GetTenantId(), notice);

        if (notification.Model.SourceAccountType != (int)TransactionAccountTypes.Account ||
            notification.Model.TargetAccountType != (int)TransactionAccountTypes.Account)
            return;

        _backgroundJobClient.Enqueue<IGeneralJob>(x =>
            x.TransactionBetweenTradeAccountFailedAsync(_tenancy.GetTenantId(), transaction.Id));
    }
}