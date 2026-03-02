namespace Bacera.Gateway.Vendor.PaymentAsia;

public sealed class PaymentAsiaRMBOptions
{
    public string EndPoint { get; set; } = null!;
    public string MerchantId { get; set; } = null!;
    public string MerchantRefKey { get; set; } = null!;
    public string SecurityKey { get; set; } = string.Empty;

    public string Currency { get; set; } = null!;
    public string Gateway { get; set; } = null!;
    public string SignType { get; set; } = null!;
    public string CharSet { get; set; } = null!;
    public string CustomerTel { get; set; } = null!;

    public static PaymentAsiaRMBOptions FromJson(string json)
        => Utils.JsonDeserializeObjectWithDefault<PaymentAsiaRMBOptions>(json);
}