namespace Bacera.Gateway.Interfaces;

public interface ISendMessageService
{
    Task SendEventToPartyAsync(long tenantId, long partyId, EventNotice notice);
    Task SendPopupToPartyAsync(long tenantId, long partyId, MessagePopupDTO msgPopup);
    
    Task SendEventToManagerAsync(long tenantId, EventNotice notice);
    Task SendPopupToManagerAsync(long tenantId, MessagePopupDTO notice);

    Task SendEventToRoleAsync(long tenantId, string role, EventNotice notice);
    Task SendPopupToRoleAsync(long tenantId, string role, MessagePopupDTO msgPopup);
    Task SendReadMessageToManagerAsync(long tenantId, string message);
}