using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.UniotcPay;

public class UniotcPayOptions
{
    public readonly int MethodId = 8201; // 8111: 企業支付寶; 8201:銀行卡轉卡; 8203:支付寶轉卡
    public int AppId { get; set; }
    public readonly string AppType = "pc";
    public readonly string Version = "v2.0";
    public readonly string SignType = "md5";
    public readonly string ShowMode = "static"; //chat, static
    public long TenantId { get; set; }
    public string EndPoint { get; set; } = null!;
    public string SecurityKey { get; set; } = string.Empty;
    public string CallbackSecurityKey { get; set; } = string.Empty;

    public string CallbackDomain { get; set; } = string.Empty;
    public string ErrorCallbackUri => CallbackDomain + $"/api/v1/payment/callback/{TenantId}/uniotc-pay/error";
    public string SuccessCallbackUri => CallbackDomain + $"/api/v1/payment/callback/{TenantId}/uniotc-pay/success";

    public static UniotcPayOptions FromJson(string json) =>
        JsonConvert.DeserializeObject<UniotcPayOptions>(json) ?? new UniotcPayOptions();

    public bool IsValid() =>
        AppId > 0
        && MethodId > 0
        && !string.IsNullOrEmpty(SecurityKey)
        && !string.IsNullOrEmpty(AppType)
        && !string.IsNullOrEmpty(Version)
        && !string.IsNullOrEmpty(SignType)
        && !string.IsNullOrEmpty(ShowMode)
        && !string.IsNullOrEmpty(CallbackSecurityKey)
        && !string.IsNullOrEmpty(EndPoint);
}