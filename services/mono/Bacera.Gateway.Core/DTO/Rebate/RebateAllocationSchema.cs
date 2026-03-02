using Newtonsoft.Json;

namespace Bacera.Gateway;

public class RebateAllocationSchema
{
    public List<RebateAllocationSchemaItem> Alpha { get; set; } = new();
    public List<RebateAllocationSchemaItem> Advantage { get; set; } = new();
    public List<RebateAllocationSchemaItem> Standard { get; set; } = new();
    public string ToJson() => JsonConvert.SerializeObject(this);
    public static RebateAllocationSchema FromJson(string json)
        => JsonConvert.DeserializeObject<RebateAllocationSchema>(json) ?? new RebateAllocationSchema();
    public bool IsEmpty() => Alpha.Count == 0 && Advantage.Count == 0 && Standard.Count == 0;
}