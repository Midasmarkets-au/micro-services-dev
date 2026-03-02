using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.Services;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.EventHandlers;

public class ApplicationRejectedEvent : INotification
{
    public Application Model { get; }

    public ApplicationRejectedEvent(Application model)
    {
        Model = model;
    }
}

public class ApplicationRejectedEventHandler : INotificationHandler<ApplicationRejectedEvent>
{
    private readonly AuthDbContext _authDbContext;
    private readonly Tenancy _tenancy;
    private readonly ILeadService _leadService;
    private readonly ISendMessageService _sendMessageService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public ApplicationRejectedEventHandler(
        Tenancy tenancy
        , ISendMessageService sendMessageService
        , AuthDbContext authDbContext
        , ILeadService leadService
        , IBackgroundJobClient backgroundJobClient)
    {
        _authDbContext = authDbContext;
        _tenancy = tenancy;
        _sendMessageService = sendMessageService;
        _backgroundJobClient = backgroundJobClient;
        _leadService = leadService;
    }

    public async Task Handle(ApplicationRejectedEvent notification, CancellationToken cancellationToken)
    {
        var notice = EventNotice.Build("__APPLICATION_REJECTED__", notification.Model.Id, notification.Model.Type);
        await _sendMessageService.SendEventToManagerAsync(_tenancy.GetTenantId(), notice);
        await _leadService.AppendEvent(notification.Model.PartyId, notification.Model,
            LeadStatusTypes.AccountApplicationRejected);
        if (notification.Model.Type != (int)ApplicationTypes.WholesaleAccount)
            return;
        await SendWholesaleApplicationRejectEmailAsync(notification.Model, cancellationToken);
    }

    private async Task SendWholesaleApplicationRejectEmailAsync(Application model,
        CancellationToken cancellationToken)
    {
        var user = await _authDbContext.Users
            .Where(x => x.PartyId == model.PartyId && x.TenantId == _tenancy.GetTenantId())
            .Select(x => new Auth.User { Email = x.Email, Id = x.Id, Language = x.Language })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.Email))
            return;

        var vm = new ApplicationForWholesaleRejectedViewModel { Email = user.Email };
        _backgroundJobClient.Enqueue<IGeneralJob>(x
            => x.ApplicationForWholesaleRejectedAsync(_tenancy.GetTenantId(), vm, user.Language));
    }
}