using Newtonsoft.Json;

using System.Text.Json.Serialization;

namespace Bacera.Gateway;

public class RebateAllocationSchemaItem
{
    [JsonPropertyName("cid"), JsonProperty("cid")]
    public int SymbolCategoryId { get; set; }

    [JsonPropertyName("r"), JsonProperty("r")]
    public decimal Rate { get; set; }

    [JsonPropertyName("cr"), JsonProperty("cr")]
    public decimal CombinedRate { get; set; }

    public string ToJson() => JsonConvert.SerializeObject(this);
}