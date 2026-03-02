using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.BigPay.Models;

public class BigPayOptions
{
    public long TenantId { get; set; }
    public string EndPoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string RedirectPrefix { get; set; } = string.Empty;
    public string CallbackDomain { get; set; } = string.Empty;
    public string CallbackSecretKey { get; set; } = string.Empty;

    public string CallbackUri => CallbackDomain + $"/api/v1/payment/callback/{TenantId}/big-pay";

    public static BigPayOptions FromJson(string json) =>
        JsonConvert.DeserializeObject<BigPayOptions>(json) ?? new BigPayOptions();
}