using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader;

public class ListResponse
{
    [JsonProperty("copying_rules")] public List<Rule> CopyingRules { get; set; } = new();
}