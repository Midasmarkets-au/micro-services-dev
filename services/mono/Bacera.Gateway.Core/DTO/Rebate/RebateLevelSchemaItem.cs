using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public class RebateLevelSchemaItem
{
    public RebateLevelSchemaItem(int cid, decimal rate)
    {
        CategoryId = cid;
        Rate = rate;
    }

    [JsonPropertyName("cid"), JsonProperty("cid")]
    public int CategoryId { get; set; }

    [JsonPropertyName("r"), JsonProperty("r")]
    public decimal Rate { get; set; }

    public static RebateLevelSchemaItem? FromJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return null;
        try
        {
            return JsonConvert.DeserializeObject<RebateLevelSchemaItem>(json);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static List<RebateLevelSchemaItem>? ListFromJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return null;
        try
        {
            return JsonConvert.DeserializeObject<List<RebateLevelSchemaItem>>(json);
        }
        catch (Exception)
        {
            return null;
        }
    }
}

public static class RebateBaseSchemaItemExtensions
{
    public static string ToJson(this List<RebateLevelSchemaItem> list)
        => Utils.JsonSerializeObject(list);
}