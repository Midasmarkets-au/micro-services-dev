using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.FivePayVA;

public class FivePayVAOptions
{
    public string EndPoint { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public int MerchantId { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public string CallbackDomain { get; set; } = null!;

    public static FivePayVAOptions FromJson(string json)
        => JsonConvert.DeserializeObject<FivePayVAOptions>(json) ?? new FivePayVAOptions();
}