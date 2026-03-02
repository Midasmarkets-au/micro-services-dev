using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader;

public class TargetList
{
    [JsonProperty("target_accounts")] public List<long> TargetAccounts { get; set; } = new();
    [JsonProperty("mode")] public Mode Mode { get; set; } = null!;
}