using Newtonsoft.Json;

namespace Bacera.Gateway;

public sealed class CreateSendTopicContentSpec
{
    public long TopicId { get; set; }

    public Dictionary<string, SendLanguageSpec> Contents { get; set; } = new(); // language -> title, subtitle, content

    public List<string>? ReceiverEmails { get; set; }

    public string TopicKey { get; set; } = string.Empty;
    public bool Overwrite { get; set; }
    public SiteTypes? SiteId { get; set; }

    public SendBatchEmailInfo ToSendBatchEmailInfo()
    {
        return new SendBatchEmailInfo
        {
            TopicId = TopicId,
            TopicKey = TopicKey,
            Contents = Contents,
            SiteId = SiteId,
            ReceiverEmails = ReceiverEmails
        };
    }
}

public sealed class SendBatchEmailTestSpec
{
    public string Uuid { get; set; } = string.Empty;
    public List<string> TestEmails { get; set; } = [];
}

public sealed class SendBatchEmailConfirmSpec
{
    public string Uuid { get; set; } = string.Empty;
    public long Total { get; set; }
}

public sealed class SendBatchEmailInfo
{
    public string Uuid { get; set; } = string.Empty;
    public string TopicKey { get; set; } = string.Empty;
    public long TopicId { get; set; }
    public Dictionary<string, SendLanguageSpec> Contents { get; set; } = new(); // language -> title, subtitle, content
    public List<string>? ReceiverEmails { get; set; }
    public SiteTypes? SiteId { get; set; }

    public long Total { get; set; }
    public long NoPromotion { get; set; }
    public long Sent { get; set; }
    public long Failed { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string[]> FailedEmails { get; set; } = [];
    public List<string> SucceedEmails { get; set; } = [];
    public List<string[]> RKeys { get; set; } = [];
    public string ToJson() => JsonConvert.SerializeObject(this);
}

public sealed class SendLanguageSpec
{
    public string Language { get; set; } = LanguageTypes.English;
    public string Title { get; set; } = string.Empty;
    public string SubTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ToJson() => JsonConvert.SerializeObject(this);
    public static SendLanguageSpec Parse(string json) => JsonConvert.DeserializeObject<SendLanguageSpec>(json)!;
}

public sealed class EmailBatchStatInfo
{
    public long Total { get; set; }
    public long Sent { get; set; }
    public long Failed { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> FailedEmails { get; set; } = [];
}

public class UserSendBatchEmailQueryBasic
{
    public string Email { get; set; } = "";
    public string Language { get; set; } = LanguageTypes.English;
    public long UserId { get; set; }
}

public sealed class SendToPartyRequest
{
    public long PartyId { get; set; }
    public long TopicId { get; set; }
    public string Language { get; set; } = "en-us";
    public string Title { get; set; } = string.Empty;
    public string SubTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> CC { get; set; } = [];
    public List<string> BCC { get; set; } = [];
}

public sealed class BatchEmailDetailItem
{
    public string Email { get; set; } = string.Empty;
    public string IsSuccess { get; set; } = "0";
    public string IsFail { get; set; } = "0";
}