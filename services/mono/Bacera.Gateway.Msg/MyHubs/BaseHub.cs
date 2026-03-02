using Bacera.Gateway.Services.Extension;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace Bacera.LiveTrade.MyHub;

public class BaseHub : Hub
{
    protected string PartyGroup => $"bcr_gtw_msg:PARTY:GROUP_TID:{GetTenantId()}_PID:{GetPartyId()}";
    protected string GetPartyGroup(long partyId) => $"bcr_gtw_msg:PARTY:GROUP_TID:{GetTenantId()}_PID:{partyId}";


    public static string GetPartyChannelNameByPartyId(long tenantId, long partyId) =>
        $"bcr_gtw_msg:PARTY_RCHANNEL_TID:{tenantId}_PID:{partyId}";


    protected long GetPartyId() => Context.GetHttpContext()?.User.GetPartyId() ?? -1;
    protected long GetTenantId() => Context.GetHttpContext()?.User.GetTenantId() ?? -1;
}