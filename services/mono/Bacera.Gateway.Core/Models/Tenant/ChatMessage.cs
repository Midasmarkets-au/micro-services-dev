using System.Text.Json.Serialization;
using Bacera.Gateway.Core.Types;
using HashidsNet;

namespace Bacera.Gateway;

using M = ChatMessage;
public partial class ChatMessage
{
    public long Id { get; set; }
    public long ChatId { get; set; }
    public long SenderPartyId { get; set; }
    public DateTime CreatedOn { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = "text";

    public virtual Chat Chat { get; set; } = null!;
    public virtual Party SenderParty { get; set; } = null!;
    public virtual ICollection<ChatMessageInbox> ChatMessageInboxes { get; set; } = new List<ChatMessageInbox>();
}

public partial class ChatMessage
{
    private static readonly Hashids Hashids = new(HashIdSaltTypes.Chat, 8,
        HashIdSaltTypes.Dictionary[HashIdSaltTypes.Chat]);

    public string HashId => HashEncode(Id);

    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();
}

public partial class ChatMessage
{
    public sealed class ClientPageModel
    {
        [JsonIgnore] public long Id { get; set; }
        public string HashId => HashEncode(Id);
        public string Content { get; set; } = string.Empty;
        public ClientUserBasicModel Sender { get; set; } = new();
        public DateTime CreatedOn { get; set; }
    }
}

public static class ChatMessageExtensions
{
    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> query)
        => query.Select(x => new M.ClientPageModel
        {
            Id = x.Id,
            Content = x.Content,
            Sender = x.SenderParty.ToClientBasicViewModel(),
            CreatedOn = x.CreatedOn
        });
}