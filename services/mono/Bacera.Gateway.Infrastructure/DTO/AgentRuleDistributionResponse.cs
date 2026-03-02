using Newtonsoft.Json;

namespace Bacera.Gateway;

public class AgentRuleDistributionResponse
{
    public int Depth { get; set; }
    public long Id { get; set; }
    public long Uid { get; set; }
    public long? AgentAccountId { get; set; }
    public long? BrokerAccountId { get; set; }

    [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public string? AllocationSchema { get; set; }

    public string RefCode { get; set; } = string.Empty;

    public RebateLevelSchema DistributionSchema { get; set; } = new();
    public RebateLevelSchema RemainSchema { get; set; } = new();

    public RebateAllocationSchema AllocationSchemas
    {
        get
        {
            var empty = new RebateAllocationSchema();
            if (string.IsNullOrEmpty(AllocationSchema)) return empty;
            try
            {
                var obj = JsonConvert.DeserializeObject<RebateAllocationSchema>(AllocationSchema);
                return obj ?? empty;
            }
            catch (Exception)
            {
                return empty;
            }
        }
    }
}