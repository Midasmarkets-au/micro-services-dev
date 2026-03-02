using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;

namespace Bacera.Gateway.Web.EventHandlers;

public class TransferCanceledEvent : INotification
{
    public Transaction Model { get; }

    public TransferCanceledEvent(Transaction model)
    {
        Model = model;
    }
}

public class TransferCanceledEventHandler : INotificationHandler<TransferCanceledEvent>
{
    private readonly Tenancy _tenancy;
    private readonly ISendMessageService _sendMessageService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public TransferCanceledEventHandler(
        Tenancy tenancy
        , ISendMessageService sendMessageService, IBackgroundJobClient backgroundJobClient)
    {
        _tenancy = tenancy;
        _sendMessageService = sendMessageService;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task Handle(TransferCanceledEvent notification, CancellationToken cancellationToken)
    {
        var transaction = notification.Model;
        var notice = EventNotice.Build("__TRANSFER_CANCELED__", notification.Model.Id, 0,
            notification.Model.ReferenceNumber);
        await _sendMessageService.SendEventToManagerAsync(_tenancy.GetTenantId(), notice);

        if (transaction.SourceAccountType != (int)TransactionAccountTypes.Account ||
            transaction.TargetAccountType != (int)TransactionAccountTypes.Account)
            return;

        _backgroundJobClient.Enqueue<IGeneralJob>(x =>
            x.TransactionBetweenTradeAccountFailedAsync(_tenancy.GetTenantId(), transaction.Id));
    }
}