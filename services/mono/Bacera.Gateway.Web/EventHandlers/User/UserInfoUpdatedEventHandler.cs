using Bacera.Gateway.Auth;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using MediatR;

namespace Bacera.Gateway.Web.EventHandlers;

public class UserInfoUpdatedEvent(User model) : INotification
{
    public User Model { get; } = model;
}

public class UserInfoUpdatedEventHandler(UserService userService) : INotificationHandler<UserInfoUpdatedEvent>
{

    public async Task Handle(UserInfoUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await userService.UpdateSearchAsync(new User.Criteria { Id = notification.Model.Id });
    }
}