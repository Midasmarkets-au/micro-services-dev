using Bacera.Gateway.Auth;
using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.EventHandlers;

public class VerificationApprovedEvent(Verification model, Application application) : INotification
{
    public Verification Model { get; } = model;
    public Application Application { get; } = application;
}

public class VerificationApprovedEventHandler(
    Tenancy tenancy,
    ISendMessageService sendMessageService,
    ILeadService leadService)
    : INotificationHandler<VerificationApprovedEvent>
{
    // private readonly long _tenantId = tenancy.GetTenantId();
    public async Task Handle(VerificationApprovedEvent notification, CancellationToken cancellationToken)
    {
        var notice = EventNotice.Build("__VERIFICATION_APPROVED__", notification.Model.Id, notification.Model.Type);
        await sendMessageService.SendEventToManagerAsync(tenancy.GetTenantId(), notice);

        await leadService.AppendEvent(notification.Model.PartyId, notification.Model,
            LeadStatusTypes.UserVerificationApproved);

        await leadService.AppendEvent(notification.Model.PartyId, notification.Application,
            LeadStatusTypes.AccountApplicationCreated);

        // var user = await userManager.Users
        //     .Where(x => x.TenantId == _tenantId && x.PartyId == notification.Model.PartyId)
        //     .SingleOrDefaultAsync(cancellationToken: cancellationToken);
        // if (user == null) return;
        //
        // if (await userManager.IsInRoleAsync(user, UserRoleTypesString.Guest))
        // {
        //     await userManager.RemoveFromRoleAsync(user, UserRoleTypesString.Guest);
        // }
        //
        // if (false == await userManager.IsInRoleAsync(user, UserRoleTypesString.Client))
        // {
        //     await userManager.AddToRoleAsync(user, UserRoleTypesString.Client);
        // }
    }
}