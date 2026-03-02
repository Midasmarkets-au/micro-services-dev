using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.EventHandlers;

public class VerificationSubmittedEvent(Verification model) : INotification
{
    public Verification Model { get; } = model;
}

public class VerificationSubmittedEventHandler(
    Tenancy tenancy,
    ISendMessageService sendMessageService,
    ILeadService leadService,
    TenantDbContext tenantCtx,
    ILogger<VerificationSubmittedEventHandler> logger,
    AutoCreateAccountService autoCreateAccountSvc)
    : INotificationHandler<VerificationSubmittedEvent>
{
    public async Task Handle(VerificationSubmittedEvent notification, CancellationToken cancellationToken)
    {
        var notice = EventNotice.Build("__VERIFICATION_SUBMITTED__", notification.Model.Id, notification.Model.Type);
        await sendMessageService.SendEventToManagerAsync(tenancy.GetTenantId(), notice);
        await leadService.AppendEvent(notification.Model.PartyId, notification.Model,
            LeadStatusTypes.UserVerificationUnderReview);

        var (_, msg) = await autoCreateAccountSvc.TryAutoCreateTradeAccountFromPartyAsync(notification.Model.PartyId);
        var comment = Comment.Build(notification.Model.Id, 1, CommentTypes.Verification, msg);
        tenantCtx.Comments.Add(comment);
        await tenantCtx.SaveChangesAsync(cancellationToken);
        logger.LogInformation("AutoCreateTradeAccount_failed_partyId:{partyId}_msg:{msg}", notification.Model.PartyId, msg);
    }
}