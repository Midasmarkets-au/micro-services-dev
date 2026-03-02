using Bacera.Gateway.Auth;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;

namespace Bacera.Gateway.Web.EventHandlers;

public class UserLoggedInEvent : INotification
{
    public long TenantId { get; set; }
    public User Model { get; }

    public UserLoggedInEvent(long tenantId, User model)
    {
        Model = model;
        TenantId = tenantId;
    }
}

public class UserLoggedInEventHandler : INotificationHandler<UserLoggedInEvent>
{
    private readonly ILogger<UserLoggedInEventHandler> _logger;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public UserLoggedInEventHandler(
        Tenancy tenancy
        , ITradeAccountJob tradeAccountJob
        , IBackgroundJobClient backgroundJobClient
        , ILogger<UserLoggedInEventHandler> logger
    )
    {
        _logger = logger;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task Handle(UserLoggedInEvent notification, CancellationToken cancellationToken)
    {
        await Task.Delay(0);
        _backgroundJobClient.Enqueue<ITradeAccountJob>(x
            => x.CheckTradeAccountBalanceAsync(notification.TenantId, notification.Model.PartyId));
    }
}