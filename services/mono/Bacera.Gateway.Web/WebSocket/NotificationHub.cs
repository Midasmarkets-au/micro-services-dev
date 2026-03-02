using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.Gateway.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Bacera.Gateway.Web.WebSocket;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class NotificationHub(IServiceProvider serviceProvider) : Hub
{
    public override async Task OnConnectedAsync()
    {
        await SubscribeChannels();
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? e)
    {
        await UnsubscribeChannels();
        await base.OnConnectedAsync();
    }

    public async Task<bool> SubscribeChannels()
    {
        var groups = GetUserSocketGroups();
        if (groups.Count < 1) return false;

        foreach (var group in groups)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
        }
        return true;
    }

    public async Task<bool> UnsubscribeChannels()
    {
        var groups = GetUserSocketGroups();
        if (groups.Count < 1) return false;

        foreach (var group in groups)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        }
        return true;
    }

    private List<string> GetUserSocketGroups()
    {
        var (tenantId, partyId) = (GetTenantId(), GetPartyId());
        if (partyId < 1) return [];
        var context = Context.GetHttpContext();
        if (context is not { Request.Host.HasValue: true }) return [];

        var groups = new List<string> { GetGeneralGroupName(tenantId), GetPartyGroupName(tenantId, partyId) };
        var user = Context.GetHttpContext()?.User;
        if (user == null) return groups;

        if (IsManager()) groups.Add(GetManagerGroupName(tenantId));

        var roleGroups = UserRoleTypesExtensions.GetAllRoleString()
            .Where(role => user.IsInRole(role))
            .Select(role => GetRoleGroupName(tenantId, role))
            .ToList();

        groups.AddRange(roleGroups);
        return groups;
    }

    public async Task RequestUserDetail(string name, string arg1, string arg2)
    {
        var tenantId = GetTenantId();
        var operatorPartyId = GetPartyId();
        var userPartyId = long.Parse(name);
        using var scope = serviceProvider.CreateTenantScope(tenantId);
        var userSvc = scope.ServiceProvider.GetRequiredService<UserService>();
        var operatorUser = await userSvc.GetPartyAsync(operatorPartyId);
        var user = await userSvc.GetPartyAsync(userPartyId);

        var supervisor = GetRoleGroupName(tenantId, UserRoleTypesString.Supervisor);
        var message = JsonConvert.SerializeObject(new
        {
            operatorUser = operatorUser.EmailRaw,
            user = user.EmailRaw,
            userPartyId,
            operatorPartyId,
        });
        await Clients.Group(supervisor).SendAsync("RequestViewUser", message);
    }

    public async Task ApproveUserDetail(string arg0, string arg1, string arg2)
    {
        var tenantId = GetTenantId();
        var userPartyId = long.Parse(arg0);
        var operatorPartyId = long.Parse(arg1);

        using var scope = serviceProvider.CreateTenantScope(tenantId);
        var userSvc = scope.ServiceProvider.GetRequiredService<UserService>();
        var user = await userSvc.GetPartyAsync(userPartyId);

        var operatorName = GetPartyGroupName(tenantId, operatorPartyId);
        var message = JsonConvert.SerializeObject(new
        {
            email = user.EmailRaw,
            phone = user.PhoneNumberRaw,
        });
        await Clients.Group(operatorName).SendAsync("ReceiveViewUser", message);
    }
    
    public async Task RequestReport(string name, string arg1, string arg2)
    {
        var tenantId = GetTenantId();
        var getTenantAdminGroupName = GetManagerGroupName(tenantId);
        var report = await GetReport(name, arg1, arg2);
        await Clients.Group(getTenantAdminGroupName).SendAsync("ReceiveReport", report.ToJson());
    }

    public async Task SendMessageToGroup(string group, string method, string arg2)
    {
        // BcrLog.Slack($"Received_SendMessageToGroup:_{arg2},[{Environment.MachineName}]");
        await Clients.Group(group).SendAsync(method, MessagePopupDTO.Parse(arg2).ToJson());
    }

    private long GetPartyId() => Context.GetHttpContext()?.User.GetPartyId() ?? -1;
    private long GetTenantId() => Context.GetHttpContext()?.User.GetTenantId() ?? -1;
    private bool IsManager()
    {
        var user = Context.GetHttpContext()?.User;
        if (user == null) return false;
        return user.IsInRole(UserRoleTypesString.TenantAdmin) || user.IsInRole(UserRoleTypesString.Admin);
    }

    private async Task<ReportNotice> GetReport(string name, string arg1 = "", string arg2 = "")
    {
        using var scope = serviceProvider.CreateTenantScope(GetTenantId());
        var reportSvc = scope.ServiceProvider.GetRequiredService<ReportService>();
        switch (name)
        {
            case ReportTypes.AwaitingVerificationCount:
                return ReportNotice.Build(name, await reportSvc.AwaitingApproveUserVerificationCountAsync());
            case ReportTypes.ProcessingKycCount:
                return ReportNotice.Build(name, await reportSvc.AwaitingReviewAndUnderReviewKycVerificationCountAsync());
            case ReportTypes.AwaitingApproveKycCount:
                return ReportNotice.Build(name, await reportSvc.AwaitingApproveKycVerificationCountAsync());
            case ReportTypes.AwaitingAccountApplicationCount:
                return ReportNotice.Build(name, await reportSvc.AwaitingApproveAccountApplicationCountAsync());
            case ReportTypes.AwaitingTransferCount:
                return ReportNotice.Build(name, await reportSvc.AwaitingApproveTransferCountAsync());
            case ReportTypes.AwaitingDepositCount:
                return ReportNotice.Build(name, await reportSvc.AwaitingApproveDepositCountAsync());
            case ReportTypes.AwaitingWithdrawalCount:
                return ReportNotice.Build(name, await reportSvc.AwaitingApproveWithdrawalCountAsync());
            case ReportTypes.AwaitingWholesaleApplicationCount:
                return ReportNotice.Build(name, await reportSvc.AwaitingApproveWholesaleApplicationCountAsync());
            case ReportTypes.AwaitingChangeLeverageApplicationCount:
                return ReportNotice.Build(name, await reportSvc.AwaitingApproveChangeLeverageApplicationCountAsync());
            case ReportTypes.AwaitingChangePasswordApplicationCount:
                return ReportNotice.Build(name, await reportSvc.AwaitingApproveChangePasswordApplicationCountAsync());
            case ReportTypes.AwaitingAutoCreatedAccountCount:
                return ReportNotice.Build(name, await reportSvc.AwaitingAutoCreatedAccountCountAsync());
        }

        return ReportNotice.Build("NotFound");
    }

    public static string GetGeneralGroupName(long tenantId) => $"GROUP:TENANT:{tenantId}:General";
    public static string GetManagerGroupName(long tenantId) => $"GROUP:TENANT:{tenantId}:Manager";
    public static string GetRoleGroupName(long tenantId, string role) => $"GROUP:TENANT:{tenantId}:ROLE:{role}".ToUpper();
    public static string GetPartyGroupName(long tenantId, long partyId) => $"GROUP:TENANT:{tenantId}:PARTY:{partyId}";
}