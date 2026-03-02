using Bacera.Gateway.Core.Types;
using MediatR;

namespace Bacera.Gateway.Web.EventHandlers;

public class VerificationStartedEvent : INotification
{
    public Verification Model { get; }

    public VerificationStartedEvent(Verification model)
    {
        Model = model;
    }
}

public class VerificationStartedEventHandler : INotificationHandler<VerificationStartedEvent>
{
    private readonly ILeadService _leadService;
    private readonly Tenancy _tenancy;


    public VerificationStartedEventHandler(
        Tenancy tenancy
        , ILeadService leadService)
    {
        _leadService = leadService;
        _tenancy = tenancy;
    }

    public async Task Handle(VerificationStartedEvent notification, CancellationToken cancellationToken)
    {
        await _leadService.AppendEvent(notification.Model.PartyId, notification.Model, LeadStatusTypes.UserVerifying);
    }
}