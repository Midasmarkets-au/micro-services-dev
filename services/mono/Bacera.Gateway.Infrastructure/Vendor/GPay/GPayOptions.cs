using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.GPay;

public class GPayOptions
{
    /*
     * 充值渠道：
     * 24 - H5快捷支付
     * 25 - 支付宝H5唤醒+PC扫码
     * 26 - 支付宝商户扫码
     * 27 - 微信扫码话费
     * 28 - 微信固码扫码
     * 29 - 网银转账
     * 30 - 卡到卡（带附言）
     * 31 - 卡到卡（无附言）
     * 32 - 个码微信自由码
     * 33 - 个码支付宝自由码
     * 34 - C2C支付宝到银行卡
     * 35 - 网银快捷
     * 36 - 网银快捷（免签约）
     * 37 - 微信H5
     * 38 - C2C支付宝个码扫码（混合）
     * 39 - 微信扫码混跑
     * 40 - H5快捷支付（一号一户）
     * 41 - C2C微信自由码（混合）
     * 42 - C2C微到卡
     * 43 - C2C卡到卡（无附言）
     * 44 - C2C卡到卡（带附言）
     * 45 - C2C微信赞赏码
     * 46 - C2C支付宝个码H5
     * 51 - USDT转账
     * 61 - USD电汇
     * 71 - EUR电汇
     */
    public long TenantId { get; set; }
    public string EndPoint { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string MerchantSecret { get; set; } = string.Empty;
    public int MethodId { get; set; } = 51;
    public string CallbackDomain { get; set; } = string.Empty;
    public string CallbackUri => CallbackDomain + $"/api/v1/payment/callback/{TenantId}/gpay";

    public static GPayOptions FromJson(string json) =>
        JsonConvert.DeserializeObject<GPayOptions>(json) ?? new GPayOptions();

    public bool IsValid() =>
        !string.IsNullOrEmpty(MerchantId)
        && !string.IsNullOrEmpty(EndPoint)
        && !string.IsNullOrEmpty(MerchantSecret)
        && !string.IsNullOrEmpty(CallbackDomain);
}