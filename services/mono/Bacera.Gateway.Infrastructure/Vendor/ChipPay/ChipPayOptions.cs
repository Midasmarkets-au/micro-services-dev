using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.ChipPay;

public class ChipPayOptions
{
    public long TenantId { get; set; }
    public string EndPoint { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string CallbackDomain { get; set; } = string.Empty;
    public string CallbackUri => CallbackDomain + $"/api/v1/payment/callback/{TenantId}/chip-pay";
    public string PaymentChannel { get; set; } = "BankCard";
    public string Phone { get; set; } = string.Empty;

    public static ChipPayOptions FromJson(string json) =>
        JsonConvert.DeserializeObject<ChipPayOptions>(json) ?? new ChipPayOptions();

    public bool IsValid() =>
        !string.IsNullOrEmpty(MerchantId)
        && !string.IsNullOrEmpty(PublicKey)
        && !string.IsNullOrEmpty(EndPoint)
        && !string.IsNullOrEmpty(PrivateKey)
        && !string.IsNullOrEmpty(CallbackDomain);
}