using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.UnionePay;

public class UnionePayOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string MerchantKey { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string Currency { get; set; } = "CNY";

    public int PaymentChannel { get; set; } = 2; // 1.银行卡 2. 支付寶 3. 微信 4.雲閃付
    public int RenderType { get; set; } = 1; // 0: 返回 UnionePayX 的 html 收銀台 页面; 1 URL 地址

    public static UnionePayOptions FromJson(string json) =>
        JsonConvert.DeserializeObject<UnionePayOptions>(json) ?? new UnionePayOptions();

    public bool IsValid() =>
        !string.IsNullOrEmpty(MerchantId)
        && !string.IsNullOrEmpty(MerchantKey)
        && !string.IsNullOrEmpty(Endpoint);
}