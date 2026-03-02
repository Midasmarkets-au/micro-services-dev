namespace Bacera.Gateway.Vendor.BeePay;

public class BeePayOptions
{
    public string MerchantId { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public static BeePayOptions FromJson(string json)
        => Utils.JsonDeserializeObjectWithDefault<BeePayOptions>(json);
}