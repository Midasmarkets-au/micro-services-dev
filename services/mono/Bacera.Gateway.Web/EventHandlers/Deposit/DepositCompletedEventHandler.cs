using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.EventHandlers;

public class DepositCompletedEvent(long depositId) : INotification
{
    public long DepositId { get; } = depositId;
}

public class DepositCompletedEventHandler(
    Tenancy tenancy,
    TenantDbContext tenantCtx,
    ISendMessageService sendMessageService,
    IBackgroundJobClient backgroundJobClient)
    : INotificationHandler<DepositCompletedEvent>
{
    public async Task Handle(DepositCompletedEvent notification, CancellationToken cancellationToken)
    {
        var tenantId = tenancy.GetTenantId();
        var deposit = await tenantCtx.Deposits
            .Select(x => new { x.Id, x.PartyId, x.IdNavigation.StateId, x.TargetAccountId })
            .SingleOrDefaultAsync(x => x.Id == notification.DepositId, cancellationToken: cancellationToken);
        if (deposit == null) return;

        var notice = EventNotice.Build("__DEPOSIT_COMPLETED__", deposit.Id, 0, deposit.PartyId.ToString());
        await sendMessageService.SendEventToManagerAsync(tenantId, notice);

        if (deposit.StateId == (int)StateTypes.DepositCallbackCompleted)
        {
            var callbackNotice = EventNotice.Build("__DEPOSIT_CALLBACK_COMPLETED__", deposit.Id, 0, deposit.PartyId.ToString());
            await sendMessageService.SendEventToManagerAsync(tenantId, callbackNotice);
        }

        if (deposit.TargetAccountId is not > 0)
            return;

        backgroundJobClient.Enqueue<IGeneralJob>(x => x.DepositCompletedAsync(tenantId, deposit.Id));
    }
}