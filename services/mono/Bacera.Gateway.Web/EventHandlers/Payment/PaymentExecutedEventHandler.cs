using Bacera.Gateway.Interfaces;

using MediatR;

namespace Bacera.Gateway.Web.EventHandlers;

public class PaymentExecutedEvent : INotification
{
    public Matter Model { get; }
    public long PaymentId { get; set; }

    public PaymentExecutedEvent(long paymentId, Matter model)
    {
        PaymentId = paymentId;
        Model = model;
    }
}

public class PaymentExecutedEventHandler : INotificationHandler<PaymentExecutedEvent>
{
    private readonly Tenancy _tenancy;
    private readonly ISendMessageService _sendMessageService;

    public PaymentExecutedEventHandler(
        Tenancy tenancy
        , ISendMessageService sendMessageService
    )
    {
        _tenancy = tenancy;
        _sendMessageService = sendMessageService;
    }

    public async Task Handle(PaymentExecutedEvent notification, CancellationToken cancellationToken)
    {
        var message = EventNotice.Build("__PAYMENT_EXECUTED__",
            notification.Model.Id, notification.Model.Type, notification.PaymentId.ToString());
        await _sendMessageService.SendEventToManagerAsync(_tenancy.GetTenantId(), message);
    }
}