using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.EventHandlers;

public class VerificationDocumentRejectedEvent(long partyId) : INotification
{
    public long PartyId { get; } = partyId;
}

public class VerificationDocumentRejectedEventHandler(Tenancy tenancy, IBackgroundJobClient backgroundJobClient)
    : INotificationHandler<VerificationDocumentRejectedEvent>
{
    public Task Handle(VerificationDocumentRejectedEvent notification, CancellationToken cancellationToken)
    {
        backgroundJobClient.Enqueue<IGeneralJob>(x =>
            x.VerificationDocumentRejectedAsync(tenancy.GetTenantId(), notification.PartyId));
        return Task.CompletedTask;
    }
}