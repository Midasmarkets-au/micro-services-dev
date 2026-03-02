using Bacera.Gateway.Auth;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;

namespace Bacera.Gateway.Web.EventHandlers;

public class UserRegisteredEvent(
    User model,
    string password,
    string callbackUrl,
    string? sourceComment = null,
    string? utm = null) : INotification
{
    public User Model { get; } = model;
    public string CallbackUrl { get; } = callbackUrl;
    public string Password { get; set; } = password;
    public string? SourceComment { get; set; } = sourceComment;
    public string? Utm { get; set; } = utm;
}

public class UserRegisteredEventHandler(
    Tenancy tenancy,
    ILeadService leadService,
    UserService userService,
    ISendMessageService sendMessageService,
    IBackgroundJobClient backgroundJobClient)
    : INotificationHandler<UserRegisteredEvent>
{
    private const string DefaultTestEmail = "hanktsou@bacera.com";

    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        // var model = new ConfirmEmailViewModel(notification.Model.Email!,
        //     notification.Model.GuessUserName(), notification.CallbackUrl);
        // _backgroundJobClient.Enqueue<ISendMailJob>(x =>
        //     x.ConfirmEmailAsync(notification.TenantId, model, notification.Model.Language));

        var debugModel = new ConfirmEmailViewModel(DefaultTestEmail, notification.Model.GuessUserName(),
            notification.CallbackUrl);
        var user = notification.Model;
        if (IsEmailTestRuleMatch(notification.Model.Email!))
            backgroundJobClient.Enqueue<IGeneralJob>(x =>
                x.ConfirmEmailAsync(user.TenantId, debugModel, notification.Model.Language));

        backgroundJobClient.Enqueue<IGeneralJob>(x =>
            x.UserRegisteredAsync(user.TenantId, user.PartyId, notification.Password));

        await userService.UpdateSearchAsync(new User.Criteria { Id = user.Id });

        var notice = EventNotice.Build("__USER_REGISTERED__", notification.Model.Id, 0, notification.Model.Email ?? "");
        await sendMessageService.SendEventToManagerAsync(tenancy.GetTenantId(), notice);
        await leadService.TryReferenceTo(
            notification.Model.PartyId,
            email: notification.Model.Email!,
            phoneNumber: notification.Model.PhoneNumber!,
            $"{notification.Model.FirstName} {notification.Model.LastName}",
            sourceComment: notification.SourceComment,
            utm: notification.Utm
        );
    }

    private static bool IsEmailTestRuleMatch(string email)
    {
        var str = email.Split('@');
        return str.First().Contains("+em");
    }
}