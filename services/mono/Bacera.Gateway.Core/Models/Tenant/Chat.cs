using Bacera.Gateway.Core.Types;
using HashidsNet;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = Chat;

public partial class Chat
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string MetaData { get; set; } = "{}";
    public string Token { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public long CreatorPartyId { get; set; }

    public Party CreatorParty { get; set; } = null!;
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public virtual ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
}

public partial class Chat
{
    private static readonly Hashids Hashids = new(HashIdSaltTypes.Chat, 8,
        HashIdSaltTypes.Dictionary[HashIdSaltTypes.Chat]);

    public string HashId => HashEncode(Id);

    public static string HashEncode(long id) => Hashids.EncodeLong(id);
    public static long HashDecode(string hashId) => Hashids.DecodeLong(hashId).FirstOrDefault();
}

public partial class Chat
{
    public class ClientPageModel
    {
        [JsonIgnore] public long Id { get; set; }
        public string HashId => HashEncode(Id);
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public ClientUserBasicModel Creator { get; set; } = new();
    }

    public sealed class ClientDetailModel : ClientPageModel
    {
        public List<ClientUserBasicModel> Participants { get; set; } = [];
        public List<ChatMessage.ClientPageModel> Messages { get; set; } = [];
    }
}

public partial class Chat
{
    public sealed class MetaDataSpec
    {
        public long TotalParticipants { get; set; }
    }
}

public static class ChatExtensions
{
    public static IQueryable<M.ClientPageModel> ToClientPageModel(this IQueryable<M> query)
        => query.Select(x => new M.ClientPageModel
        {
            Id = x.Id,
            Name = x.Name,
            Status = x.Status,
            Creator = x.CreatorParty.ToClientBasicViewModel(),
        });

    public static IQueryable<M.ClientDetailModel> ToClientDetailModel(this IQueryable<M> query)
        => query.Select(x => new M.ClientDetailModel
        {
            Id = x.Id,
            Name = x.Name,
            Status = x.Status,
            Creator = x.CreatorParty.ToClientBasicViewModel(),
            Participants = x.Participants.Select(p => p.Party.ToClientBasicViewModel()).ToList(),
            Messages = x.Messages
                .OrderByDescending(m => m.Id)
                .Select(m => new ChatMessage.ClientPageModel
                {
                    Id = m.Id,
                    Content = m.Content,
                    Sender = m.SenderParty.ToClientBasicViewModel(),
                    CreatedOn = m.CreatedOn
                }).ToList()
        });
}

