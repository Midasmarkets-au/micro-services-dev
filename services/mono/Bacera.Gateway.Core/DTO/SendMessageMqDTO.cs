using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public sealed class SendMessageMqDTO
{
    public SendMessageMqCategoryTypes Category { get; set; }
    public string Data { get; set; } = string.Empty;

    public string ToJson() => JsonConvert.SerializeObject(this);

    public static bool TryParse(string json, out SendMessageMqDTO source)
    {
        source = new SendMessageMqDTO();
        try
        {
            source = JsonConvert.DeserializeObject<SendMessageMqDTO>(json)!;
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public sealed class SendBatchEmailDTO
{
    public long TenantId { get; set; }
    public long UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Language { get; set; } = LanguageTypes.English;
    public string TopicKey { get; set; } = string.Empty;
    public long TopicId { get; set; }

    public SendMessageMqDTO ToSendMessageMqDTO()
    {
        return new SendMessageMqDTO
        {
            Category = SendMessageMqCategoryTypes.BatchEmail,
            Data = ToJson()
        };
    }

    public static SendBatchEmailDTO Build(long tenantId, long userId, string email, string language, long topicId,
        string topicKey)
    {
        return new SendBatchEmailDTO
        {
            TenantId = tenantId,
            TopicKey = topicKey,
            UserId = userId,
            Email = email,
            Language = language,
            TopicId = topicId
        };
    }
    
    public string ToJson() => JsonConvert.SerializeObject(this);

    public static bool TryParse(string json, out SendBatchEmailDTO source)
    {
        source = new SendBatchEmailDTO();
        try
        {
            source = JsonConvert.DeserializeObject<SendBatchEmailDTO>(json)!;
            return true;
        }
        catch
        {
            return false;
        }
    }
}