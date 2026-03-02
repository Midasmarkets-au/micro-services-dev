namespace Bacera.Gateway.Vendor.OFAPay;

public class OFAPayOptions
{
    public string EndPoint { get; set; } = null!;
    public string SecretKey { get; set; } = string.Empty;
    public string MerchantId { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public string PayType { get; set; } = null!;
    public string ProductName { get; set; } = null!;

    public string CallbackDomain { get; set; } = null!;
    public string? RedirectPage { get; set; } 

    public string CallbackUrl => CallbackDomain + $"/api/v1/payment/callback/{10000}/ofa-pay";


    public static OFAPayOptions FromJson(string json)
        => Utils.JsonDeserializeObjectWithDefault<OFAPayOptions>(json);
}