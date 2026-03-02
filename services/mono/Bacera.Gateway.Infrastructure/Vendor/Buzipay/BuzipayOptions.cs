using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.Buzipay;

public class BuzipayOptions
{
    [JsonProperty("secretKey")]
    public string SecretKey { get; set; } = string.Empty;
    
    [JsonProperty("webhookSecretKey")]
    public string WebhookSecretKey { get; set; } = string.Empty;
    
    [JsonProperty("apiUrl")]
    public string ApiUrl { get; set; } = "https://sandbox-api.buzipay.com";
    
    [JsonProperty("successUrl")]
    public string SuccessUrl { get; set; } = string.Empty;
    
    [JsonProperty("cancelUrl")]
    public string CancelUrl { get; set; } = string.Empty;
    
    public static BuzipayOptions FromJson(string json)
    {
        return JsonConvert.DeserializeObject<BuzipayOptions>(json) ?? new();
    }
    
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(SecretKey) 
            && !string.IsNullOrEmpty(WebhookSecretKey)
            && !string.IsNullOrEmpty(ApiUrl);
    }
}
