using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader;

public class Rule
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("source")] public long Source { get; set; }
    [JsonProperty("tag")] public string Tag { get; set; } = string.Empty;
    [JsonProperty("targets_list")] public List<TargetList> TargetList { get; set; } = new();
}