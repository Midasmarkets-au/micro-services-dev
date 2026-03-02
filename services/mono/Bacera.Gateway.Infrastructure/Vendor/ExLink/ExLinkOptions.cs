using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.ExLink.Models;

public class ExLinkOptions
{
    [JsonProperty("uid")]
    public int Uid { get; set; }
    
    [JsonProperty("requestUrl")]
    public string RequestUrl { get; set; } = string.Empty;
    
    [JsonProperty("coinName")]
    public string CoinName { get; set; } = string.Empty;
    
    [JsonProperty("secretKey")]
    public string SecretKey { get; set; } = string.Empty;
    
    [JsonProperty("payType")]
    public int PayType { get; set; }
    
    [JsonProperty("callbackSecretKey")]
    public string CallbackSecretKey { get; set; } = string.Empty;

    public bool IsValid() =>
        Uid > 0
        && !string.IsNullOrEmpty(RequestUrl)
        && !string.IsNullOrEmpty(SecretKey)
        && !string.IsNullOrEmpty(CallbackSecretKey);

    public static ExLinkOptions FromJson(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<ExLinkOptions>(json) ?? new ExLinkOptions();
        }
        catch
        {
            return new ExLinkOptions();
        }
    }
}