using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader;

public class TokenResponse
{
    [JsonProperty("access_token")] public string? AccessToken { get; set; }
    [JsonProperty("refresh_token")] public string? RefreshToken { get; set; }
    [JsonProperty("expiration")] public DateTime Expiration { get; set; } = DateTime.MinValue;
}