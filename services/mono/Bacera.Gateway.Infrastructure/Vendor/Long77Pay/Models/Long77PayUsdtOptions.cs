using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.Long77Pay;

public class Long77PayUsdtOptions
{
    public long TenantId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string MerchantSecret { get; set; } = string.Empty;
    public string CallbackDomain { get; set; } = string.Empty;
    public string CallbackUri => CallbackDomain + $"/api/v1/payment/callback/{TenantId}/long77-pay/usdt";

    public static Long77PayUsdtOptions FromJson(string json) =>
        JsonConvert.DeserializeObject<Long77PayUsdtOptions>(json) ?? new Long77PayUsdtOptions();

    public bool IsValid() =>
        !string.IsNullOrEmpty(MerchantId)
        && !string.IsNullOrEmpty(Endpoint)
        && !string.IsNullOrEmpty(MerchantSecret)
        && !string.IsNullOrEmpty(CallbackDomain);
}