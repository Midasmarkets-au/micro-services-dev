using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.Long77Pay;

public class Long77PayOptions
{
    public long TenantId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string MerchantSecret { get; set; } = string.Empty;
    public string CallbackDomain { get; set; } = string.Empty;
    public string CallbackUri => CallbackDomain + $"/api/v1/payment/callback/{TenantId}/long77-pay";

    // Virtual Account endpoints
    public string VAEndpoint { get; set; } = "https://vi.long77.net/gateway/bnb/createVA.do";
    public string VATrackingEndpoint { get; set; } = "http://vi.long77.net/gateway/bnb/paymentDetailsVA.do";
    public string VACallbackUri => CallbackDomain + $"/api/v1/payment/callback/{TenantId}/long77-pay/va";

    public static Long77PayOptions FromJson(string json) =>
        JsonConvert.DeserializeObject<Long77PayOptions>(json) ?? new Long77PayOptions();

    public bool IsValid() =>
        !string.IsNullOrEmpty(MerchantId)
        && !string.IsNullOrEmpty(Endpoint)
        && !string.IsNullOrEmpty(MerchantSecret)
        && !string.IsNullOrEmpty(CallbackDomain);
}