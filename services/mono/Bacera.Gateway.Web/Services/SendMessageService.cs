using Bacera.Gateway.Interfaces;
using Bacera.Gateway.Web.WebSocket;
using Microsoft.AspNetCore.SignalR;

namespace Bacera.Gateway.Web.Services;

public class SendMessageService(IHubContext<NotificationHub> hubContext) : ISendMessageService
{
    public Task SendEventToPartyAsync(long tenantId, long partyId, EventNotice notice)
        => hubContext.Clients.Group(NotificationHub.GetPartyGroupName(tenantId, partyId)).SendAsync("ReceiveEvent", notice.ToJson());

    public Task SendPopupToPartyAsync(long tenantId, long partyId, MessagePopupDTO msgPopup)
        => hubContext.Clients.Group(NotificationHub.GetPartyGroupName(tenantId, partyId)).SendAsync("ReceivePopup", msgPopup.ToJson());

    public Task SendEventToManagerAsync(long tenantId, EventNotice notice)
        => hubContext.Clients.Group(NotificationHub.GetManagerGroupName(tenantId)).SendAsync("ReceiveEvent", notice.ToJson());

    public Task SendPopupToManagerAsync(long tenantId, MessagePopupDTO notice)
        => hubContext.Clients.Group(NotificationHub.GetManagerGroupName(tenantId)).SendAsync("ReceivePopup", notice.ToJson());

    public Task SendEventToRoleAsync(long tenantId, string role, EventNotice notice)
        => hubContext.Clients.Group(NotificationHub.GetRoleGroupName(tenantId, role)).SendAsync("ReceiveEvent", notice.ToJson());

    public Task SendPopupToRoleAsync(long tenantId, string role, MessagePopupDTO msgPopup)
        => hubContext.Clients.Group(NotificationHub.GetRoleGroupName(tenantId, role)).SendAsync("ReceivePopup", msgPopup.ToJson());

    public Task SendReadMessageToManagerAsync(long tenantId, string message)
        => hubContext.Clients.Group(NotificationHub.GetManagerGroupName(tenantId)).SendAsync("ReceiveRead", message);
}