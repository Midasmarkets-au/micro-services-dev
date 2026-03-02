using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Bacera.Gateway.Msg.MyOptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bacera.Gateway.Services;

public class ChatService(
    TenantDbContext tenantCtx,
    UserService userSvc,
    IOptions<WsSchemeOption> wsOption
)
{
    public async Task<Chat> CreateChatAsync(long creatorPartyId, string? chatName)
    {
        var user = await userSvc.GetPartyAsync(creatorPartyId);
        if (string.IsNullOrEmpty(chatName)) chatName = $"Chat CreatedBy {user.EmailRaw}";
        else chatName += user.EmailRaw;

        var chat = new Chat
        {
            Name = chatName,
            CreatedOn = DateTime.UtcNow,
            CreatorPartyId = creatorPartyId,
            Status = "active",
        };

        await tenantCtx.Chats.AddAsync(chat);
        await tenantCtx.SaveChangesAsync();
        
        var token = GenerateTokenByChatId(chat.Id);
        chat.Token = token;

        var chatParticipant = new ChatParticipant
        {
            ChatId = chat.Id,
            PartyId = creatorPartyId,
        };

        await tenantCtx.ChatParticipants.AddAsync(chatParticipant);
        await tenantCtx.SaveChangesAsync();
        return chat;
    }

    public async Task<bool> AddParticipantAsync(long chatId, long participantPartyId)
    {
        var chat = await tenantCtx.Chats.FindAsync(chatId);
        if (chat == null) return false;

        var participant = new ChatParticipant
        {
            ChatId = chatId,
            PartyId = participantPartyId,
        };

        await tenantCtx.ChatParticipants.AddAsync(participant);
        await tenantCtx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveParticipantAsync(long chatId, long participantPartyId)
    {
        var chat = await tenantCtx.Chats.FindAsync(chatId);
        if (chat == null) return false;

        var participant = await tenantCtx.ChatParticipants
            .FirstOrDefaultAsync(x => x.ChatId == chatId && x.PartyId == participantPartyId);
        if (participant == null) return false;

        tenantCtx.ChatParticipants.Remove(participant);
        await tenantCtx.SaveChangesAsync();
        return true;
    }

    public async Task<ChatMessage?> CreateMessageAsync(long chatId, long senderPartyId, string content,
        string type = "text")
    {
        var chat = await tenantCtx.Chats.FindAsync(chatId);
        if (chat == null) return null;

        var message = new ChatMessage
        {
            ChatId = chatId,
            SenderPartyId = senderPartyId,
            CreatedOn = DateTime.UtcNow,
            Content = content,
            Type = type
        };

        await tenantCtx.ChatMessages.AddAsync(message);
        await tenantCtx.SaveChangesAsync();
        return message;
    }

    public async Task<long> CreateImageAsync(long chatId, long senderPartyId, string imageUrl)
    {
        var chat = await tenantCtx.Chats.FindAsync(chatId);
        if (chat == null) return -1;

        var message = new ChatMessage
        {
            ChatId = chatId,
            SenderPartyId = senderPartyId,
            CreatedOn = DateTime.UtcNow,
            Content = imageUrl,
            Type = "image"
        };

        await tenantCtx.ChatMessages.AddAsync(message);
        await tenantCtx.SaveChangesAsync();
        return message.Id;
    }

    public async Task<List<ChatMessageInbox>> CreateInboxesAsync(long chatMessageId)
    {
        var message = await tenantCtx.ChatMessages.FindAsync(chatMessageId);
        if (message == null) return [];

        var receiverPartyIds = await tenantCtx.ChatParticipants
            .Where(x => x.ChatId == message.ChatId)
            .Select(x => x.PartyId)
            .ToListAsync();

        var items = receiverPartyIds
            .Select(x => new ChatMessageInbox
            {
                ChatMessageId = chatMessageId,
                ReceiverPartyId = x,
            })
            .ToList();

        await tenantCtx.ChatMessageInboxes.AddRangeAsync(items);
        await tenantCtx.SaveChangesAsync();
        return items;
    }

    public async Task<List<ChatMessageInbox>> GetPartyUndeliveredInboxes(long partyId)
    {
        var inboxes = await tenantCtx.ChatMessageInboxes
            .Where(x => x.ReceiverPartyId == partyId && x.DeliveredOn == null)
            .Include(x => x.ChatMessage)
            .ToListAsync();
        return inboxes;
    }

    public string GenerateTokenByChatId(long chatId)
    {
        var certificate = new X509Certificate2(wsOption.Value.PfxPath, wsOption.Value.Password,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);

        var keyMaterial = certificate.GetCertHash();

        var nonce = new byte[8];
        RandomNumberGenerator.Fill(nonce);

        var data = new byte[16];
        BitConverter.GetBytes((uint)chatId).CopyTo(data, 0);

        var expiration = (uint)DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
        BitConverter.GetBytes(expiration).CopyTo(data, 4);

        nonce.CopyTo(data, 8);

        using var hmac = new HMACSHA256(keyMaterial);
        var mac = hmac.ComputeHash(data);

        var shorterMac = mac.Take(16).ToArray();

        var tokenBytes = new byte[data.Length + shorterMac.Length];
        data.CopyTo(tokenBytes, 0);
        shorterMac.CopyTo(tokenBytes, data.Length);

        // Base64url encode (results in ~40 chars)
        var token = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        return token;
    }

}