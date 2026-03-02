namespace Bacera.Gateway.Vendor.Bakong;

public class BakongOptions
{
    public string EndPoint => $"{EndPointBase}/app/page/{MerchantToken}";
    public string EndPointBase { get; set; } = null!;
    public string MerchantSecret { get; set; } = string.Empty;
    public string MerchantToken { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;

    public string Network { get; set; } = string.Empty;

    public static BakongOptions FromJson(string json) => Utils.JsonDeserializeObjectWithDefault<BakongOptions>(json);
}