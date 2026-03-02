using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader;

public class CreateRequest
{
    [JsonProperty("source")] public long Source { get; set; }
    [JsonProperty("tag")] public string Tag { get; set; } = string.Empty;
    [JsonProperty("targets_list")] public List<TargetList> TargetsList { get; set; } = new();

    public static CreateRequest Create(long source, long target, string mode, int? value = 0)
        => new()
        {
            Source = source,
            Tag = $"rule_{source}_{target}_mode_{mode}_value_{value}",
            TargetsList = new List<TargetList>
            {
                new()
                {
                    Mode = new Mode(mode, value),
                    TargetAccounts = new List<long> { target },
                }
            }
        };
}