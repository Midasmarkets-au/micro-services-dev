using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.PayPal;

public class PayPalOrderResponseModel
{
    [JsonProperty("id")] public string Id { get; set; } = null!;

    [JsonProperty("status")] public string Status { get; set; } = null!;

    [JsonProperty("intent")] public string Intent { get; set; } = null!;

    [JsonProperty("payment_source")] public PaymentSource PaymentSource { get; set; } = null!;

    [JsonProperty("purchase_units")] public List<PurchaseUnit> PurchaseUnits { get; set; } = null!;

    [JsonProperty("payer")] public Payer Payer { get; set; } = null!;

    [JsonProperty("create_time")] public string CreateTime { get; set; } = null!;

    [JsonProperty("links")] public List<LinkDescription> Links { get; set; } = null!;

    public static PayPalOrderResponseModel FromJson(string json) =>
        JsonConvert.DeserializeObject<PayPalOrderResponseModel>(json)!;
}

public sealed class Payer
{
    [JsonProperty("name")] public Name Name { get; set; } = null!;

    [JsonProperty("email_address")] public string EmailAddress { get; set; } = null!;

    [JsonProperty("payer_id")] public string PayerId { get; set; } = null!;
}

public sealed class Name
{
    [JsonProperty("given_name")] public string GivenName { get; set; } = null!;

    [JsonProperty("surname")] public string Surname { get; set; } = null!;
}