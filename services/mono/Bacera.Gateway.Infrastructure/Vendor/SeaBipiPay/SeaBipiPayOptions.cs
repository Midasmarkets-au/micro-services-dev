using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.SeaBipiPay;

public class SeaBipiPayOptions
{
    public long TenantId { get; set; }
    public string EndPoint { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string MerchantSecret { get; set; } = string.Empty;
    public string Currency { get; set; } = "RMB";
    public string CallbackDomain { get; set; } = string.Empty;
    public string CallbackUri => CallbackDomain + "/api/v1/payment/callback/10000/sea-bipipay";

    public static SeaBipiPayOptions FromJson(string json) =>
        JsonConvert.DeserializeObject<SeaBipiPayOptions>(json) ?? new SeaBipiPayOptions();
}