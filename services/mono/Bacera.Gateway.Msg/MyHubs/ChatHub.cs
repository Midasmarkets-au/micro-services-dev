using Bacera.Gateway;
using Bacera.Gateway.Msg.Services;
using Bacera.Gateway.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace Bacera.LiveTrade.MyHub;

[Authorize]
public class ChatHub(
    RedisPubSubWithAckService redisPubSubSvc,
    IServiceProvider provider,
    ChatService chatSvc
) : BaseHub
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

    /// <summary>
    /// Unsubscribe channels when disconnected
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public override async Task OnDisconnectedAsync(Exception? e)
    {
        await UnsubscribeChannels();
        await base.OnDisconnectedAsync(e);
    }

    private async Task<bool> SubscribeChannels()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, PartyGroup);
        var partyId = GetPartyId();
        var tenantId = GetTenantId();
        var partyRChannelName = GetPartyChannelNameByPartyId(tenantId, partyId);
        redisPubSubSvc.SetupSubscriberForParty(PartyGroup, partyRChannelName);
        _ = ChatTasks.DeliverUnDeliveredMessageForPartyTask(provider, tenantId, partyId);
        return true;
    }

    private async Task<bool> UnsubscribeChannels()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, PartyGroup);
        return true;
    }

    public async Task<bool> CreateChat(string chatName)
    {
        var partyId = GetPartyId();
        var chat = await chatSvc.CreateChatAsync(partyId, chatName);
        var chatIdHash = Chat.HashEncode(chat.Id);
        await Clients.Group(PartyGroup).SendAsync("ChatCreated", chatIdHash);
        return true;
    }

    public async Task<bool> SendMessage(string chatHashId, string message, string type)
    {
        var partyId = GetPartyId();
        var chatId = Chat.HashDecode(chatHashId);
        var chatMessage = await chatSvc.CreateMessageAsync(chatId, partyId, message);
        if (chatMessage == null)
        {
            await Clients.Group(PartyGroup).SendAsync("ChatMessageFailed", chatHashId, message);
            return false;
        }

        await Clients.Group(PartyGroup).SendAsync("ChatMessageSucceed", chatHashId, message);

        var tenantId = GetTenantId();
        _ = ChatTasks.DeliverChatMessageTask(provider, tenantId, chatMessage.Id);
        return true;
    }

    public async Task<bool> AddParticipant(string chatHashId, long participantPartyId)
    {
        var chatId = Chat.HashDecode(chatHashId);
        var result = await chatSvc.AddParticipantAsync(chatId, participantPartyId);
        if (result)
        {
            await Clients.Group(PartyGroup).SendAsync("ChatParticipantAdded", chatHashId);

            var participantGroup = GetPartyGroup(participantPartyId);
            await Clients.Group(participantGroup).SendAsync("NewChatAdded", chatHashId);
        }
        else
        {
            await Clients.Group(PartyGroup).SendAsync("ChatParticipantFailed", chatHashId);
        }

        return result;
    }
}