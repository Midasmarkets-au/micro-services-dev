using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;

namespace Bacera.Gateway.Web.EventHandlers;

public class TransferCompletedEvent(Transaction model) : INotification
{
    public Transaction Model { get; } = model;
}

public class TransferCompletedEventHandler(Tenancy tenancy, ISendMessageService sendMessageService, IBackgroundJobClient backgroundJobClient)
    : INotificationHandler<TransferCompletedEvent>
{
    public async Task Handle(TransferCompletedEvent notification, CancellationToken cancellationToken)
    {
        var transaction = notification.Model;
        var notice = EventNotice.Build("__TRANSFER_COMPLETED__", notification.Model.Id, 0,
            notification.Model.ReferenceNumber);

        await sendMessageService.SendEventToManagerAsync(tenancy.GetTenantId(), notice);
        if (transaction.SourceAccountType != (int)TransactionAccountTypes.Account ||
            transaction.TargetAccountType != (int)TransactionAccountTypes.Account)
            return;

        backgroundJobClient.Enqueue<IGeneralJob>(x =>
            x.TransactionBetweenTradeAccountCompletedAsync(tenancy.GetTenantId(), transaction.Id));
    }
}