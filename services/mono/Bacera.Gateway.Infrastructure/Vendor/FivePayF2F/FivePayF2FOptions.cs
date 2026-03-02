using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.FivePayF2F;

public class FivePayF2FOptions
{
    public string EndPoint { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public int MerchantId { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public string CallbackDomain { get; set; } = null!;

    public static FivePayF2FOptions FromJson(string json)
        => JsonConvert.DeserializeObject<FivePayF2FOptions>(json) ?? new FivePayF2FOptions();
}