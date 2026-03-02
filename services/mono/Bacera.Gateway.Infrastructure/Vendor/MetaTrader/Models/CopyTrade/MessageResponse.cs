using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader;

public class MessageResponse
{
    [JsonProperty("id")] public int? Id { get; set; }
    [JsonProperty("message")] public string? Message { get; set; }
}