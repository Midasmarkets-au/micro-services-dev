using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bacera.Gateway;

public class EventNotice
{
    public long Id { get; set; }
    public int Type { get; set; }
    public string Event { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public string ReferenceNumber = string.Empty;

    public string ToJson() => JsonConvert.SerializeObject(this, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

    public static EventNotice Build(string @event, long id, int type = 0, string referenceNumber = "", string message = "")
        => new()
        {
            Id = id,
            Type = type,
            Event = @event,
            Message = message,
            ReferenceNumber = referenceNumber,
        };
}