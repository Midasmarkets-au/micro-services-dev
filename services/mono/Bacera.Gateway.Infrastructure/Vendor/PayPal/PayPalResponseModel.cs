using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.PayPal;

public sealed class PayPalResponseModel
{
    [JsonProperty("id")] public string Id { get; set; } = null!;

    [JsonProperty("status")] public string Status { get; set; } = null!;

    [JsonProperty("payment_source")] public PaymentSource PaymentSource { get; set; } = null!;

    [JsonProperty("links")] public List<LinkDescription> Links { get; set; } = null!;

    public static PayPalResponseModel FromJson(string json) => JsonConvert.DeserializeObject<PayPalResponseModel>(json)!;
}

public sealed class PayPalInfo
{
    // PayPal info is empty in the response, but we keep the class
    // for potential future extensions
}

public sealed class LinkDescription
{
    [JsonProperty("href")] public string Href { get; set; } = null!;

    [JsonProperty("rel")] public string Rel { get; set; } = null!;

    [JsonProperty("method")] public string Method { get; set; } = null!;
}