using Bacera.Gateway.Auth;
using Bacera.Gateway.Interfaces;
using MediatR;

namespace Bacera.Gateway.Web.EventHandlers;

public class AccountUpdatedEvent(long id) : INotification
{
    public long Id { get; } = id;
}

public class AccountUpdatedEventHandler(ILogger<AccountUpdatedEventHandler> logger) : INotificationHandler<AccountUpdatedEvent>
{
    private readonly ILogger<AccountUpdatedEventHandler> _logger = logger;

    public async Task Handle(AccountUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await Task.Delay(0, cancellationToken);
        var accountId = notification.Id;
    }
}