namespace Bacera.Gateway.Vendor.Help2Pay.Models;

public class Help2PayOptions
{
    public string MerchantCode { get; set; } = null!;
    public string SecurityCode { get; set; } = null!;
    public string EndPoint { get; set; } = null!;
    public string CallbackDomain { get; set; } = null!;
    public string CallbackUri => CallbackDomain + $"/api/v1/payment/callback/{10000}/help2pay";


    public static Help2PayOptions FromJson(string json)
        => Utils.JsonDeserializeObjectWithDefault<Help2PayOptions>(json);

    public bool IsValid() =>
        !string.IsNullOrEmpty(MerchantCode)
        && !string.IsNullOrEmpty(SecurityCode)
        && !string.IsNullOrEmpty(EndPoint)
        && !string.IsNullOrEmpty(CallbackDomain);
}