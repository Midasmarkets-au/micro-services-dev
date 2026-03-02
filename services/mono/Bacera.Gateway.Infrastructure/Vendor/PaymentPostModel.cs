using System.Text.Json.Serialization;

namespace Bacera.Gateway.Vendor;

public class PaymentPostModel
{
    public string RedirectUrl { get; set; } = "";

    [Newtonsoft.Json.JsonIgnore]
    [JsonIgnore]
    public Dictionary<string, string> FormDictionary { get; set; } = new();

    public dynamic Form => Utils.JsonDeserializeObjectWithDefault<dynamic>(FormDictionary.ToString() ?? "{}");
}