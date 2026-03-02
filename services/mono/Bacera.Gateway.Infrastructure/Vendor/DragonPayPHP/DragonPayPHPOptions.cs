namespace Bacera.Gateway.Vendor.PaymentAsia;

public sealed class DragonPayPHPOptions
{
    public string EndPoint => $"{EndPointBase}/{MerchantToken}";
    public string EndPointBase { get; set; } = null!;
    public string MerchantSecret { get; set; } = string.Empty;
    public string MerchantToken { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Network { get; set; } = string.Empty;

    public static DragonPayPHPOptions FromJson(string json)
        => Utils.JsonDeserializeObjectWithDefault<DragonPayPHPOptions>(json);
}