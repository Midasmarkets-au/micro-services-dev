using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.PayPal;

public class WebhookPayload
{
    [JsonProperty("id")] public string Id { get; set; } = string.Empty;

    [JsonProperty("event_version")] public string EventVersion { get; set; } = "1.0";

    [JsonProperty("create_time")] public string CreateTime { get; set; } = string.Empty;

    [JsonProperty("resource_type")] public string ResourceType { get; set; } = string.Empty;

    [JsonProperty("resource_version")] public string ResourceVersion { get; set; } = "2.0";

    [JsonProperty("event_type")] public string EventType { get; set; } = string.Empty;

    [JsonProperty("summary")] public string Summary { get; set; } = string.Empty;

    [JsonProperty("resource")] public Resource Resource { get; set; } = new();

    [JsonProperty("links")] public List<Link> Links { get; set; } = [];

    public static WebhookPayload FromJson(string json) => JsonConvert.DeserializeObject<WebhookPayload>(json)!;
}

public class Resource
{
    [JsonProperty("create_time")] public string CreateTime { get; set; } = string.Empty;

    [JsonProperty("purchase_units")] public List<PurchaseUnit> PurchaseUnits { get; set; } = [];

    [JsonProperty("links")] public List<Link> Links { get; set; } = [];

    [JsonProperty("id")] public string Id { get; set; } = string.Empty;

    [JsonProperty("payment_source")] public PaymentSource PaymentSource { get; set; } = new();

    [JsonProperty("intent")] public string Intent { get; set; } = string.Empty;

    [JsonProperty("payer")] public Payer Payer { get; set; } = new();

    [JsonProperty("status")] public string Status { get; set; } = string.Empty;
}

public class Payee
{
    [JsonProperty("email_address")] public string EmailAddress { get; set; } = string.Empty;

    [JsonProperty("merchant_id")] public string MerchantId { get; set; } = string.Empty;
}

public class Shipping
{
    [JsonProperty("name")] public Name Name { get; set; } = new();

    [JsonProperty("address")] public Address Address { get; set; } = new();
}

public class Address
{
    [JsonProperty("address_line_1")] public string AddressLine1 { get; set; } = string.Empty;

    [JsonProperty("admin_area_2")] public string AdminArea2 { get; set; } = string.Empty;

    [JsonProperty("admin_area_1")] public string AdminArea1 { get; set; } = string.Empty;

    [JsonProperty("postal_code")] public string PostalCode { get; set; } = string.Empty;

    [JsonProperty("country_code")] public string CountryCode { get; set; } = string.Empty;
}

public class Link
{
    [JsonProperty("href")] public string Href { get; set; } = string.Empty;

    [JsonProperty("rel")] public string Rel { get; set; } = string.Empty;

    [JsonProperty("method")] public string Method { get; set; } = string.Empty;
}