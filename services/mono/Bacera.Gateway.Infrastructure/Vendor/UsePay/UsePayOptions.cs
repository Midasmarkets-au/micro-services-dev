using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.UsePay;

public class UsePayOptions
{
    public string EndPoint { get; set; } = string.Empty;
    public string MerchantNo { get; set; } = string.Empty;
    public string MerchantPublicKey { get; set; } = string.Empty;
    public string PayId { get; set; } = string.Empty;
    public string CallbackDomain { get; set; } = string.Empty;

    public static UsePayOptions FromJson(string json)
    {
        return JsonConvert.DeserializeObject<UsePayOptions>(json) ?? new UsePayOptions();
    }
}