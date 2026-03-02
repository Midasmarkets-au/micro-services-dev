using Bacera.Gateway.Services;
using Bacera.Gateway.Services.Extension;
using Bacera.LiveTrade.MyHub;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bacera.Gateway.Msg.Services;

public static class ChatTasks
{
    public static async Task DeliverChatMessageTask(IServiceProvider provider, long tenantId, long chatMessageId)
    {
        using var scope = provider.CreateTenantScope(tenantId);
        var tenantCtx = scope.ServiceProvider.GetTenantDbContext();
        var chatSvc = scope.ServiceProvider.GetRequiredService<ChatService>();
        var redisPubSubSvc = scope.ServiceProvider.GetRequiredService<RedisPubSubWithAckService>();

        var inboxes = await chatSvc.CreateInboxesAsync(chatMessageId);
        foreach (var inbox in inboxes)
        {
            var rChannelName = BaseHub.GetPartyChannelNameByPartyId(tenantId, inbox.ReceiverPartyId);
            var res = await redisPubSubSvc.PublishWithAckAsync(rChannelName, inbox.ChatMessage.Content);
            if (!res) continue;

            inbox.DeliveredOn = DateTime.UtcNow;
            tenantCtx.Entry(inbox).Property(y => y.DeliveredOn).IsModified = true;
        }

        await tenantCtx.SaveChangesAsync();
    }

    public static async Task DeliverUnDeliveredMessageForPartyTask(IServiceProvider provider, long tenantId,
        long partyId)
    {
        using var scope = provider.CreateTenantScope(tenantId);
        var tenantCtx = scope.ServiceProvider.GetTenantDbContext();
        var redisPubSubSvc = scope.ServiceProvider.GetRequiredService<RedisPubSubWithAckService>();

        var inboxes = await tenantCtx.ChatMessageInboxes
            .Where(x => x.ReceiverPartyId == partyId && x.DeliveredOn == null)
            .Include(x => x.ChatMessage)
            .ToListAsync();

        foreach (var inbox in inboxes)
        {
            var rChannelName = BaseHub.GetPartyChannelNameByPartyId(tenantId, inbox.ReceiverPartyId);
            var res = await redisPubSubSvc.PublishWithAckAsync(rChannelName, inbox.ChatMessage.Content);
            if (!res) continue;

            inbox.DeliveredOn = DateTime.UtcNow;
            tenantCtx.Entry(inbox).Property(y => y.DeliveredOn).IsModified = true;
        }

        await tenantCtx.SaveChangesAsync();
    }
}