using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Interfaces;
using MediatR;

namespace Bacera.Gateway.Web.EventHandlers;

public class VerificationRejectedEvent : INotification
{
    public Verification Model { get; }

    public VerificationRejectedEvent(Verification model)
    {
        Model = model;
    }
}

public class VerificationRejectedEventHandler : INotificationHandler<VerificationRejectedEvent>
{
    private readonly Tenancy _tenancy;
    private readonly ILeadService _leadService;
    private readonly ISendMessageService _sendMessageService;

    public VerificationRejectedEventHandler(
        Tenancy tenancy
        , ISendMessageService sendMessageService
        , ILeadService leadService)
    {
        _tenancy = tenancy;
        _sendMessageService = sendMessageService;
        _leadService = leadService;
    }

    public async Task Handle(VerificationRejectedEvent notification, CancellationToken cancellationToken)
    {
        var notice = EventNotice.Build("__VERIFICATION_REJECTED__", notification.Model.Id, notification.Model.Type);
        await _sendMessageService.SendEventToManagerAsync(_tenancy.GetTenantId(), notice);
        await _leadService.AppendEvent(notification.Model.PartyId, notification.Model,
            LeadStatusTypes.UserVerificationRejected);
    }
}