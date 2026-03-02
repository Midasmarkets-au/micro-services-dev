using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.Buy365;

public class NPayOptions
{
    public string ServerIp { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string MerchantSecret { get; set; } = string.Empty;
    public string CallbackSecret { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;

    public static NPayOptions FromJson(string json) =>
        JsonConvert.DeserializeObject<NPayOptions>(json) ?? new NPayOptions();

    public bool IsValid() =>
        !string.IsNullOrEmpty(MerchantId)
        && !string.IsNullOrEmpty(ServerIp)
        && !string.IsNullOrEmpty(Endpoint)
        && !string.IsNullOrEmpty(MerchantSecret)
        && !string.IsNullOrEmpty(CallbackSecret);
}