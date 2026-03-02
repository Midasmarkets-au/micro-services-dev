using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.EventHandlers;

public class TradeAccountPasswordChangeRequestedEvent : INotification
{
    public ResetTradeAccountPasswordViewModel Model { get; }

    public TradeAccountPasswordChangeRequestedEvent(ResetTradeAccountPasswordViewModel model)
    {
        Model = model;
    }
}

public class
    TradeAccountPasswordChangeRequestedEventHandler : INotificationHandler<TradeAccountPasswordChangeRequestedEvent>
{
    private readonly AuthDbContext _authDbContext;
    private readonly Tenancy _tenancy;
    private readonly ISendMessageService _sendMessageService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public TradeAccountPasswordChangeRequestedEventHandler(
        Tenancy tenancy
        , ISendMessageService sendMessageService
        , IBackgroundJobClient backgroundJobClient
        , AuthDbContext authDbContext
    )
    {
        _authDbContext = authDbContext;
        _tenancy = tenancy;
        _sendMessageService = sendMessageService;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task Handle(TradeAccountPasswordChangeRequestedEvent notification, CancellationToken cancellationToken)
    {
        await Task.Delay(0, cancellationToken);
        var model = notification.Model;
        var lang = await _authDbContext.Users
            .Where(x => x.TenantId == model.TenantId && x.PartyId == model.PartyId)
            .Select(x => x.Language)
            .FirstOrDefaultAsync(cancellationToken);

        _backgroundJobClient.Enqueue<IGeneralJob>(x =>
            x.ResetTradeAccountPasswordAsync(_tenancy.GetTenantId(), notification.Model, lang ?? "en-us"));
    }
}