using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.Poli.Models;

public class PoliOptions
{
    public string MerchantCode { get; set; } = null!;
    public string SecurityCode { get; set; } = null!;
    public string EndPoint { get; set; } = "https://poliapi.apac.paywithpoli.com/api/";
    public string FrontUri { get; set; } = "https://client.au.mybcr.dev";
    public string CallBackUri { get; set; } = "https://pro.t.api.mybcr.dev/api/v1/payment/callback/poli";


    public static PoliOptions FromJson(string json) => JsonConvert.DeserializeObject<PoliOptions>(json)!; 
}