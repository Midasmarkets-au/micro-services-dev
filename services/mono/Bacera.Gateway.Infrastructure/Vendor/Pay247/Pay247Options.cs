namespace Bacera.Gateway.Vendor.Pay247;

public class Pay247Options
{
    public string MerchantId { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string CallbackUrl { get; set; } = null!;
    public string Version { get; set; } = null!;
    public string PayMethod { get; set; } = null!;
    public string EndPointBase { get; set; } = null!;
    public string PayinEndPoint { get; set; } = null!;

    public string GetPayinUrl()
    {
        var baseUrl = EndPointBase.TrimEnd('/');
        var payinUrl = PayinEndPoint.TrimStart('/');
        return $"{baseUrl}/{payinUrl}";
    }
    public static Pay247Options FromJson(string json)
        => Utils.JsonDeserializeObjectWithDefault<Pay247Options>(json);
}