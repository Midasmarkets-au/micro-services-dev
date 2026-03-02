using Bacera.Gateway.Auth;
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

public class ApplicationCreatedEvent(Application model, string? utm = null) : INotification
{
    public Application Model { get; } = model;
    public string? Utm { get; } = utm;
}

public class ApplicationCreatedEventHandler(
    Tenancy tenancy,
    ISendMessageService sendMessageService,
    AuthDbContext authDbContext,
    IBackgroundJobClient backgroundJobClient,
    AutoCreateAccountService autoCreateAccountSvc,
    TenantDbContext tenantCtx,
    ILeadService leadService)
    : INotificationHandler<ApplicationCreatedEvent>
{
    public async Task Handle(ApplicationCreatedEvent notification, CancellationToken cancellationToken)
    {
        EventNotice? notice = null;

        var application = notification.Model;

        switch (application.Type)
        {
            case (int)ApplicationTypes.WholesaleAccount:
                await SendWholesaleApplicationEventEmailAsync(application, cancellationToken);
                break;

            case (int)ApplicationTypes.TradeAccount:
            {
                var user = await authDbContext.Users
                    .Where(x => x.PartyId == application.PartyId && x.TenantId == tenancy.GetTenantId())
                    .SingleOrDefaultAsync(cancellationToken: cancellationToken);
                if (user == null) return;

                var applicationSupplement =
                    JsonConvert.DeserializeObject<ApplicationSupplement>(application.Supplement!);
                var role = applicationSupplement?.Role ?? AccountRoleTypes.Client;
                var leadSourceType = role == AccountRoleTypes.Client
                    ? LeadSourceTypes.TradeAccount
                    : LeadSourceTypes.BecomeAgent;

                var supplement = new Dictionary<string, LeadItem>
                {
                    { application.GetType().Name, application.ToLeadItem() },
                };
                if (notification.Utm != null)
                {
                    supplement["utm"] = new LeadItem { Data = notification.Utm, };
                }
                await leadService.CreateAsync(Lead.CreateSpec.Build(
                    user.PartyId,
                    user.GuessUserName(),
                    user.Email ?? string.Empty,
                    user.PhoneNumber ?? string.Empty,
                    leadSourceType,
                    LeadStatusTypes.AccountApplicationCreated,
                    supplement
                ));

                notice = EventNotice.Build("__APPLICATION_CREATED__", notification.Model.Id, notification.Model.Type);


                var (_, msg) = await autoCreateAccountSvc.TryAutoCreateTradeAccountFromApplicationAsync(notification.Model);
                var comment = Comment.Build(notification.Model.Id, 1, CommentTypes.Application, msg);
                tenantCtx.Comments.Add(comment);
                await tenantCtx.SaveChangesAsync(cancellationToken);
                break;
            }

            case (int)ApplicationTypes.TradeAccountChangeLeverage:
            {
                notice = EventNotice.Build("__LEVERAGE_APPLICATION_CREATED__", notification.Model.Id, notification.Model.Type);
                await SendWholesaleApplicationEventEmailAsync(application, cancellationToken);
                break;
            }
        }

        if (notice == null) return;
        await sendMessageService.SendEventToManagerAsync(tenancy.GetTenantId(), notice);
    }

    private async Task SendWholesaleApplicationEventEmailAsync(Application model,
        CancellationToken cancellationToken)
    {
        var user = await authDbContext.Users
            .Where(x => x.PartyId == model.PartyId && x.TenantId == tenancy.GetTenantId())
            .Select(x => new User { Email = x.Email, Id = x.Id, Language = x.Language })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.Email))
            return;

        var hasPermission = await authDbContext.UserClaims
            .Where(x => x.UserId == user.Id)
            .Where(x => x.ClaimType == UserClaimTypes.Permission)
            .Where(x => x.ClaimValue == UserPermissionTypes.ApplicationWholesaleDisabled)
            .AnyAsync(cancellationToken: cancellationToken);
        if (!hasPermission)
        {
            var permission = new UserClaim
            {
                UserId = user.Id, ClaimType = UserClaimTypes.Permission,
                ClaimValue = UserPermissionTypes.ApplicationWholesaleDisabled
            };
            await authDbContext.UserClaims.AddAsync(permission, cancellationToken);
            await authDbContext.SaveChangesAsync(cancellationToken);
        }

        var supplement = JsonConvert.DeserializeObject<dynamic>(model.Supplement ?? "{}");
        if (supplement?.request?.started?.method != null
            && (int)supplement!.request!.started!.method == 1)
        {
            var vmEvidenceNeeded = new ApplicationForWholesaleEvidenceNeededViewModel
            {
                Email = user.Email,
            };
            backgroundJobClient.Enqueue<IGeneralJob>(x
                => x.ApplicationForWholesaleEvidenceNeededAsync(tenancy.GetTenantId(), vmEvidenceNeeded,
                    user.Language));
        }
        else if (supplement?.request?.started?.method != null
                 && (int)supplement!.request!.started!.method == 2)
        {
            var vmMoreEvidenceNeeded = new ApplicationForWholesaleEvidenceForSophisticatedInvestorNeededViewModel
            {
                Email = user.Email,
            };
            backgroundJobClient.Enqueue<IGeneralJob>(x
                => x.ApplicationForWholesaleEvidenceForSophisticatedInvestorNeededAsync(tenancy.GetTenantId(),
                    vmMoreEvidenceNeeded, user.Language));
        }
    }
}