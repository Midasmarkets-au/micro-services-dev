using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.BipiPay;

public class BipiPayOptions
{
    public long TenantId { get; set; }
    public string EndPoint { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string MerchantSecret { get; set; } = string.Empty;
    public string Currency { get; set; } = "RMB";
    public string CallbackDomain { get; set; } = string.Empty;
    public string CallbackUri => CallbackDomain + "/api/v1/payment/callback/10000/bipipay";

    public static BipiPayOptions FromJson(string json) =>
        JsonConvert.DeserializeObject<BipiPayOptions>(json) ?? new BipiPayOptions();

    public bool IsValid() =>
        !string.IsNullOrEmpty(MerchantId)
        && !string.IsNullOrEmpty(EndPoint)
        && !string.IsNullOrEmpty(Currency)
        && !string.IsNullOrEmpty(MerchantSecret);
}