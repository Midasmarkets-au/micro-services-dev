using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Bacera.Gateway.Web;

public class MarketHub : Hub
{
    /// <summary>
    /// Subscribe channels when connected
    /// </summary>
    /// <returns></returns>
    public override async Task OnConnectedAsync()
    {
        await SubscribeChannels();
        await base.OnConnectedAsync();
    }
    
    public static string GetTenantMarketGroupName(long tenantId) => $"GROUP:TENANT:{tenantId}:Market";
    
    public static string GetOCEXGroupName() => $"GROUP:Source:OCEX:Market";
    public static string GetMMGroupName() => $"GROUP:Source:MM:Market";

    /// <summary>
    /// Unsubscribe channels when disconnected
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public override async Task OnDisconnectedAsync(Exception? e)
    {
        await UnsubscribeChannels();
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Subscribe Channels for party
    /// </summary>
    /// <returns></returns>
    private async Task<bool> SubscribeChannels()
    {
        var user = Context.User;
        var context = Context.GetHttpContext();
        if (context is not { Request.Host.HasValue: true }) return false;

        var tenantId = GetTenantId(context);

        // Handle Source parameter for MT5 pricing logic separation
        var source = context.Request.Query["source"].FirstOrDefault();
        if (!string.IsNullOrEmpty(source) && source.ToUpper() == "OCEX")
        {
            // OCEX: only subscribe to the OCEX group; do not add default tenant market groups
            var sourceGroupName = GetOCEXGroupName();
            await Groups.AddToGroupAsync(Context.ConnectionId, sourceGroupName);

            // Store source in connection items for later use
            Context.Items["Source"] = source;

            // Log for debugging
            var logger = context.RequestServices.GetService<ILogger<MarketHub>>();
            logger?.LogInformation("Client connected with Source parameter: {Source} to group: {GroupName}",
                source, sourceGroupName);
        }
        else
        {
            // Default MM: subscribe to main tenant market group
            await Groups.AddToGroupAsync(Context.ConnectionId, GetTenantMarketGroupName(tenantId));
            // And also a global MM group so we can broadcast to MM-only clients
            await Groups.AddToGroupAsync(Context.ConnectionId, GetMMGroupName());

            // Handle account type filtering if provided in query parameters
            var accountTypes = context.Request.Query["accountTypes"].ToArray();
            if (accountTypes.Length > 0)
            {
                foreach (var accountType in accountTypes)
                {
                    if (!string.IsNullOrEmpty(accountType))
                    {
                        var accountGroupName = $"GROUP:TENANT:{tenantId}:Market:AccountType:{accountType}";
                        await Groups.AddToGroupAsync(Context.ConnectionId, accountGroupName);
                    }
                }
            }
        }
        
        return true;
    }

    /// <summary>
    /// Unsubscribe User / Tenant Channels
    /// </summary>
    /// <returns></returns>
    private async Task<bool> UnsubscribeChannels()
    {
        var context = Context.GetHttpContext();
        if (context is not { Request.Host.HasValue: true }) return false;

        // var tenantExists = await GetTenantByHost(context.Request.Host.Host);
        // if (tenantExists == null || tenantExists.Id != GetTenantId()) return false;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetTenantMarketGroupName(GetTenantId(context)));
        return true;
    }

    private static long GetTenantId(HttpContext context)
        => ParseId(GetUserClaim(context, UserClaimTypes.TenantId));

    private static string GetUserClaim(HttpContext context, string claimType)
        => context.User.Claims.FirstOrDefault(x => x.Type.Equals(claimType))?.Value ?? string.Empty;

    private static long ParseId(string idString)
        => string.IsNullOrEmpty(idString) ? -1 : (long.TryParse(idString, out var id) ? id : -1);

    //public async Task SendMessage(string user, string message)
    //{
    //    await Clients.All.SendAsync("ReceiveMessage", user, message);
    //}

    // private async Task<Tenant?> GetTenantByHost(string host)
    // {
    //     var tenants = await _tenantSvc.GetAllTenantsAsync();
    //     return tenants
    //         .FirstOrDefault(x => x.Domains.Any(d => d.DomainName == host));
    // }

    // private long GetTenantId()
    // {
    //     var tid = Context.GetHttpContext()?.User.Claims.FirstOrDefault(x => x.Type == UserClaimTypes.TenantId)
    //         ?.Value;
    //     return (long.TryParse(tid, out var id)) ? id : -1L;
    // }
}