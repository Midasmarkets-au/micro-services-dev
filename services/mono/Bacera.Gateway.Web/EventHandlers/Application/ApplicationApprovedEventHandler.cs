using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs;
using Bacera.Gateway.Web.Services;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.EventHandlers;

public class ApplicationApprovedEvent : INotification
{
    public Application Model { get; }

    public ApplicationApprovedEvent(Application model)
    {
        Model = model;
    }
}

public class ApplicationApprovedEventHandler : INotificationHandler<ApplicationApprovedEvent>
{
    private readonly AuthDbContext _authDbContext;
    private readonly Tenancy _tenancy;
    private readonly ILeadService _leadService;
    private readonly ISendMessageService _sendMessageService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public ApplicationApprovedEventHandler(
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

    public async Task Handle(ApplicationApprovedEvent notification, CancellationToken cancellationToken)
    {
        var notice = EventNotice.Build("__APPLICATION_APPROVED__", notification.Model.Id, notification.Model.Type);
        await _sendMessageService.SendEventToManagerAsync(_tenancy.GetTenantId(), notice);

        var supplement = JsonConvert
            .DeserializeObject<ApplicationSupplement>(notification.Model.Supplement!) ?? new ApplicationSupplement();

        var leadStatus = supplement.Role switch
        {
            AccountRoleTypes.Client => LeadStatusTypes.AccountApplicationApproved,
            AccountRoleTypes.Agent => LeadStatusTypes.AgentAccountCreated,
            _ => LeadStatusTypes.UserNotRegistered
        };

        await _leadService.AppendEvent(notification.Model.PartyId, notification.Model, leadStatus);

        if (notification.Model.Type != (int)ApplicationTypes.WholesaleAccount)
            return;

        // await sendWholesaleApplicationApprovedEmailAsync(notification.Model, cancellationToken);
    }

    private async Task SendWholesaleApplicationApprovedEmailAsync(Application model,
        CancellationToken cancellationToken)
    {
        var user = await _authDbContext.Users
            .Where(x => x.PartyId == model.PartyId && x.TenantId == _tenancy.GetTenantId())
            .Select(x => new Auth.User { Email = x.Email, Id = x.Id, Language = x.Language })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.Email))
            return;

        var vm = new ApplicationForWholesaleApprovedViewModel { Email = user.Email };
        _backgroundJobClient
            .Enqueue<IGeneralJob>(x
                => x.ApplicationForWholesaleApprovedAsync(_tenancy.GetTenantId(), vm, user.Language));

        var vmWelcome = new ApplicationForWholesaleCreatedViewModel
        {
            Email = user.Email,
        };
        _backgroundJobClient.Enqueue<IGeneralJob>(x
            => x.ApplicationForWholesaleCreatedAsync(_tenancy.GetTenantId(), vmWelcome, user.Language));
    }
}