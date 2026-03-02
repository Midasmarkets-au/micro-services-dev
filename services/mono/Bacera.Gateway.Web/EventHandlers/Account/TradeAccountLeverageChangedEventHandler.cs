using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.ViewModels.Tenant;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.EventHandlers;

public class TradeAccountLeverageChangedEvent : INotification
{
    public long AccountNumber { get; }
    public long PartyId { get; set; }
    public int Leverage { get; set; }
    public int OriginalLeverage { get; set; }

    public TradeAccountLeverageChangedEvent(long partyId, long accountNumber, int leverage, int originalLeverage)
    {
        PartyId = partyId;
        AccountNumber = accountNumber;
        Leverage = leverage;
        OriginalLeverage = originalLeverage;
    }
}

public class TradeAccountLeverageChangedEventHandler(
    Tenancy tenancy,
    ISendMessageService sendMessageService,
    IBackgroundJobClient backgroundJobClient,
    TenantDbContext tenantCtx)
    : INotificationHandler<TradeAccountLeverageChangedEvent>
{
    public async Task Handle(TradeAccountLeverageChangedEvent notification, CancellationToken cancellationToken)
    {
        var user = await tenantCtx.Parties
            .Where(x => x.Id == notification.PartyId)
            .ToTenantBasicViewModel()
            .SingleAsync(cancellationToken: cancellationToken);

        var model = new TradeAccountLeverageChangedViewModel
        {
            AccountNumber = notification.AccountNumber,
            Leverage = notification.Leverage,
            OriginalLeverage = notification.OriginalLeverage,
            NativeName = user.DisplayName,
            Email = user.EmailRaw
        };

        // await _sendMessageService.SendEventToManagerAsync(_tenancyResolver.GetTenantId(),
        //     $"__WITHDRAWAL_CREATED__:{notification.Model.PartyId}");

        backgroundJobClient.Enqueue<IGeneralJob>(x =>
            x.TradeAccountLeverageChangedAsync(tenancy.GetTenantId(), model, user.Language));
    }
}